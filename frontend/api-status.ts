export { ApiStatus, parseApiStatus };

type ApiStatus = {
  name: string;
  isRunning: boolean;
  version: string;
};
function parseApiStatus(data: unknown): ApiStatus {
  if (typeof data !== "object" || data === null) {
    throw new Error("The API response is not an object");
  }
  if (!("isRunning" in data) || typeof data.isRunning !== "boolean") {
    throw new Error("Missing or invalid isRunning field");
  }
  if (!("name" in data) || typeof data.name !== "string") {
    throw new Error("Missing or invalid name field");
  }
  if (!data.name.trim()) {
    throw new Error("API name cannot be empty");
  }
  if (!("version" in data) || typeof data.version !== "string") {
    throw new Error("Missing or invalid version field");
  }
  if (!data.version.trim()) {
    throw new Error("API version cannot be empty");
  }

  return {
    name: data.name,
    isRunning: data.isRunning,
    version: data.version,
  };
}
