import { useState } from "react";
import { TicketSummary, parseTicket } from "../ticket";
import { useEffect } from "react";
export function useTickets(
  isActive: boolean,
  listType: "archived" | "active" = "active",
  page: number = 1,
  pageSize: number = 50,
) {
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
  }, [isActive, listType]);
  useEffect(() => {
    if (!isActive) return;
    setTickets([]);
    setTicketsError("");
    setLoadingTickets(true);
    let ignore = false;

    async function loadTickets() {
      try {
        let endpoint = "/api/tickets";
        if (listType === "archived") {
          endpoint = "/api/tickets/archived";
        }
        endpoint = `${endpoint}?page=${page}&pageSize=${pageSize}`;

        const loadTicketsResponse = await fetch(endpoint, {
          credentials: "same-origin",
        });

        if (ignore) return;
        if (loadTicketsResponse.status === 401) {
          setSessionExpired(true);
          return;
        }
        if (!loadTicketsResponse.ok) {
          setTicketsError("Could not load tickets");
          return;
        }
        const ticketsData: unknown = await loadTicketsResponse.json();
        if (ignore) return;
        if (!Array.isArray(ticketsData)) {
          setTicketsError("The API returned an invalid ticket list");
          return;
        }
        const parsedTickets = ticketsData.map(parseTicket);
        setTickets(parsedTickets);
      } catch (error) {
        if (!ignore) {
          setTicketsError(
            "Could not load or read tickets. Check your connection and try again.",
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
  }, [isActive, ticketsRefreshKey, listType, page, pageSize]);

  return {
    tickets,
    ticketsError,
    isLoadingTickets,
    hasSessionExpired,
    refreshTickets,
  };
}
