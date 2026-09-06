(() => {
  'use strict';

  if (window.__PARADISE_PHONE_CALL_API_V2_BRIDGE__) return;
  window.__PARADISE_PHONE_CALL_API_V2_BRIDGE__ = '1.0.0';

  const originalFetch = window.fetch.bind(window);
  const OLD_PATH = '/nitro/phone-call-api.php';
  const NEW_PATH = '/nitro/phone-call-api-v2.php';

  window.fetch = function paradisePhoneCallFetch(input, init) {
    try {
      if (typeof input === 'string' && input.includes(OLD_PATH)) {
        input = input.replace(OLD_PATH, NEW_PATH);
      }
    } catch {}

    return originalFetch(input, init);
  };

  console.info('[ParadisePhone] call API V2 guard active');
})();
