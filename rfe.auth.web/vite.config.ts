import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tsconfigPaths from 'vite-tsconfig-paths'
import fs from 'fs'
import path from 'path'

const certPath = path.resolve(__dirname, '../certs/localhost+2.pem')
const keyPath = path.resolve(__dirname, '../certs/localhost+2-key.pem')
const httpsConfig = fs.existsSync(certPath) && fs.existsSync(keyPath)
  ? {
      key: fs.readFileSync(keyPath),
      cert: fs.readFileSync(certPath),
    }
  : undefined

export default defineConfig({
  plugins: [react(), tsconfigPaths()],
  server: {
    port: 5173,
    https: httpsConfig,
    proxy: {
      '/api': {
        target: 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      },
      '/connect': {
        target: 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
