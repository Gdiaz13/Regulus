import { preview } from 'vite';
import config from '../vite.config.mjs';

const server = await preview({
  ...config,
  configFile: false,
});

server.printUrls();
