import { createRoot } from "react-dom/client";
import { LoginScreen } from "./components/LoginScreen";
import { StatusPanel } from "./components/StatusPanel";
import { GuestHome } from "./components/GuestHome";
import { AuthenticatedHome } from "./components/AuthenticatedHome";
import { CheckingSession } from "./components/CheckingSession";
import { RegisterScreen } from "./components/RegisterScreen";
import { useState, useEffect } from "react";
import { ConfirmEmailScreen } from "./components/ConfirmEmailScreen";
import { getCsrfToken } from "./get-csrf-token";
import { TicketSummary, parseTicket } from "./ticket";
import { TicketDetail, parseTicketDetails } from "./ticket";
import { TicketDetailsScreen } from "./components/TicketDetailsScreen";

const app = document.getElementById("app");
if (!(app instanceof HTMLElement)) {
  throw new Error("Nie znaleziono elementu");
}

function App() {
  const [ticketDetailRefreshKey, setTicketDetailRefreshKey] = useState(0);
  const [ticketDetail, setTicketDetail] = useState<TicketDetail | null>(null);
  const [isLoadingTicketDetail, setLoadingTicketDetail] = useState(false);
  const [ticketDetailError, setTicketDetailError] = useState("");
  const [selectedTicketId, setSelectedTicketId] = useState<number | null>(null);
  const [ticketsRefreshKey, setTicketsRefreshKey] = useState(0);
  const [ticketsError, setTicketsError] = useState("");
  const [tickets, setTickets] = useState<TicketSummary[]>([]);
  const [isLoadingTickets, setLoadingTickets] = useState(true);
  const [isLoggingOut, setLoggingOut] = useState(false);
  const [logoutMessage, setLogoutMessage] = useState("");
  const [hasSessionError, setSessionError] = useState(false);
  const [sessionMessage, setSessionMessage] = useState("Sprawdzam sesję...");
  const [screen, setScreen] = useState<
    | "guest"
    | "login"
    | "register"
    | "confirm"
    | "dashboard"
    | "checking"
    | "details"
  >(window.location.pathname === "/confirm-email" ? "confirm" : "checking");
  async function checkSession() {
    setSessionError(false);
    setSessionMessage("Sprawdzam sesję...");
    try {
      const meResponse = await fetch("/api/auth/me", {
        method: "GET",
        credentials: "same-origin",
      });
      if (meResponse.status === 200) {
        setScreen("dashboard");
      } else if (meResponse.status === 401) {
        setScreen("guest");
      } else {
        setSessionError(true);
        setSessionMessage(
          "Nie udało się sprawdzić sesji. Sprawdź API i odswież stronę.",
        );
      }
    } catch {
      setSessionError(true);
      setSessionMessage(
        "Nie udało się sprawdzić sesji. Sprawdź API i odswież stronę.",
      );
    }
  }
  useEffect(() => {
    if (window.location.pathname === "/confirm-email") return;

    checkSession();
  }, []);
  useEffect(() => {
    if (screen !== "dashboard") return;
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
          setScreen("login");
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
  }, [screen, ticketsRefreshKey]);
  useEffect(() => {
    if (screen !== "details" || selectedTicketId === null) return;
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
          setScreen("login");
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
  }, [screen, selectedTicketId, ticketDetailRefreshKey]);

  function openLoginFromConfirmation() {
    window.history.replaceState(null, "", "/");
    setScreen("login");
  }

  function renderCurrentScreen() {
    if (screen === "login") {
      return (
        <LoginScreen
          onBackClick={() => setScreen("guest")}
          onLoginSuccess={() => setScreen("dashboard")}
        />
      );
    }
    if (screen === "register") {
      return <RegisterScreen onBackClick={() => setScreen("guest")} />;
    }
    if (screen === "confirm") {
      return <ConfirmEmailScreen onLoginClick={openLoginFromConfirmation} />;
    }
    if (screen === "dashboard") {
      return (
        <AuthenticatedHome
          tickets={tickets}
          message={logoutMessage}
          onLogoutClick={handleLogoutClick}
          isLoggingOut={isLoggingOut}
          isLoadingTickets={isLoadingTickets}
          ticketsError={ticketsError}
          onTicketCreated={refreshTickets}
          onRefreshTickets={refreshTickets}
          onTicketClick={(id) => {
            setTicketDetail(null);
            setSelectedTicketId(id);
            setScreen("details");
          }}
        />
      );
    }
    if (screen === "checking") {
      return (
        <CheckingSession
          message={sessionMessage}
          onRetryClick={checkSession}
          hasSessionError={hasSessionError}
        />
      );
    }
    if (screen === "details") {
      return (
        <TicketDetailsScreen
          selectedTicketId={selectedTicketId}
          ticketDetail={ticketDetail}
          isLoadingTicketDetail={isLoadingTicketDetail}
          ticketDetailError={ticketDetailError}
          onBackClick={() => setScreen("dashboard")}
          onRetryClick={refreshTicketDetail}
        />
      );
    }
    return (
      <GuestHome
        onLoginClick={() => setScreen("login")}
        onRegisterClick={() => setScreen("register")}
      />
    );
  }
  async function handleLogoutClick() {
    setLogoutMessage("Przygotowuję wylogowanie...");
    setLoggingOut(true);
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setLogoutMessage("Nie udało się przygotować bezpiecznego wylogowania.");
        return;
      }
      const logoutResponse = await fetch("/api/auth/logout", {
        method: "POST",
        credentials: "same-origin",
        headers: {
          "X-CSRF-TOKEN": csrfToken,
        },
      });
      if (!logoutResponse.ok) {
        setLogoutMessage(
          "Wystąpił problem z wylogowaniem, sprawdź połączenie i spróbuj ponownie.",
        );
        return;
      }
      setScreen("guest");
      setLogoutMessage("");
    } catch (error) {
      setLogoutMessage(
        "Błąd API lub sieci, sprawdź połączenie i spróbuj ponownie.",
      );
    } finally {
      setLoggingOut(false);
    }
  }
  function refreshTickets() {
    setTicketsRefreshKey((previous) => previous + 1);
  }
  function refreshTicketDetail() {
    setTicketDetailRefreshKey((previous) => previous + 1);
  }

  return (
    <div className="app-shell">
      {renderCurrentScreen()}
      <StatusPanel title="Status API" />
    </div>
  );
}

createRoot(app).render(<App />);
