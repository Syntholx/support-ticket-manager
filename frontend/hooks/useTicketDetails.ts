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
            "Ticket not found or access denied.",
          );
          return;
        }
        if (!ticketDetailResponse.ok) {
          setTicketDetailError(
            "Could not load ticket details. Check your connection and try again.",
          );
          return;
        }
        const ticketDetailsData: unknown = await ticketDetailResponse.json();
        if (ignore) return;
        setTicketDetail(parseTicketDetails(ticketDetailsData));
      } catch (error) {
        if (!ignore) {
          setTicketDetailError("Could not connect or read the response");
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
    setStartTicketMessage("Starting work…");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setStartTicketMessage(
          "Could not prepare the request to start work.",
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
        setStartTicketMessage("You do not have permission to start work on this ticket");
        return;
      }
      if (responseStartTicket.status === 404) {
        setStartTicketMessage("Ticket not found or unavailable");
        return;
      }
      if (responseStartTicket.status === 409) {
        setStartTicketMessage(
          "Ticket status changed. Details refreshed.",
        );
        refreshTicketDetail();
        return;
      }
      if (!responseStartTicket.ok) {
        setStartTicketMessage(
          "Could not start work. Try again.",
        );
        return;
      }
      setStartTicketMessage("Work started");
      refreshTicketDetail();
    } catch (error) {
      setStartTicketMessage(
        "Could not start work on the ticket. Try again.",
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
    setCloseTicketMessage("Closing ticket…");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setCloseTicketMessage(
          "Could not prepare the request to close the ticket.",
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
        setCloseTicketMessage("Ticket is already closed");
        refreshTicketDetail();
        return;
      }
      if (responseCloseTicket.status === 404) {
        setCloseTicketMessage(
          "Ticket not found or access denied",
        );
        return;
      }
      if (!responseCloseTicket.ok) {
        setCloseTicketMessage(
          "Could not close the ticket. Try again.",
        );
        return;
      }
      setCloseTicketMessage("Ticket closed");
      refreshTicketDetail();
    } catch (error) {
      setCloseTicketMessage(
        "Could not close the ticket. Try again.",
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
    setReopenTicketMessage("Reopening ticket…");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setReopenTicketMessage(
          "Could not prepare the request to reopen the ticket. Try again.",
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
          "You do not have permission to reopen this ticket.",
        );
        return;
      }
      if (ticketReopenResponse.status === 404) {
        setReopenTicketMessage(
          "Ticket not found or access denied",
        );
        return;
      }
      if (ticketReopenResponse.status === 409) {
        setReopenTicketMessage("Status changed. Details refreshed.");
        refreshTicketDetail();
        return;
      }
      if (!ticketReopenResponse.ok) {
        setReopenTicketMessage(
          "Could not reopen the ticket. Try again.",
        );
        return;
      }
      setReopenTicketMessage("Ticket reopened.");
      refreshTicketDetail();
    } catch (error) {
      setReopenTicketMessage(
        "Could not reopen the ticket. Check your connection and try again.",
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
      setPriorityMessage("Priority must be an integer between 1 and 5.");
      return;
    }
    setChangingPriority(true);
    setPriorityMessage("Updating priority…");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setPriorityMessage(
          "Could not prepare the priority update. Try again.",
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
          "Invalid priority. Choose a value from 1 to 5.",
        );
        return;
      }
      if (changePriorityResponse.status === 401) {
        setSessionExpired(true);
        return;
      }
      if (changePriorityResponse.status === 403) {
        setPriorityMessage(
          "You do not have permission to change priority. Contact the administrator if needed.",
        );
        return;
      }
      if (changePriorityResponse.status === 404) {
        setPriorityMessage("Ticket not found or access denied");
        return;
      }
      if (changePriorityResponse.status === 409) {
        setPriorityMessage("The ticket changed. Details refreshed.");
        refreshTicketDetail();
        return;
      }
      if (!changePriorityResponse.ok) {
        setPriorityMessage(
          "Could not change the priority. Try again.",
        );
        return;
      }
      setPriorityMessage("Priority updated.");
      refreshTicketDetail();
    } catch (error) {
      setPriorityMessage(
        "Could not change the priority. Check your connection and try again.",
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
