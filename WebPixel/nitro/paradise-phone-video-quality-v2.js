(() => {
  'use strict';

  if (window.__PARADISE_PHONE_VIDEO_QUALITY_V2__) return;
  window.__PARADISE_PHONE_VIDEO_QUALITY_V2__ = '2.3.0';

  const TARGET_FPS = 15;
  const TARGET_BITRATE = 10_000_000;
  const MAX_RELAY_WIDTH = 1920;
  const MAX_RELAY_HEIGHT = 1440;
  const RECOVERY_MS = 5000;

  function tuneVideoTrack(track) {
    if (!track || track.kind !== 'video') return;
    try { track.contentHint = 'detail'; } catch {}
    try {
      track.applyConstraints?.({ frameRate: { ideal: TARGET_FPS, max: TARGET_FPS } }).catch?.(() => {});
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

  function isAnsweringIncomingCall() {
    const status = document.querySelector('.paradise-call-stable-v2 .pcall-status');
    return /décrochage/i.test(status?.textContent || '');
  }

  function createSharpRelayStream(source, originalCaptureStream) {
    const scale = relayScaleFor(source);
    const relay = document.createElement('canvas');
    relay.width = Math.max(1, Math.floor(source.width * scale));
    relay.height = Math.max(1, Math.floor(source.height * scale));

    const ctx = relay.getContext('2d', { alpha: false, desynchronized: true, willReadFrequently: false });
    if (!ctx) return originalCaptureStream.call(source, TARGET_FPS);

    ctx.imageSmoothingEnabled = false;
    ctx.imageSmoothingQuality = 'low';

    let stopped = false;
    let timer = 0;
    const draw = () => {
      if (stopped) return;
      try {
        ctx.imageSmoothingEnabled = false;
        ctx.drawImage(source, 0, 0, source.width, source.height, 0, 0, relay.width, relay.height);
      } catch {}
    };

    draw();
    timer = window.setInterval(draw, Math.max(16, Math.round(1000 / TARGET_FPS)));

    const stream = originalCaptureStream.call(relay, TARGET_FPS);
    const tracks = stream?.getVideoTracks?.() || [];
    const cleanup = () => {
      if (stopped) return;
      stopped = true;
      clearInterval(timer);
      timer = 0;
    };

    tracks.forEach(track => {
      tuneVideoTrack(track);
      const nativeStop = track.stop?.bind(track);
      if (nativeStop) track.stop = () => { cleanup(); nativeStop(); };
      track.addEventListener?.('ended', cleanup, { once: true });
    });

    console.info('[ParadisePhone] sharp room relay', {
      source: `${source.width}x${source.height}`,
      relay: `${relay.width}x${relay.height}`,
      fps: TARGET_FPS,
      bitrate: TARGET_BITRATE
    });
    return stream;
  }

  /* Keep a second recovery stream for received tracks. Stable V2 owns its own stream,
     but on the answering side Chromium can fire ontrack before the call UI/state is mounted.
     This stream guarantees that those early tracks are not lost visually. */
  const recoveredRemote = new MediaStream();
  let recoveryUntil = 0;
  let recoveryTimer = 0;

  function addRecoveredTrack(track) {
    if (!track || track.kind !== 'video') return;
    tuneVideoTrack(track);
    if (!recoveredRemote.getTracks().some(existing => existing.id === track.id)) {
      recoveredRemote.addTrack(track);
    }
    recoveryUntil = Math.max(recoveryUntil, Date.now() + RECOVERY_MS);
    startRecoveryBinding();
  }

  function bindRecoveredRemote() {
    const video = document.querySelector('.paradise-call-stable-v2 video[data-pcall-remote]');
    const live = recoveredRemote.getVideoTracks().filter(track => track.readyState === 'live');
    if (!video || !live.length) return false;

    const current = video.srcObject;
    const currentLive = current?.getVideoTracks?.().some(track => track.readyState === 'live');
    const currentReady = video.readyState >= 2 && video.videoWidth > 0 && video.videoHeight > 0;

    if (!currentLive || !currentReady) {
      video.srcObject = recoveredRemote;
      video.muted = true;
      video.playsInline = true;
      video.play?.().catch(() => {});
    }
    return true;
  }

  function startRecoveryBinding() {
    if (recoveryTimer) return;
    recoveryTimer = window.setInterval(() => {
      bindRecoveredRemote();
      if (Date.now() > recoveryUntil) {
        clearInterval(recoveryTimer);
        recoveryTimer = 0;
      }
    }, 100);
  }

  if (window.RTCPeerConnection?.prototype) {
    const proto = window.RTCPeerConnection.prototype;

    if (proto.addTrack && !proto.__paradiseVideoQualityPatched) {
      const originalAddTrack = proto.addTrack;
      Object.defineProperty(proto, '__paradiseVideoQualityPatched', { value: true, configurable: true });
      proto.addTrack = function(track, ...streams) {
        tuneVideoTrack(track);
        const sender = originalAddTrack.call(this, track, ...streams);
        if (track?.kind === 'video') {
          [0, 120, 500, 1400, 3000].forEach(delay => window.setTimeout(() => tuneSender(sender), delay));
        }
        return sender;
      };
    }

    if (proto.setRemoteDescription && !proto.__paradiseIncomingTrackRecoveryPatched) {
      const originalSetRemoteDescription = proto.setRemoteDescription;
      Object.defineProperty(proto, '__paradiseIncomingTrackRecoveryPatched', { value: true, configurable: true });

      proto.setRemoteDescription = async function(description) {
        if (!this.__paradiseRecoveryListener) {
          this.__paradiseRecoveryListener = true;
          this.addEventListener('track', event => {
            addRecoveredTrack(event.track);
            const streamTracks = event.streams?.[0]?.getVideoTracks?.() || [];
            streamTracks.forEach(addRecoveredTrack);
          });
        }

        const result = await originalSetRemoteDescription.call(this, description);
        try {
          this.getReceivers?.().forEach(receiver => addRecoveredTrack(receiver.track));
          [0, 120, 300, 700, 1400, 2500].forEach(delay => window.setTimeout(() => {
            try { this.getReceivers?.().forEach(receiver => addRecoveredTrack(receiver.track)); } catch {}
            bindRecoveredRemote();
          }, delay));
        } catch {}
        return result;
      };
    }
  }

  if (window.HTMLCanvasElement?.prototype?.captureStream && !window.HTMLCanvasElement.prototype.__paradiseRoomCapturePatched) {
    const proto = window.HTMLCanvasElement.prototype;
    const originalCaptureStream = proto.captureStream;
    Object.defineProperty(proto, '__paradiseRoomCapturePatched', { value: true, configurable: true });

    proto.captureStream = function(frameRate) {
      const isPhoneCanvas = !!this.closest?.('.nitro-phone-frame,.paradise-call-layer,.phone-camera-shell');
      const isRoomSized = this.width >= 320 && this.height >= 200;
      const requested = Number(frameRate || 0);
      const isStableRoomCapture = !isPhoneCanvas && isRoomSized && requested > 0 && requested <= 8;

      if (isStableRoomCapture) {
        /* On the callee/answering side use the native renderer stream. Copying a live WebGL
           canvas into a 2D relay can produce black frames on some Chromium/WebGL contexts. */
        if (isAnsweringIncomingCall()) {
          const direct = originalCaptureStream.call(this, TARGET_FPS);
          direct?.getVideoTracks?.().forEach(tuneVideoTrack);
          console.info('[ParadisePhone] native room capture for answered call', {
            source: `${this.width}x${this.height}`,
            fps: TARGET_FPS
          });
          return direct;
        }
        return createSharpRelayStream(this, originalCaptureStream);
      }

      const stream = originalCaptureStream.call(this, frameRate);
      stream?.getVideoTracks?.().forEach(tuneVideoTrack);
      return stream;
    };
  }

  console.info('[ParadisePhone] video quality V2.3 active');
})();