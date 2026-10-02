import { readFileSync } from "node:fs";
import { join } from "node:path";
import { defineConfig } from "vite";

const localAppData = process.env.LOCALAPPDATA;
if (!localAppData) {
  throw new Error("LOCALAPPDATA is required for the local HTTPS certificate.");
}

const certDirectory = join(localAppData, "TSMDevCert");

export default defineConfig({
  server: {
    https: {
      cert: readFileSync(join(certDirectory, "localhost.pem")),
      key: readFileSync(join(certDirectory, "localhost.key")),
    },
    proxy: {
      "/api/": {
        target: "https://localhost:7280",
        changeOrigin: true,
      },
    },
  },
});
