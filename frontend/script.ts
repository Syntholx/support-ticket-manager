import { ApiStatus, parseApiStatus } from "./api-status.js";
const checkButton = document.querySelector("#check-connection");
const connectStatus = document.querySelector("#connection-status");
if (checkButton === null) {
  throw new Error("Nie znaleziono przycisku sprawdzania połączenia.");
}
checkButton.addEventListener("click", () => {
  loadAPIStatus();
});

const apiUrl = "https://localhost:7280/api/status";

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

    connectStatus.textContent = formatApiStatus(status);
  } catch (error) {
    connectStatus.textContent = "Nie udało się połączyć lub odczytać danych";
  } finally {
    checkButton.disabled = false;
  }
}

function formatApiStatus({ name, version }: ApiStatus): string {
  return `${name} — wersja ${version}`;
}
