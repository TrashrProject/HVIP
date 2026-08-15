const FALLBACK_ENDPOINT = '/WebPixel/asset-resolver.php?u=';
const IMAGE_EXT = /\.(png|gif|jpe?g|webp|svg|ico)(?:$|\?)/i;

self.addEventListener('install', event => {
  self.skipWaiting();
});

self.addEventListener('activate', event => {
  event.waitUntil(self.clients.claim());
});

self.addEventListener('fetch', event => {
  const request = event.request;
  if (request.method !== 'GET') return;

  const url = new URL(request.url);
  if (url.origin !== self.location.origin) return;
  if (!IMAGE_EXT.test(url.pathname)) return;
  if (url.pathname.includes('/WebPixel/asset-resolver.php')) return;

  event.respondWith((async () => {
    let originalResponse = null;

    try {
      originalResponse = await fetch(request);
      if (originalResponse && originalResponse.ok) return originalResponse;
    } catch (_) {
      // Try the local basename resolver below.
    }

    try {
      const fallback = await fetch(FALLBACK_ENDPOINT + encodeURIComponent(url.pathname), {
        credentials: 'same-origin',
        cache: 'force-cache'
      });
      if (fallback.ok) return fallback;
    } catch (_) {}

    if (originalResponse) return originalResponse;
    return new Response('', { status: 404, statusText: 'Not Found' });
  })());
});
