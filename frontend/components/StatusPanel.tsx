import { useState } from "react";
import { parseApiStatus } from "../api-status.js";
export function StatusPanel(props: { title: string }) {
  const [message, setMessage] = useState("Nie sprawdzono połączenia z TSM");
  const [isLoading, setIsLoading] = useState(false);
  async function handleCheckConnection() {
    try {
      setMessage("Łaczenie z API TSM....");
      setIsLoading(true);
      const response = await fetch("api/status");
      if (!response.ok) {
        setMessage("API zwróciło błąd");
        return;
      }
      const data: unknown = await response.json();
      const { name, version, isRunning } = parseApiStatus(data);
      if (isRunning === true) {
        setMessage(`${name} — wersja ${version} — API działa poprawnie`);
      } else {
        setMessage(`${name} — wersja ${version} — API zgłasza, że nie działa.`);
      }
    } catch (error) {
      setMessage("Nie udało się połączyć");
    } finally {
      setIsLoading(false);
    }
  }
  return (
    <section className="status-card">
      <h2>{props.title}</h2>
      <StatusMessage text={message} />
      <button
        type="button"
        onClick={handleCheckConnection}
        disabled={isLoading}
      >
        {isLoading ? "Sprawdzanie..." : "Sprawdź połączenie z TSM"}
      </button>
    </section>
  );
}
function StatusMessage(props: { text: string }) {
  return <p role="status">{props.text}</p>;
}
