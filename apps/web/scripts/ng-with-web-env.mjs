import { spawn } from 'node:child_process';
import { config } from 'dotenv';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.join(path.dirname(fileURLToPath(import.meta.url)), '..');
config({ path: path.join(root, '.env') });

const envBase = process.env.API_BASE_URL?.trim();
const defineArgs =
  envBase !== undefined && envBase !== ''
    ? ['--define', `__WEB_API_BASE_URL__=${JSON.stringify(envBase)}`]
    : [];

const ngCli = path.join(root, 'node_modules', '@angular', 'cli', 'bin', 'ng.js');
const ngArgs = [...process.argv.slice(2), ...defineArgs];

const child = spawn(process.execPath, [ngCli, ...ngArgs], {
  cwd: root,
  stdio: 'inherit',
  env: process.env,
});

child.on('exit', (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal);
    return;
  }
  process.exit(code ?? 1);
});
