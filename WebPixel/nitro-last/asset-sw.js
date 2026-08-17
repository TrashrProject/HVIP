const FALLBACK_ENDPOINT = '/WebPixel/asset-resolver.php?u=';
const IMAGE_EXT = /\.(png|gif|jpe?g|webp|svg|ico)(?:$|\?)/i;
const STYLE_EXT = /\.css(?:$|\?)/i;
const FONT_EXT = /\.(ttf|otf|woff2?|eot)(?:$|\?)/i;

const TRANSPARENT_PNG = Uint8Array.from(atob('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8WQAAAABJRU5ErkJggg=='), c => c.charCodeAt(0));

self.addEventListener('install', event => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));

function transparentImageResponse() {
  return new Response(TRANSPARENT_PNG, {
    status: 200,
    headers: {
      'Content-Type': 'image/png',
      'Cache-Control': 'public, max-age=3600',
      'X-HVIP-Placeholder': '1'
    }
  });
}

function emptyStyleResponse() {
  return new Response('/* optional legacy stylesheet unavailable */', {
    status: 200,
    headers: {
      'Content-Type': 'text/css; charset=utf-8',
      'Cache-Control': 'public, max-age=3600',
      'X-HVIP-Placeholder': '1'
    }
  });
}

self.addEventListener('fetch', event => {
  const request = event.request;
  if (request.method !== 'GET') return;

  const url = new URL(request.url);
  if (url.origin !== self.location.origin) return;
  if (url.pathname.includes('/WebPixel/asset-resolver.php')) return;

  const isAvatarFallback = /\/avatar-(?:image|placeholder)\.php$/i.test(url.pathname);
  const isImage = IMAGE_EXT.test(url.pathname) || isAvatarFallback;
  const isStyle = STYLE_EXT.test(url.pathname);
  const isFont = FONT_EXT.test(url.pathname);

  if (!isImage && !isStyle && !isFont) return;

  event.respondWith((async () => {
    let originalResponse = null;

    try {
      originalResponse = await fetch(request);
      if (originalResponse && originalResponse.ok) return originalResponse;
    } catch (_) {}

    if (isImage) {
      if (!isAvatarFallback) {
        try {
          const fallback = await fetch(FALLBACK_ENDPOINT + encodeURIComponent(url.pathname), {
            credentials: 'same-origin',
            cache: 'force-cache'
          });
          if (fallback.ok) return fallback;
        } catch (_) {}
      }
      return transparentImageResponse();
    }

    if (isStyle) return emptyStyleResponse();

    // Les vieilles fontes Nitro optionnelles ne doivent plus générer de 404.
    if (isFont) return new Response('', {
      status: 204,
      headers: { 'Cache-Control': 'public, max-age=3600' }
    });

    return originalResponse || new Response('', { status: 204 });
  })());
});
