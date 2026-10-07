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
import { TicketDetailsScreen } from "./components/TicketDetailsScreen";
import { useTicketDetails } from "./hooks/useTicketDetails";
import { useTickets } from "./hooks/useTickets";
import {
  BrowserRouter,
  Route,
  useLocation,
  useNavigate,
  Routes,
  Navigate,
} from "react-router";
import { useMatch } from "react-router";

const app = document.getElementById("app");
if (!(app instanceof HTMLElement)) {
  throw new Error("Nie znaleziono elementu");
}

function App() {
  const location = useLocation();
  const navigate = useNavigate();
  const ticketMatch = useMatch("/tickets/:ticketId");
  const ticketIdText = ticketMatch?.params.ticketId;
  const ticketIdNumber = Number(ticketIdText);
  const selectedTicketId =
    ticketIdText !== undefined &&
    /^[1-9]\d*$/.test(ticketIdText) &&
    Number.isSafeInteger(ticketIdNumber)
      ? ticketIdNumber
      : null;
  const [isCheckingSession, setCheckingSession] = useState(
    location.pathname !== "/confirm-email",
  );
  const [isAuthenticated, setAuthenticated] = useState(false);
  const [isLoggingOut, setLoggingOut] = useState(false);
  const [logoutMessage, setLogoutMessage] = useState("");
  const [hasSessionError, setSessionError] = useState(false);
  const [sessionMessage, setSessionMessage] = useState("Sprawdzam sesję...");
  const {
    ticketDetail,
    isLoadingTicketDetail,
    ticketDetailError,
    hasSessionExpired: detailsSessionExpired,
    refreshTicketDetail,
    isClosingTicket,
    closeTicketMessage,
    handleCloseTicketClick,
  } = useTicketDetails(
    ticketMatch !== null &&
      selectedTicketId !== null &&
      isAuthenticated &&
      !isCheckingSession &&
      !hasSessionError,
    selectedTicketId,
  );
  const {
    tickets,
    ticketsError,
    isLoadingTickets,
    hasSessionExpired: ticketsSessionExpired,
    refreshTickets,
  } = useTickets(
    location.pathname === "/tickets" &&
      isAuthenticated &&
      !isCheckingSession &&
      !hasSessionError,
  );
  useEffect(() => {
    if (detailsSessionExpired || ticketsSessionExpired) {
      setAuthenticated(false);
      navigate("/login", { replace: true });
    }
  }, [ticketsSessionExpired, detailsSessionExpired, navigate]);

  async function checkSession() {
    setCheckingSession(true);
    setSessionError(false);
    setSessionMessage("Sprawdzam sesję...");
    try {
      const meResponse = await fetch("/api/auth/me", {
        method: "GET",
        credentials: "same-origin",
      });
      if (meResponse.status === 200) {
        setAuthenticated(true);
      } else if (meResponse.status === 401) {
        setAuthenticated(false);
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
    } finally {
      setCheckingSession(false);
    }
  }
  useEffect(() => {
    if (window.location.pathname === "/confirm-email") return;

    checkSession();
  }, []);

  function openLoginFromConfirmation() {
    navigate("/login", { replace: true });
  }

  function renderCurrentScreen() {
    if (isCheckingSession || hasSessionError) {
      return (
        <CheckingSession
          message={sessionMessage}
          onRetryClick={checkSession}
          hasSessionError={hasSessionError}
        />
      );
    }
    if (
      !isAuthenticated &&
      (location.pathname === "/tickets" || ticketMatch !== null)
    ) {
      return <Navigate to="/login" replace />;
    }

    if (
      isAuthenticated &&
      (location.pathname === "/" || location.pathname === "/login")
    ) {
      return <Navigate to="/tickets" replace />;
    }
    if (ticketMatch !== null && selectedTicketId === null) {
      return (
        <section className="welcome-card">
          <h1>Nieprawidłowy numer zgłoszenia</h1>
          <button
            type="button"
            className="back-button"
            onClick={() => navigate("/tickets")}
          >
            {" "}
            Wróć do listy
          </button>
        </section>
      );
    }
    return (
      <Routes>
        <Route
          path="/login"
          element={
            <LoginScreen
              onBackClick={() => navigate("/")}
              onLoginSuccess={() => {
                setAuthenticated(true);
                navigate("/tickets", { replace: true });
              }}
            />
          }
        />
        <Route
          path="/register"
          element={<RegisterScreen onBackClick={() => navigate("/")} />}
        />
        <Route
          path="/confirm-email"
          element={
            <ConfirmEmailScreen onLoginClick={openLoginFromConfirmation} />
          }
        />
        <Route
          path="/tickets"
          element={
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
                navigate(`/tickets/${id}`);
              }}
            />
          }
        />
        <Route
          path="/tickets/:ticketId"
          element={
            <TicketDetailsScreen
              selectedTicketId={selectedTicketId}
              ticketDetail={ticketDetail}
              isLoadingTicketDetail={isLoadingTicketDetail}
              ticketDetailError={ticketDetailError}
              onBackClick={() => navigate("/tickets")}
              onRetryClick={refreshTicketDetail}
              onCloseTicket={handleCloseTicketClick}
              isClosingTicket={isClosingTicket}
              closeTicketMessage={closeTicketMessage}
            />
          }
        />
        <Route
          path="/"
          element={
            <GuestHome
              onLoginClick={() => navigate("/login")}
              onRegisterClick={() => navigate("/register")}
            />
          }
        />
        <Route
          path="*"
          element={
            <section className="welcome-card">
              <h1>Nie znaleziono strony</h1>
              <button
                type="button"
                className="back-button"
                onClick={() => navigate("/")}
              >
                Wróć na stronę główną
              </button>
            </section>
          }
        />
      </Routes>
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
      setAuthenticated(false);
      navigate("/", { replace: true });
      setLogoutMessage("");
    } catch (error) {
      setLogoutMessage(
        "Błąd API lub sieci, sprawdź połączenie i spróbuj ponownie.",
      );
    } finally {
      setLoggingOut(false);
    }
  }

  return (
    <div className="app-shell">
      {renderCurrentScreen()}
      <StatusPanel title="Status API" />
    </div>
  );
}

createRoot(app).render(
  <BrowserRouter>
    <App />
  </BrowserRouter>,
);
