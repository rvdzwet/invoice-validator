import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  const isProd = mode === 'production';

  return {
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
    build: {
      // Production build settings
      outDir: '../BouwdepotInvoiceValidator/wwwroot', // Output to .NET project's wwwroot
      emptyOutDir: true, // Ensure the output directory is emptied before build
      sourcemap: !isProd,
      minify: isProd ? 'esbuild' : false,
      target: 'es2018',
      cssCodeSplit: true,
      rollupOptions: {
        output: {
          manualChunks: {
            vendor: ['react', 'react-dom'],
            mui: ['@mui/material', '@mui/icons-material', '@mui/lab']
          }
        }
      },
      chunkSizeWarningLimit: 1000,
    },
    // Optimize dependencies
    optimizeDeps: {
      include: [
        'react', 
        'react-dom', 
        '@mui/material', 
        '@mui/icons-material', 
        '@emotion/react', 
        '@emotion/styled'
      ]
    },
  };
});
