import { defineConfig } from 'vite';
import angular from '@vitejs/plugin-angular';

export default defineConfig({
  plugins: [angular()],
  server: {
    middlewareMode: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5164',
        changeOrigin: true,
        pathRewrite: {
          '^/api': '/api'
        },
      },
    },
  },
  preview: {
    proxy: {
      '/api': {
        target: 'http://localhost:5164',
        changeOrigin: true,
        pathRewrite: {
          '^/api': '/api'
        },
      },
    },
  },
});
