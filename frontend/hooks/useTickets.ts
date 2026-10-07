import { useState } from "react";
import { TicketSummary, parseTicket } from "../ticket";
import { useEffect } from "react";
export function useTickets(isActive: boolean) {
  const [ticketsRefreshKey, setTicketsRefreshKey] = useState(0);
  const [ticketsError, setTicketsError] = useState("");
  const [isLoadingTickets, setLoadingTickets] = useState(true);
  const [tickets, setTickets] = useState<TicketSummary[]>([]);
  const [hasSessionExpired, setSessionExpired] = useState(false);
  function refreshTickets() {
    setTicketsRefreshKey((previous) => previous + 1);
  }
  useEffect(() => {
    setSessionExpired(false);
  }, [isActive]);
  useEffect(() => {
    if (!isActive) return;
    setTickets([]);
    setTicketsError("");
    setLoadingTickets(true);
    let ignore = false;

    async function loadTickets() {
      try {
        const loadTicketsResponse = await fetch("/api/tickets", {
          credentials: "same-origin",
        });
        if (ignore) return;
        if (loadTicketsResponse.status === 401) {
          setSessionExpired(true);
          return;
        }
        if (!loadTicketsResponse.ok) {
          setTicketsError("Nie udało się pobrać zgłoszeń");
          return;
        }
        const ticketsData: unknown = await loadTicketsResponse.json();
        if (ignore) return;
        if (!Array.isArray(ticketsData)) {
          setTicketsError("Api zwróciło nieprawidłową listę zgłoszeń");
          return;
        }
        const parsedTickets = ticketsData.map(parseTicket);
        setTickets(parsedTickets);
      } catch (error) {
        if (!ignore) {
          setTicketsError(
            "Nie udało się pobrać lub odczytać zgłoszeń, sprawdź połączenie i spróbuj ponownie.",
          );
          return;
        }
      } finally {
        if (!ignore) {
          setLoadingTickets(false);
        }
      }
    }
    loadTickets();
    return () => {
      ignore = true;
    };
  }, [isActive, ticketsRefreshKey]);

  return {
    tickets,
    ticketsError,
    isLoadingTickets,
    hasSessionExpired,
    refreshTickets,
  };
}
