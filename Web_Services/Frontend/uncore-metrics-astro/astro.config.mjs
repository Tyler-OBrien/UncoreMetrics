import { defineConfig } from 'astro/config';
import preact from "@astrojs/preact";
import cloudflare from "@astrojs/cloudflare";
import tailwindcss from "@tailwindcss/vite";

// https://astro.build/config
export default defineConfig({
  integrations: [preact({
    compat: true
  })],
  prefetch: {
    prefetchAll: true
  },
  vite: {
    plugins: [tailwindcss()],
    // Plotly has to be bundled rather than left external for the Workers build
    ssr: {
      noExternal: ['plotly.js-basic-dist-min']
    },
    build: {
      noExternal: ['plotly.js-basic-dist-min']
    }
  },
  output: "server",
  // No sessions or image transforms are used, so don't ask for a KV namespace or an Images binding
  session: false,
  adapter: cloudflare({ imageService: "passthrough" })
});
