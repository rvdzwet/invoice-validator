import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    open: true,
    // Proxy API requests to the backend during development
    proxy: {
      '/api': {
        target: 'https://localhost:7051',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
