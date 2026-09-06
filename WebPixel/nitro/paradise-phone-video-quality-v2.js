(() => {
  'use strict';

  if (window.__PARADISE_PHONE_VIDEO_QUALITY_V2__) return;
  window.__PARADISE_PHONE_VIDEO_QUALITY_V2__ = '2.0.0';

  const TARGET_FPS = 15;
  const TARGET_BITRATE = 4_000_000;

  function tuneVideoTrack(track) {
    if (!track || track.kind !== 'video') return;
    try { track.contentHint = 'detail'; } catch {}
  }

  async function tuneSender(sender) {
    if (!sender?.track || sender.track.kind !== 'video') return;
    tuneVideoTrack(sender.track);

    try {
      const params = sender.getParameters?.();
      if (!params) return;

      if (Array.isArray(params.encodings) && params.encodings.length) {
        for (const encoding of params.encodings) {
          encoding.maxBitrate = Math.max(Number(encoding.maxBitrate || 0), TARGET_BITRATE);
          encoding.maxFramerate = Math.max(Number(encoding.maxFramerate || 0), TARGET_FPS);
          encoding.scaleResolutionDownBy = 1;
        }
      }

      if ('degradationPreference' in params) params.degradationPreference = 'maintain-resolution';
      await sender.setParameters?.(params);
    } catch (error) {
      console.debug('[ParadisePhone] video sender quality hint skipped', error);
    }
  }

  /* Stable V2 creates its RTCPeerConnection only when a video call starts.
     Patching addTrack here lets us request detail/bitrate without changing the stable call engine. */
  if (window.RTCPeerConnection?.prototype?.addTrack && !window.RTCPeerConnection.prototype.__paradiseVideoQualityPatched) {
    const proto = window.RTCPeerConnection.prototype;
    const originalAddTrack = proto.addTrack;

    Object.defineProperty(proto, '__paradiseVideoQualityPatched', { value: true, configurable: true });
    proto.addTrack = function(track, ...streams) {
      tuneVideoTrack(track);
      const sender = originalAddTrack.call(this, track, ...streams);
      if (track?.kind === 'video') {
        [0, 500, 1400].forEach(delay => window.setTimeout(() => tuneSender(sender), delay));
      }
      return sender;
    };
  }

  /* The call engine currently requests the Nitro room canvas at 8 FPS.
     Raise only that exact room-capture case to 15 FPS; other canvas consumers are untouched. */
  if (window.HTMLCanvasElement?.prototype?.captureStream && !window.HTMLCanvasElement.prototype.__paradiseRoomCapturePatched) {
    const proto = window.HTMLCanvasElement.prototype;
    const originalCaptureStream = proto.captureStream;

    Object.defineProperty(proto, '__paradiseRoomCapturePatched', { value: true, configurable: true });
    proto.captureStream = function(frameRate) {
      const isPhoneCanvas = !!this.closest?.('.nitro-phone-frame,.paradise-call-layer,.phone-camera-shell');
      const isRoomSized = this.width >= 320 && this.height >= 200;
      const requested = Number(frameRate || 0);
      const effectiveRate = !isPhoneCanvas && isRoomSized && requested > 0 && requested <= 8
        ? TARGET_FPS
        : frameRate;

      const stream = originalCaptureStream.call(this, effectiveRate);
      stream?.getVideoTracks?.().forEach(tuneVideoTrack);
      return stream;
    };
  }

  console.info('[ParadisePhone] video quality V2 active');
})();
