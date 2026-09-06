(() => {
  'use strict';

  if (window.__PARADISE_PHONE_NOTIFICATION_FAST_V3__) return;
  window.__PARADISE_PHONE_NOTIFICATION_FAST_V3__ = '3.2.0';

  // The notification center V2.2 now owns the status-bar bell directly.
  // Keep this legacy loader intentionally passive to avoid two MutationObservers
  // fighting over the same launcher and causing flicker / layout glitches.
  if (window.__PARADISE_PHONE_NOTIFICATION_CENTER_V2__) {
    console.info('[ParadisePhone] notification fast V3.2 passive — bell managed by center V2.2');
    return;
  }
})();