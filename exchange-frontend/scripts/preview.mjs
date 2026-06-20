import { preview } from 'vite';
import { viteOptions } from './viteOptions.mjs';

const server = await preview(viteOptions);

server.printUrls();
