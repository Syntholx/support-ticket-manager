import { readFileSync } from "node:fs";
import { join } from "node:path";
import { defineConfig } from "vite";
import tailwindcss from "@tailwindcss/vite";
export default defineConfig(({ command }) => {
  const localAppData = process.env.LOCALAPPDATA;
  if (command === "serve" && !localAppData) {
    throw new Error("LOCALAPPDATA is required for the local HTTPS certificate.");
  }
  const certDirectory = join(localAppData ?? "", "TSMDevCert");
  return {
  plugins: [tailwindcss()],
  server: {
    https: command === "serve" ? {
      cert: readFileSync(join(certDirectory, "localhost.pem")),
      key: readFileSync(join(certDirectory, "localhost.key")),
    } : undefined,

    proxy: {
      "/api/": {
        target: "https://localhost:7280",
        changeOrigin: true,
      },
    },
  },
  };
});
