/**
 * hoodcms/dev — CLI entry (HOOD-131).
 *
 * Loaded under tsx by the `hoodcms` bin, so the consumer's `hood.dev.ts` and any custom
 * targets compile on the fly. Discovers config, dispatches the command, and maps the result
 * to an exit code.
 */
import { runCli } from './index';

try {
    process.exitCode = await runCli();
} catch (err) {
    console.error(`\nhoodcms: ${err instanceof Error ? err.message : String(err)}`);
    process.exitCode = 1;
}
