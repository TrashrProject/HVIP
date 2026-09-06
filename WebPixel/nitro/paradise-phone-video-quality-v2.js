(() => {
  'use strict';

  if (window.__PARADISE_PHONE_VIDEO_QUALITY_V2__) return;
  window.__PARADISE_PHONE_VIDEO_QUALITY_V2__ = '2.2.0';

  const TARGET_FPS = 15;
  const TARGET_BITRATE = 10_000_000;
  const MAX_RELAY_WIDTH = 1920;
  const MAX_RELAY_HEIGHT = 1440;
  const peerRegistry = new Set();

  function tuneVideoTrack(track) {
    if (!track || track.kind !== 'video') return;
    try { track.contentHint = 'detail'; } catch {}
    try {
      track.applyConstraints?.({
        frameRate: { ideal: TARGET_FPS, max: TARGET_FPS }
      }).catch?.(() => {});
    } catch {}
  }

  async function tuneSender(sender) {
    if (!sender?.track || sender.track.kind !== 'video') return;
    tuneVideoTrack(sender.track);

    try {
      const params = sender.getParameters?.();
      if (!params) return;

      if (!Array.isArray(params.encodings) || !params.encodings.length) params.encodings = [{}];

      for (const encoding of params.encodings) {
        encoding.maxBitrate = TARGET_BITRATE;
        encoding.maxFramerate = TARGET_FPS;
        encoding.scaleResolutionDownBy = 1;
        if ('priority' in encoding) encoding.priority = 'high';
        if ('networkPriority' in encoding) encoding.networkPriority = 'high';
      }

      if ('degradationPreference' in params) params.degradationPreference = 'maintain-resolution';
      await sender.setParameters?.(params);
    } catch (error) {
      console.debug('[ParadisePhone] video sender quality hint skipped', error);
    }
  }

  function relayScaleFor(source) {
    if (!source?.width || !source?.height) return 1;
    return source.width * 2 <= MAX_RELAY_WIDTH && source.height * 2 <= MAX_RELAY_HEIGHT ? 2 : 1;
  }

  function createSharpRelayStream(source, originalCaptureStream) {
    const scale = relayScaleFor(source);
    const relay = document.createElement('canvas');
    relay.width = Math.max(1, Math.floor(source.width * scale));
    relay.height = Math.max(1, Math.floor(source.height * scale));

    const ctx = relay.getContext('2d', {
      alpha: false,
      desynchronized: true,
      willReadFrequently: false
    });

    if (!ctx) return originalCaptureStream.call(source, TARGET_FPS);

    ctx.imageSmoothingEnabled = false;
    ctx.imageSmoothingQuality = 'low';

    /* Manual frame delivery is more reliable for an off-DOM relay canvas.
       With captureStream(fps), Chromium can occasionally negotiate the call while
       the answering side still receives a permanently black first frame. */
    let stream = originalCaptureStream.call(relay, 0);
    let tracks = stream?.getVideoTracks?.() || [];
    let manualFrames = tracks.length > 0 && tracks.every(track => typeof track.requestFrame === 'function');

    if (!manualFrames) {
      try { tracks.forEach(track => track.stop()); } catch {}
      stream = originalCaptureStream.call(relay, TARGET_FPS);
      tracks = stream?.getVideoTracks?.() || [];
    }

    let stopped = false;
    let timer = 0;

    const pushFrame = () => {
      if (!manualFrames) return;
      tracks.forEach(track => {
        try { track.requestFrame(); } catch {}
      });
    };

    const draw = () => {
      if (stopped) return;
      try {
        ctx.imageSmoothingEnabled = false;
        ctx.clearRect(0, 0, relay.width, relay.height);
        ctx.drawImage(source, 0, 0, source.width, source.height, 0, 0, relay.width, relay.height);
        pushFrame();
      } catch {}
    };

    /* Send several immediate frames so both the caller and the user answering
       have a real room frame available before/while SDP negotiation completes. */
    draw();
    requestAnimationFrame(draw);
    window.setTimeout(draw, 60);
    window.setTimeout(draw, 180);
    timer = window.setInterval(draw, Math.max(16, Math.round(1000 / TARGET_FPS)));

    const cleanup = () => {
      if (stopped) return;
      stopped = true;
      clearInterval(timer);
      timer = 0;
    };

    tracks.forEach(track => {
      tuneVideoTrack(track);
      const nativeStop = track.stop?.bind(track);
      if (nativeStop) {
        track.stop = () => {
          cleanup();
          nativeStop();
        };
      }
      track.addEventListener?.('ended', cleanup, { once: true });
    });

    console.info('[ParadisePhone] sharp room relay', {
      source: `${source.width}x${source.height}`,
      relay: `${relay.width}x${relay.height}`,
      fps: TARGET_FPS,
      bitrate: TARGET_BITRATE,
      manualFrames
    });

    return stream;
  }

  function ensureRemoteTrackTap(pc) {
    if (!pc || pc.__paradiseRemoteTrackTap) return;

    const remoteStream = new MediaStream();
    Object.defineProperty(pc, '__paradiseRemoteTrackTap', {
      value: remoteStream,
      configurable: true
    });
    peerRegistry.add(pc);

    pc.addEventListener('track', event => {
      const incomingTracks = event.streams?.[0]?.getTracks?.() || [event.track];
      incomingTracks.forEach(track => {
        if (!track || remoteStream.getTracks().some(existing => existing.id === track.id)) return;
        remoteStream.addTrack(track);
        if (track.kind === 'video') {
          tuneVideoTrack(track);
          track.addEventListener?.('unmute', bindRemoteVideo, { once: true });
        }
      });
      [0, 40, 120, 350, 900].forEach(delay => window.setTimeout(bindRemoteVideo, delay));
    });

    pc.addEventListener('connectionstatechange', () => {
      if (pc.connectionState === 'closed' || pc.connectionState === 'failed') peerRegistry.delete(pc);
      if (pc.connectionState === 'connected') [0, 80, 250].forEach(delay => window.setTimeout(bindRemoteVideo, delay));
    });
  }

  function bindRemoteVideo() {
    const video = document.querySelector('[data-pcall-remote]');
    if (!(video instanceof HTMLVideoElement)) return;

    const peers = [...peerRegistry].reverse();
    const peer = peers.find(candidate => {
      const stream = candidate?.__paradiseRemoteTrackTap;
      return stream?.getVideoTracks?.().some(track => track.readyState === 'live');
    });
    const stream = peer?.__paradiseRemoteTrackTap;
    if (!stream?.getVideoTracks?.().length) return;

    const currentIds = video.srcObject?.getVideoTracks?.().map(track => track.id).join(',') || '';
    const nextIds = stream.getVideoTracks().map(track => track.id).join(',');
    if (!currentIds || currentIds !== nextIds) video.srcObject = stream;

    video.autoplay = true;
    video.playsInline = true;
    video.muted = true;
    video.play?.().catch(() => {});
  }

  /* Stable V2 creates its RTCPeerConnection only when a video call starts.
     Patch addTrack before the engine loads so the sender always favours detail/resolution. */
  if (window.RTCPeerConnection?.prototype && !window.RTCPeerConnection.prototype.__paradiseVideoQualityPatched) {
    const proto = window.RTCPeerConnection.prototype;
    const originalAddTrack = proto.addTrack;
    const originalSetRemoteDescription = proto.setRemoteDescription;

    Object.defineProperty(proto, '__paradiseVideoQualityPatched', { value: true, configurable: true });

    proto.addTrack = function(track, ...streams) {
      tuneVideoTrack(track);
      const sender = originalAddTrack.call(this, track, ...streams);
      if (track?.kind === 'video') {
        [0, 120, 500, 1400, 3000].forEach(delay => window.setTimeout(() => tuneSender(sender), delay));
      }
      return sender;
    };

    /* Register the receiver BEFORE native setRemoteDescription fires track events.
       This closes the race that only affected the user who answered the video call. */
    proto.setRemoteDescription = function(description) {
      ensureRemoteTrackTap(this);
      return originalSetRemoteDescription.call(this, description);
    };
  }

  /* Stable V2 requests the Nitro room canvas at 8 FPS.
     For that exact room source, relay it through a nearest-neighbour high-resolution canvas. */
  if (window.HTMLCanvasElement?.prototype?.captureStream && !window.HTMLCanvasElement.prototype.__paradiseRoomCapturePatched) {
    const proto = window.HTMLCanvasElement.prototype;
    const originalCaptureStream = proto.captureStream;

    Object.defineProperty(proto, '__paradiseRoomCapturePatched', { value: true, configurable: true });
    proto.captureStream = function(frameRate) {
      const isPhoneCanvas = !!this.closest?.('.nitro-phone-frame,.paradise-call-layer,.phone-camera-shell');
      const isRoomSized = this.width >= 320 && this.height >= 200;
      const requested = Number(frameRate || 0);
      const isStableRoomCapture = !isPhoneCanvas && isRoomSized && requested > 0 && requested <= 8;

      if (isStableRoomCapture) return createSharpRelayStream(this, originalCaptureStream);

      const stream = originalCaptureStream.call(this, frameRate);
      stream?.getVideoTracks?.().forEach(tuneVideoTrack);
      return stream;
    };
  }

  /* Lightweight receiver rescue. No DOM observer and no full-page scan. */
  window.setInterval(bindRemoteVideo, 250);

  console.info('[ParadisePhone] video quality V2.2 active');
})();
