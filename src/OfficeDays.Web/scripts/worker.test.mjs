import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
import { runInNewContext } from 'node:vm';

const template = await readFile(new URL('./worker.js', import.meta.url), 'utf8');
const origin = 'https://office.example';
const assets = { '/index.html': 'shell-digest', '/_app/immutable/guest.js': 'guest-digest' };

function release(version, storage, download) {
  const handlers = new Map();
  const cacheStorage = {
    keys: () => [...storage.keys()],
    delete: name => storage.delete(name),
    open(name) {
      if (!storage.has(name)) storage.set(name, new Map());
      const files = storage.get(name);
      return {
        keys: () => [...files.keys()].map(path => new Request(origin + path)),
        match: path => files.get(path),
        async addAll(requests) {
          const entries = await Promise.all(requests.map(async request => {
            const path = new URL(request.url).pathname;
            assert.equal(request.integrity, assets[path]);
            assert.equal(request.credentials, 'omit');
            return [path, await download(path)];
          }));
          for (const [path, response] of entries) files.set(path, response);
        }
      };
    }
  };
  runInNewContext(template.replace('__BUILD_VERSION__', version)
    .replace('/* ASSET_INTEGRITY */ {}', JSON.stringify(assets)), {
    self: { location: new URL(origin), clients: { claim() {} }, addEventListener: (name, handler) => handlers.set(name, handler) },
    caches: cacheStorage, URL, Response,
    Request: class extends Request {
      constructor(path, options) { super(new URL(path, origin), options); }
    }
  });
  return (name, event = {}) => {
    let completion;
    const capture = promise => { completion = promise; };
    handlers.get(name)({ ...event, waitUntil: capture, respondWith: capture });
    return completion;
  };
}

test('Should_KeepPreviousReleaseUntilActivation_WhenDownloadIsInterruptedAndRetried', async () => {
  // Arrange
  const storage = new Map([['unrelated', new Map()]]);
  const old = release('old', storage, () => 'old content');
  await old('install');
  await old('activate');
  let interrupted = true;
  const next = release('next', storage, path => {
    if (interrupted && path.endsWith('.js')) throw new Error('Disconnected');
    return 'new content';
  });
  const request = { method: 'GET', mode: 'navigate', url: origin + '/guest' };

  // Act and assert
  await assert.rejects(next('install'), /Disconnected/);
  assert.equal(await old('fetch', { request }), 'old content');
  assert.equal(storage.get('office-days-shell-next').size, 0);
  interrupted = false;
  await next('install');
  assert.equal(await old('fetch', { request }), 'old content');
  await next('activate');
  assert.equal(await next('fetch', { request }), 'new content');
  assert.deepEqual([...storage.keys()], ['unrelated', 'office-days-shell-next']);
});

test('Should_BypassWorker_WhenRequestIsPrivateOrOutsideApp', () => {
  // Arrange
  const worker = release('test', new Map(), () => assert.fail('Unexpected download'));

  // Act and assert
  for (const [method, url] of [
    ['GET', origin + '/api/auth/me'], ['GET', origin + '/api/users/me/export'],
    ['POST', origin + '/api/attendance'], ['GET', origin + '/health/ready'],
    ['GET', 'https://other.example/guest']
  ]) {
    assert.equal(worker('fetch', { request: { url, method, mode: 'navigate' } }), undefined);
  }
});
