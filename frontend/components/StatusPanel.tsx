import { useState } from "react";
import { parseApiStatus } from "../api-status.js";
export function StatusPanel(props: { title: string }) {
  const [message, setMessage] = useState("TSM connection not checked");
  const [isLoading, setIsLoading] = useState(false);
  async function handleCheckConnection() {
    try {
      setMessage("Connecting to the TSM API…");
      setIsLoading(true);
      const response = await fetch("api/status");
      if (!response.ok) {
        setMessage("The API returned an error");
        return;
      }
      const data: unknown = await response.json();
      const { name, version, isRunning } = parseApiStatus(data);
      if (isRunning === true) {
        setMessage(`${name} — version ${version} — API is online`);
      } else {
        setMessage(`${name} — version ${version} — API is offline.`);
      }
    } catch (error) {
      setMessage("Could not connect");
    } finally {
      setIsLoading(false);
    }
  }
  return (
    <section className="mt-5 rounded-2xl border border-solid border-[#dce5ef] bg-white p-6 [&>h2]:mt-0">
      <h2>{props.title}</h2>
      <StatusMessage text={message} />
      <button
        className="min-h-[44px] rounded-[10px] border border-solid border-[#b8c8e4] bg-white px-4 py-2.5 font-semibold text-[#1d4ed8] hover:bg-[#eff6ff] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        type="button"
        onClick={handleCheckConnection}
        disabled={isLoading}
      >
        {isLoading ? "Checking…" : "Check TSM connection"}
      </button>
    </section>
  );
}
function StatusMessage(props: { text: string }) {
  return <p role="status">{props.text}</p>;
}
