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
import { TicketSummary } from "./ticket";
const app = document.getElementById("app");
if (!(app instanceof HTMLElement)) {
  throw new Error("Nie znaleziono elementu");
}

function App() {
  const [tickets, setTickets] = useState<TicketSummary[]>([
    { id: 1, title: "Problem z logowaniem", priority: 2, status: "Open" },
    { id: 2, title: "Problem z drukarką", priority: 3, status: "Open" },
  ]);
  const [isLoggingOut, setLoggingOut] = useState(false);
  const [logoutMessage, setLogoutMessage] = useState("");
  const [hasSessionError, setSessionError] = useState(false);
  const [sessionMessage, setSessionMessage] = useState("Sprawdzam sesję...");
  const [screen, setScreen] = useState<
    "guest" | "login" | "register" | "confirm" | "dashboard" | "checking"
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
  return (
    <div className="app-shell">
      {renderCurrentScreen()}
      <StatusPanel title="Status API" />
    </div>
  );
}

createRoot(app).render(<App />);
