// The static adapter writes its fallback HTML after Vite emits the chunks.
// Pin the finished shell and every public dependency to their exact bytes.
import { createHash } from 'node:crypto';
import { relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { readFile, readdir, writeFile } from 'node:fs/promises';

const root = new URL('../build/', import.meta.url);
const paths = ['index.html', 'manifest.webmanifest'];

for (const directory of ['_app/immutable', 'icons']) {
  for (const entry of await readdir(new URL(directory, root), { recursive: true, withFileTypes: true })) {
    if (entry.isFile()) {
      paths.push(relative(fileURLToPath(root), entry.parentPath).split('\\').join('/') + '/' + entry.name);
    }
  }
}

const assets = {};

for (const path of paths.sort()) {
  const bytes = await readFile(new URL(path, root));
  assets['/' + path] = 'sha256-' + createHash('sha256').update(bytes).digest('base64');
}

const template = await readFile(new URL('./worker.js', import.meta.url), 'utf8');
const version = createHash('sha256')
                   .update(template)
                   .update(JSON.stringify(assets))
                   .digest('hex');

await writeFile(new URL('service-worker.js', root), 
                template.replace('__BUILD_VERSION__', version)
                        .replace('/* ASSET_INTEGRITY */ {}', JSON.stringify(assets)));
