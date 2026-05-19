import path from 'node:path'
import { fileURLToPath, URL } from 'node:url'
import react from '@vitejs/plugin-react'
import { loadEnv } from 'vite'
import { defineConfig } from 'vitest/config'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const port = Number(env.VITE_PORT || '5173')
  const usePolling = env.CHOKIDAR_USEPOLLING === 'true'
  const hmrHost = env.VITE_HMR_HOST

  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@': path.resolve(fileURLToPath(new URL('.', import.meta.url)), './src'),
      },
    },
    server: {
      host: env.VITE_HOST || '0.0.0.0',
      port,
      strictPort: true,
      watch: {
        usePolling,
        interval: Number(env.CHOKIDAR_INTERVAL || '300'),
      },
      hmr: hmrHost
        ? {
            host: hmrHost,
            port: Number(env.VITE_HMR_PORT || String(port)),
          }
        : undefined,
    },
    test: {
      environment: 'jsdom',
      setupFiles: './src/app/test/setup.ts',
      include: ['src/**/*.{test,spec}.{ts,tsx}'],
      fileParallelism: false,
      globals: false,
      css: true,
    },
  }
})
