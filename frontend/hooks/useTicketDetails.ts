import { useEffect, useState } from "react";
import type { TicketDetail } from "../ticket";
import { parseTicketDetails } from "../ticket";
import { getCsrfToken } from "../get-csrf-token";

export function useTicketDetails(
  isActive: boolean,
  selectedTicketId: number | null,
) {
  const [ticketDetail, setTicketDetail] = useState<TicketDetail | null>(null);
  const [isLoadingTicketDetail, setLoadingTicketDetail] = useState(false);
  const [ticketDetailError, setTicketDetailError] = useState("");
  const [ticketDetailRefreshKey, setTicketDetailRefreshKey] = useState(0);
  const [hasSessionExpired, setSessionExpired] = useState(false);
  const [isClosingTicket, setClosingTicket] = useState(false);
  const [closeTicketMessage, setCloseTicketMessage] = useState("");

  useEffect(() => {
    setSessionExpired(false);
    if (!isActive) return;

    setTicketDetail(null);
    setCloseTicketMessage("");
  }, [isActive, selectedTicketId]);

  useEffect(() => {
    if (!isActive || selectedTicketId === null) return;

    setTicketDetailError("");
    setLoadingTicketDetail(true);
    let ignore = false;
    async function loadTicketDetail() {
      try {
        const ticketDetailResponse = await fetch(
          `/api/tickets/${selectedTicketId}`,
          {
            credentials: "same-origin",
          },
        );
        if (ignore) return;
        if (ticketDetailResponse.status === 401) {
          setSessionExpired(true);
          return;
        }
        if (ticketDetailResponse.status === 404) {
          setTicketDetailError(
            "Zgłoszenie nie istnieje lub nie masz do niego dostępu.",
          );
          return;
        }
        if (!ticketDetailResponse.ok) {
          setTicketDetailError(
            "Nie udało się pobrać szczegółów zgłoszenia, sprawdź połączenie i spróbuj ponownie.",
          );
          return;
        }
        const ticketDetailsData: unknown = await ticketDetailResponse.json();
        if (ignore) return;
        setTicketDetail(parseTicketDetails(ticketDetailsData));
      } catch (error) {
        if (!ignore) {
          setTicketDetailError("Błąd połączenia lub odczytu danych");
        }
      } finally {
        if (!ignore) {
          setLoadingTicketDetail(false);
        }
      }
    }
    loadTicketDetail();
    return () => {
      ignore = true;
    };
  }, [isActive, selectedTicketId, ticketDetailRefreshKey]);

  function refreshTicketDetail() {
    setTicketDetailRefreshKey((previous) => previous + 1);
  }
  async function handleCloseTicketClick() {
    if (selectedTicketId === null) return;
    setClosingTicket(true);
    setCloseTicketMessage("Zamykam zgłoszenie...");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setCloseTicketMessage(
          "Nie udało się przygotować bezpiecznego zamknięcia zgłoszenia.",
        );
        return;
      }
      const responseCloseTicket = await fetch(
        `/api/tickets/${selectedTicketId}/close`,
        {
          method: "POST",
          credentials: "same-origin",
          headers: {
            "X-CSRF-TOKEN": csrfToken,
          },
        },
      );
      if (responseCloseTicket.status === 401) {
        setSessionExpired(true);
        return;
      }
      if (responseCloseTicket.status === 409) {
        setCloseTicketMessage("Zgłoszenie jest już zamknięte");
        refreshTicketDetail();
        return;
      }
      if (responseCloseTicket.status === 404) {
        setCloseTicketMessage(
          "Zgłoszenie nie istnieje lub nie masz do niego dostępu",
        );
        return;
      }
      if (!responseCloseTicket.ok) {
        setCloseTicketMessage(
          "Wystąpił problem z zamknięciem zgłoszenia. Spróbuj ponownie.",
        );
        return;
      }
      setCloseTicketMessage("Zgłoszenie zamknięte");
      refreshTicketDetail();
    } catch (error) {
      setCloseTicketMessage(
        "Nie udało się zamknąć zgłoszenia. Spróbuj ponownie.",
      );
    } finally {
      setClosingTicket(false);
    }
  }

  return {
    ticketDetail,
    isLoadingTicketDetail,
    ticketDetailError,
    hasSessionExpired,
    refreshTicketDetail,
    isClosingTicket,
    closeTicketMessage,
    handleCloseTicketClick,
  };
}
