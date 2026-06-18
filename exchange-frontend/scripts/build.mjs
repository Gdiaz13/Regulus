import { build } from 'vite';
import config from '../vite.config.mjs';

await build({
  ...config,
  configFile: false,
});
