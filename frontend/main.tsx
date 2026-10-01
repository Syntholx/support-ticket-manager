import { createRoot } from "react-dom/client";
import { useState } from "react";
import { parseApiStatus } from "./api-status.js";
const heading = document.getElementById("status-heading-root");
if (!(heading instanceof HTMLElement)) {
  throw new Error("Nie znaleziono elementu");
}
createRoot(heading).render(<StatusPanel title="Status TSM" />);

function StatusPanel(props: { title: string }) {
  const [message, setMessage] = useState("Nie sprawdzono połączenia z TSM");
  const [isLoading, setIsLoading] = useState(false);
  async function handleCheckConnection() {
    try {
      setMessage("Łaczenie z API TSM....");
      setIsLoading(true);
      const response = await fetch("https://localhost:7280/api/status");
      if (!response.ok) {
        setMessage("API zwróciło błąd");
        return;
      }
      const data: unknown = await response.json();
      const { name, version } = parseApiStatus(data);
      setMessage(`${name} — wersja ${version}`);
    } catch (error) {
      setMessage("Nie udało się połączyć");
    } finally {
      setIsLoading(false);
    }
  }
  return (
    <div>
      <h1>{props.title}</h1>
      <StatusMessage text={message} />
      <button
        type="button"
        onClick={handleCheckConnection}
        disabled={isLoading}
      >
        Sprawdź połączenie z TSM
      </button>
    </div>
  );
}
function StatusMessage(props: { text: string }) {
  return <p role="status">{props.text}</p>;
}
