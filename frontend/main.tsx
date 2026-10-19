import { createRoot } from "react-dom/client";
import { ArchivedTicketsScreen } from "./components/ArchivedTicketsScreen";
import { LoginScreen } from "./components/LoginScreen";
import { StatusPanel } from "./components/StatusPanel";
import { GuestHome } from "./components/GuestHome";
import { AuthenticatedHome } from "./components/AuthenticatedHome";
import { CheckingSession } from "./components/CheckingSession";
import { RegisterScreen } from "./components/RegisterScreen";
import { useEffect, startTransition, useState } from "react";
import { ConfirmEmailScreen } from "./components/ConfirmEmailScreen";
import { TicketDetailsScreen } from "./components/TicketDetailsScreen";
import { useTicketDetails } from "./hooks/useTicketDetails";
import { useTickets } from "./hooks/useTickets";
import { useSession } from "./hooks/useSession";
import {
  BrowserRouter,
  Route,
  useLocation,
  useNavigate,
  Routes,
  Navigate,
  useMatch,
} from "react-router";
import { useLogout } from "./hooks/useLogout";

const app = document.getElementById("app");
if (!(app instanceof HTMLElement)) {
  throw new Error("Nie znaleziono elementu");
}

function App() {
  const [activePage, setActivePage] = useState(1);
  const activePageSize = 5;
  const maxActivePage = 10000;
  const maxArchivePage = 10000;
  const archivePageSize = 5;
  const location = useLocation();
  const ticketReturnPath =
    location.state?.returnTo === "/tickets/archived"
      ? "/tickets/archived"
      : "/tickets";
  const navigate = useNavigate();

  const ticketMatch = useMatch("/tickets/:ticketId");
  const ticketIdText = ticketMatch?.params.ticketId;
  const ticketIdNumber = Number(ticketIdText);
  const isArchiveRoute = location.pathname === "/tickets/archived";
  const isTicketDetailsRoute = ticketMatch !== null && !isArchiveRoute;
  const selectedTicketId =
    ticketIdText !== undefined &&
    /^[1-9]\d*$/.test(ticketIdText) &&
    Number.isSafeInteger(ticketIdNumber)
      ? ticketIdNumber
      : null;

  const {
    isAuthenticated,
    isCheckingSession,
    hasSessionError,
    sessionMessage,
    checkSession,
    setAuthenticated,
    isSupport,
  } = useSession(location.pathname !== "/confirm-email");
  const {
    ticketDetail,
    isLoadingTicketDetail,
    ticketDetailError,
    hasSessionExpired: detailsSessionExpired,
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
  } = useTicketDetails(
    isTicketDetailsRoute &&
      selectedTicketId !== null &&
      isAuthenticated &&
      !isCheckingSession &&
      !hasSessionError,
    selectedTicketId,
  );
  const [archivePage, setArchivePage] = useState(1);
  const {
    tickets,
    ticketsError,
    isLoadingTickets,
    hasSessionExpired: ticketsSessionExpired,
    refreshTickets,
  } = useTickets(
    (location.pathname === "/tickets" || isArchiveRoute) &&
      isAuthenticated &&
      !isCheckingSession &&
      !hasSessionError,
    isArchiveRoute ? "archived" : "active",
    isArchiveRoute ? archivePage : activePage,
    isArchiveRoute ? archivePageSize : activePageSize,
  );
  const { isLoggingOut, logoutMessage, handleLogoutClick } = useLogout(() => {
    startTransition(() => {
      setAuthenticated(false);
      setArchivePage(1);
      setActivePage(1);
      navigate("/", { replace: true });
    });
  });
  useEffect(() => {
    if (detailsSessionExpired || ticketsSessionExpired) {
      setAuthenticated(false);
      setArchivePage(1);
      setActivePage(1);
      navigate("/login", { replace: true });
    }
  }, [ticketsSessionExpired, detailsSessionExpired, navigate]);

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
      (location.pathname === "/tickets" ||
        isArchiveRoute ||
        isTicketDetailsRoute)
    ) {
      return <Navigate to="/login" replace />;
    }

    if (
      isAuthenticated &&
      (location.pathname === "/" || location.pathname === "/login")
    ) {
      return <Navigate to="/tickets" replace />;
    }
    if (isTicketDetailsRoute && selectedTicketId === null) {
      return (
        <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
          <h1>Nieprawidłowy numer zgłoszenia</h1>
          <button
            type="button"
            className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8]"
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
                checkSession();
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
              isSupport={isSupport}
              message={logoutMessage}
              onLogoutClick={handleLogoutClick}
              isLoggingOut={isLoggingOut}
              isLoadingTickets={isLoadingTickets}
              ticketsError={ticketsError}
              onTicketCreated={refreshTickets}
              onRefreshTickets={refreshTickets}
              page={activePage}
              pageSize={activePageSize}
              maxPage={maxActivePage}
              onNextPage={() =>
                setActivePage((previous) =>
                  Math.min(maxActivePage, previous + 1),
                )
              }
              onPreviousPage={() =>
                setActivePage((previous) => Math.max(1, previous - 1))
              }
              onArchiveClick={() => navigate("/tickets/archived")}
              onTicketClick={(id) => {
                navigate(`/tickets/${id}`, {
                  state: { returnTo: "/tickets" },
                });
              }}
            />
          }
        />
        <Route
          path="/tickets/:ticketId"
          element={
            <TicketDetailsScreen
              isChangingPriority={isChangingPriority}
              priorityMessage={priorityMessage}
              onChangePriority={handleChangePriority}
              isSupport={isSupport}
              isReopenTicket={isReopeningTicket}
              reopenTicketMessage={reopenTicketMessage}
              onReopenTicket={handleReopenTicketClick}
              isStartingTicket={isStartingTicket}
              startTicketMessage={startTicketMessage}
              onStartTicket={handleStartTicketClick}
              selectedTicketId={selectedTicketId}
              ticketDetail={ticketDetail}
              isLoadingTicketDetail={isLoadingTicketDetail}
              ticketDetailError={ticketDetailError}
              onBackClick={() => navigate(ticketReturnPath)}
              onRetryClick={refreshTicketDetail}
              onCloseTicket={handleCloseTicketClick}
              isClosingTicket={isClosingTicket}
              closeTicketMessage={closeTicketMessage}
            />
          }
        />
        <Route
          path="/tickets/archived"
          element={
            <ArchivedTicketsScreen
              tickets={tickets}
              pageSize={archivePageSize}
              ticketsError={ticketsError}
              page={archivePage}
              maxPage={maxArchivePage}
              onPreviousPage={() =>
                setArchivePage((previous) => Math.max(1, previous - 1))
              }
              onNextPage={() =>
                setArchivePage((previous) =>
                  Math.min(maxArchivePage, previous + 1),
                )
              }
              isLoadingTickets={isLoadingTickets}
              onRefreshClick={refreshTickets}
              onTicketClick={(id) => {
                navigate(`/tickets/${id}`, {
                  state: { returnTo: "/tickets/archived" },
                });
              }}
              onBackClick={() => navigate("/tickets")}
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
            <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
              <h1>Nie znaleziono strony</h1>
              <button
                type="button"
                className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8]"
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

  return (
    <div className="mx-auto my-10 w-full max-w-[760px] p-4">
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
