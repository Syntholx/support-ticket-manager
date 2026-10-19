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
  const [isStartingTicket, setStartingTicket] = useState(false);
  const [startTicketMessage, setStartTicketMessage] = useState("");
  const [isReopeningTicket, setReopeningTicket] = useState(false);
  const [reopenTicketMessage, setReopenTicketMessage] = useState("");
  const [isChangingPriority, setChangingPriority] = useState(false);
  const [priorityMessage, setPriorityMessage] = useState("");

  useEffect(() => {
    setSessionExpired(false);
    if (!isActive) return;

    setTicketDetail(null);
    setCloseTicketMessage("");
    setStartTicketMessage("");
    setReopenTicketMessage("");
    setPriorityMessage("");
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
  async function handleStartTicketClick() {
    if (
      selectedTicketId === null ||
      isClosingTicket ||
      isStartingTicket ||
      isReopeningTicket ||
      isChangingPriority
    )
      return;
    setStartingTicket(true);
    setStartTicketMessage("Rozpoczynam obsługę...");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setStartTicketMessage(
          "Nie udało się przygotować bezpiecznie obsługi zgłoszenia.",
        );
        return;
      }
      const responseStartTicket = await fetch(
        `/api/tickets/${selectedTicketId}/start`,
        {
          method: "POST",
          credentials: "same-origin",
          headers: {
            "X-CSRF-TOKEN": csrfToken,
          },
        },
      );
      if (responseStartTicket.status === 401) {
        setSessionExpired(true);
        return;
      }
      if (responseStartTicket.status === 403) {
        setStartTicketMessage("Nie masz uprawnień do rozpoczęcia obsługi");
        return;
      }
      if (responseStartTicket.status === 404) {
        setStartTicketMessage("Zgłoszenie nie istnieje lub jest niedostępne");
        return;
      }
      if (responseStartTicket.status === 409) {
        setStartTicketMessage(
          "Status zgłoszenia się zmienił. Odświeżono szczegoły.",
        );
        refreshTicketDetail();
        return;
      }
      if (!responseStartTicket.ok) {
        setStartTicketMessage(
          "Wystąpił problem z rozpoczęciem obsługi. Spróbuj ponownie.",
        );
        return;
      }
      setStartTicketMessage("Obsługa się rozpoczęła");
      refreshTicketDetail();
    } catch (error) {
      setStartTicketMessage(
        "Nie udało się rozpoczać obsługi zgłoszenia, Spróbuj ponownie.",
      );
    } finally {
      setStartingTicket(false);
    }
  }
  async function handleCloseTicketClick() {
    if (
      selectedTicketId === null ||
      isClosingTicket ||
      isStartingTicket ||
      isReopeningTicket ||
      isChangingPriority
    )
      return;
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
  async function handleReopenTicketClick() {
    if (
      selectedTicketId === null ||
      isClosingTicket ||
      isStartingTicket ||
      isReopeningTicket ||
      isChangingPriority
    )
      return;
    setReopeningTicket(true);
    setReopenTicketMessage("Otwieram zgłoszenie ponownie...");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setReopenTicketMessage(
          "Nie udało się przygotować bezpiecznie ponownego otwarcia zgłoszenia. Spróbuj ponownie.",
        );
        return;
      }
      const ticketReopenResponse = await fetch(
        `/api/tickets/${selectedTicketId}/reopen`,
        {
          method: "POST",
          credentials: "same-origin",
          headers: {
            "X-CSRF-TOKEN": csrfToken,
          },
        },
      );
      if (ticketReopenResponse.status === 401) {
        setSessionExpired(true);
        return;
      }
      if (ticketReopenResponse.status === 403) {
        setReopenTicketMessage(
          "Nie masz uprawnień do ponownego otwarcia zgłoszenia.",
        );
        return;
      }
      if (ticketReopenResponse.status === 404) {
        setReopenTicketMessage(
          "Zgłoszenie nie istnieje lub nie masz do niego dostępu",
        );
        return;
      }
      if (ticketReopenResponse.status === 409) {
        setReopenTicketMessage("Status się zmienił, odswieżono szczegoły.");
        refreshTicketDetail();
        return;
      }
      if (!ticketReopenResponse.ok) {
        setReopenTicketMessage(
          "Wystąpił problem z ponownym otwarciem zgłoszenia. Spróbuj ponownie.",
        );
        return;
      }
      setReopenTicketMessage("Sukces! Udało się ponownie otworzyć zgłoszenie.");
      refreshTicketDetail();
    } catch (error) {
      setReopenTicketMessage(
        "Nie udało się otworzyć ponownie zgłoszenia. Sprawdź połączenie z API i spróbuj ponownie.",
      );
      return;
    } finally {
      setReopeningTicket(false);
    }
  }
  async function handleChangePriority(priority: number) {
    if (
      selectedTicketId === null ||
      isClosingTicket ||
      isStartingTicket ||
      isReopeningTicket ||
      isChangingPriority
    )
      return;

    if (!Number.isInteger(priority) || priority < 1 || priority > 5) {
      setPriorityMessage("Priorytet musi być liczbą całkowitą od 1 do 5.");
      return;
    }
    setChangingPriority(true);
    setPriorityMessage("Zmieniam priorytet...");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setPriorityMessage(
          "Nie udało się przygotować bezpiecznie zmiany priorytetu. Spróbuj ponownie.",
        );
        return;
      }
      const changePriorityResponse = await fetch(
        `/api/tickets/${selectedTicketId}/priority`,
        {
          method: "POST",
          credentials: "same-origin",
          headers: {
            "X-CSRF-TOKEN": csrfToken,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ priority }),
        },
      );
      if (changePriorityResponse.status === 400) {
        setPriorityMessage(
          "Nieprawidłowy priorytet, priorytet mieści się między 1 a 5.",
        );
        return;
      }
      if (changePriorityResponse.status === 401) {
        setSessionExpired(true);
        return;
      }
      if (changePriorityResponse.status === 403) {
        setPriorityMessage(
          "Nie masz uprawnień do zmiany priorytetu. Jeśli to błąd, skontaktuj się z administratorem strony.",
        );
        return;
      }
      if (changePriorityResponse.status === 404) {
        setPriorityMessage("Zgłoszenie nie istnieje lub nie masz dostępu");
        return;
      }
      if (changePriorityResponse.status === 409) {
        setPriorityMessage("Wystąpił konflikt zapisu. Odświeżono szczegóły.");
        refreshTicketDetail();
        return;
      }
      if (!changePriorityResponse.ok) {
        setPriorityMessage(
          "Wystąpił problem ze zmianą priorytetu. Spróbuj ponownie.",
        );
        return;
      }
      setPriorityMessage("Sukces! Priorytet został zmieniony.");
      refreshTicketDetail();
    } catch (error) {
      setPriorityMessage(
        "Nie udało się zmienić priorytetu. Sprawdź połączenie z API i spróbuj ponownie.",
      );
      return;
    } finally {
      setChangingPriority(false);
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
    isStartingTicket,
    startTicketMessage,
    handleStartTicketClick,
    isReopeningTicket,
    reopenTicketMessage,
    handleReopenTicketClick,
    isChangingPriority,
    priorityMessage,
    handleChangePriority,
  };
}
