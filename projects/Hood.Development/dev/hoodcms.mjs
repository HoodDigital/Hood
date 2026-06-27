#!/usr/bin/env node
/**
 * hoodcms/dev — the `hoodcms` bin.
 *
 * Registers tsx's ESM loader in-process, then imports the TypeScript CLI. Running in a single
 * process (rather than spawning a tsx child) keeps Ctrl+C handling intact for the `watch`
 * parallel-watch teardown. tsx is a dependency of hoodcms, so consumers never install it.
 */
import { register } from 'tsx/esm/api';

register();

await import(new URL('./cli.ts', import.meta.url).href);
