import { createServer } from 'vite';
import config from '../vite.config.mjs';

const server = await createServer({
  ...config,
  configFile: false,
});

await server.listen();
server.printUrls();
