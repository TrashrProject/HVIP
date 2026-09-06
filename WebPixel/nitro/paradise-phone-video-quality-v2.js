(() => {
  'use strict';

  if (window.__PARADISE_PHONE_VIDEO_QUALITY_V2__) return;
  window.__PARADISE_PHONE_VIDEO_QUALITY_V2__ = '2.4.0';

  const TARGET_FPS = 15;
  const TARGET_BITRATE = 12_000_000;
  const RECOVERY_MS = 6000;

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

  /*
   * IMPORTANT:
   * Do not relay/copy the Nitro WebGL room canvas through a 2D canvas.
   * With Chromium + WebGL that copy can be black even though canvas.captureStream()
   * on the renderer itself works perfectly. The previous high-resolution relay was
   * therefore asymmetric: the caller could receive video while the callee saw black.
   *
   * We now keep the native renderer stream on BOTH sides. The room canvas is already
   * large (for example 1920x945), which is far above the phone viewport resolution.
   * Quality is obtained by preserving that native source and tuning WebRTC bitrate,
   * rather than by upscaling/copying the WebGL canvas.
   */
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
        const stream = originalCaptureStream.call(this, TARGET_FPS);
        stream?.getVideoTracks?.().forEach(tuneVideoTrack);
        console.info('[ParadisePhone] native room capture', {
          source: `${this.width}x${this.height}`,
          fps: TARGET_FPS,
          bitrate: TARGET_BITRATE
        });
        return stream;
      }

      const stream = originalCaptureStream.call(this, frameRate);
      stream?.getVideoTracks?.().forEach(tuneVideoTrack);
      return stream;
    };
  }

  /* Recovery stream for Chromium cases where the remote track arrives before the
     call UI is mounted. This does not create another video source; it only keeps a
     reference to the already received WebRTC track and rebinds it if necessary. */
  const recoveredRemote = new MediaStream();
  let recoveryUntil = 0;
  let recoveryTimer = 0;

  function pruneRecoveredTracks() {
    for (const track of recoveredRemote.getTracks()) {
      if (track.readyState !== 'live') {
        try { recoveredRemote.removeTrack(track); } catch {}
      }
    }
  }

  function addRecoveredTrack(track) {
    if (!track || track.kind !== 'video') return;
    tuneVideoTrack(track);
    pruneRecoveredTracks();
    if (!recoveredRemote.getTracks().some(existing => existing.id === track.id)) {
      recoveredRemote.addTrack(track);
    }
    recoveryUntil = Math.max(recoveryUntil, Date.now() + RECOVERY_MS);
    startRecoveryBinding();
  }

  function bindRecoveredRemote() {
    pruneRecoveredTracks();
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
          [0, 120, 300, 700, 1400, 2500, 4500].forEach(delay => window.setTimeout(() => {
            try { this.getReceivers?.().forEach(receiver => addRecoveredTrack(receiver.track)); } catch {}
            bindRecoveredRemote();
          }, delay));
        } catch {}
        return result;
      };
    }
  }

  console.info('[ParadisePhone] video quality V2.4 active — native WebGL capture both sides');
})();