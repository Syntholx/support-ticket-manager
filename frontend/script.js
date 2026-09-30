const checkButton = document.querySelector("#check-connection");
const connectStatus = document.querySelector("#connection-status");
checkButton.addEventListener("click", () => {
  loadAPIStatus();
});

async function loadAPIStatus() {
  checkButton.disabled = true;
  try {
    connectStatus.textContent = "Sprawdzanie połączenia...";
    const response = await fetch("https://localhost:7280/api/status");
    if (!response.ok) {
      connectStatus.textContent = "API zwróciło błąd";
      return;
    }
    const data = await response.json();
    connectStatus.textContent = `${data.name}  — wersja ${data.version}`;
  } catch (error) {
    connectStatus.textContent = "Nie udało się połączyć lub odczytać danych";
  } finally {
    checkButton.disabled = false;
  }
}
