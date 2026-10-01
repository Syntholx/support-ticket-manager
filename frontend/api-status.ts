export { ApiStatus, parseApiStatus };

type ApiStatus = {
  name: string;
  isRunning: boolean;
  version: string;
};
function parseApiStatus(data: unknown): ApiStatus {
  if (typeof data !== "object" || data === null) {
    throw new Error("Odpowiedź API nie jest obiektem");
  }
  if (!("isRunning" in data) || typeof data.isRunning !== "boolean") {
    throw new Error("Brak isRunning lub nieprawidłowy typ");
  }
  if (!("name" in data) || typeof data.name !== "string") {
    throw new Error("Brak name lub nieprawidłowy typ");
  }
  if (!data.name.trim()) {
    throw new Error("Nazwa API nie może być pusta");
  }
  if (!("version" in data) || typeof data.version !== "string") {
    throw new Error("brak version lub nieprawidłowy typ");
  }
  if (!data.version.trim()) {
    throw new Error("Wersja API nie może być pusta");
  }

  return {
    name: data.name,
    isRunning: data.isRunning,
    version: data.version,
  };
}
