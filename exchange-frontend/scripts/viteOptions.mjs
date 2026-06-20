import { fileURLToPath } from 'node:url';

export const root = fileURLToPath(new URL('..', import.meta.url));
export const viteOptions = { configLoader: 'runner', root };
