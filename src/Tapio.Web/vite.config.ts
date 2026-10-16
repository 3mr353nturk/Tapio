import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    allowedHosts: ['.ngrok-free.app', '.ngrok.io', '.ngrok.app'],
    watch: {
      // Windows can EBUSY-lock large static JSON during file watching
      ignored: ['**/turkiyeIlIlce.json']
    },
    proxy: {
      '/api': {
        target: 'http://localhost:5045',
        changeOrigin: true
      },
      '/uploads': {
        target: 'http://localhost:5045',
        changeOrigin: true
      }
    }
  }
});
