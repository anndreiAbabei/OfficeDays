const prefix = 'office-days-shell-';
const cacheName = prefix + '__BUILD_VERSION__';
const integrity = /* ASSET_INTEGRITY */ {};
const shell = '/index.html';

const routes = new Set(['/', '/index.html', '/dashboard', '/guest', '/login', '/register', '/profile', '/tokens', '/reports', '/admin']);

function assetRequest(path) {
  return new Request(path, { integrity: integrity[path], credentials: 'omit', cache: 'reload', redirect: 'error' });
}

async function missingAssets(cache) {
  const present = new Set((await cache.keys()).map(request => new URL(request.url).pathname));
  return Object.keys(integrity).filter(path => !present.has(path));
}

async function prepare() {
  const cache = await caches.open(cacheName);
  const missing = await missingAssets(cache);
  
  if (missing.length) 
    await cache.addAll(missing.map(assetRequest));
}

// addAll commits atomically. A failed update leaves the previous worker active.
// Let the browser wait for existing clients before retiring their release.
self.addEventListener('install', event => event.waitUntil(prepare()));
self.addEventListener('activate', event => event.waitUntil((async () => {
  for (const name of await caches.keys()) {
    if (name.startsWith(prefix) && name !== cacheName) await caches.delete(name);
  }
  await self.clients.claim();
})()));

self.addEventListener('message', event => {
  if (event.data !== 'prepare-offline' || !event.ports[0]) return;
  event.waitUntil(prepare().then(
    () => event.ports[0].postMessage(true),
    () => event.ports[0].postMessage(false)
  ));
});

async function navigate() {
  try {
    await prepare();
    
    const response = await (await caches.open(cacheName)).match(shell);
    
    if (response) 
      return response;
    
  } catch { 
    /* Reconnect to recover an evicted or incomplete release. */ 
  }
  
  return new Response(
      '<!doctype html><meta name="viewport" content="width=device-width,initial-scale=1"><title>Office Days offline</title><h1>Offline files unavailable</h1><p>Reconnect and retry to restore missing app files. If an update is pending, close all Office Days windows and reopen. Your saved guest data is preserved.</p><a href="">Retry connection</a>', 
      {
        status: 503,
        headers: { 'Content-Type': 'text/html; charset=utf-8' }
      });
}

self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);
  if (request.method !== 'GET' || url.origin !== self.location.origin) return;
  if (request.mode === 'navigate' && routes.has(url.pathname.replace(/\/$/, '') || '/')) {
    event.respondWith(navigate());
  } else if (Object.hasOwn(integrity, url.pathname) && !url.search) {
    event.respondWith((async () => {
      const cache = await caches.open(cacheName);
      return await cache.match(url.pathname) ?? await fetch(assetRequest(url.pathname));
    })());
  }
});
