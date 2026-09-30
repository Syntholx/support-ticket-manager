const checkButton = document.querySelector("#check-connection");
const connectStatus = document.querySelector("#connection-status");
if (checkButton === null) {
  throw new Error("Nie znaleziono przycisku sprawdzania połączenia.");
}
checkButton.addEventListener("click", () => {
  loadAPIStatus();
});

const apiUrl = "https://localhost:7280/api/status";
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
async function loadAPIStatus() {
  if (!(checkButton instanceof HTMLButtonElement) || connectStatus === null) {
    throw new Error("Nie znaleziono lub komunikatu statusu");
  }

  checkButton.disabled = true;
  try {
    connectStatus.textContent = "Łączenie z API TSM...";
    const response = await fetch(apiUrl);
    if (!response.ok) {
      connectStatus.textContent = "API zwróciło błąd";
      return;
    }
    const data: unknown = await response.json();
    const status = parseApiStatus(data);

    connectStatus.textContent = `${status.name}  — wersja ${status.version}`;
  } catch (error) {
    connectStatus.textContent = "Nie udało się połączyć lub odczytać danych";
  } finally {
    checkButton.disabled = false;
  }
}
