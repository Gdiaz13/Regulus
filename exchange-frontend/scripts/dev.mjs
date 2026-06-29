import './generate-tokens.mjs';
import { createServer } from 'vite';
import { viteOptions } from './viteOptions.mjs';

const server = await createServer(viteOptions);

await server.listen();
server.printUrls();
