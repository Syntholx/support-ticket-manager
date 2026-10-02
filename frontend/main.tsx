import { createRoot } from "react-dom/client";
import { useState } from "react";
import { parseApiStatus } from "./api-status.js";
import { FormEvent } from "react";
const app = document.getElementById("app");
if (!(app instanceof HTMLElement)) {
  throw new Error("Nie znaleziono elementu");
}
function GuestHome(props: {
  onLoginClick: () => void;
  onRegisterClick: () => void;
}) {
  return (
    <section className="welcome-card">
      <h1>Support Ticket Manager</h1>
      <p>Zaloguj się, aby przeglądać, tworzyć i sledzić zgłoszenia.</p>
      <button type="button" onClick={props.onLoginClick}>
        Zaloguj się
      </button>
      <button type="button" onClick={props.onRegisterClick}>
        Utwórz konto
      </button>
    </section>
  );
}
function RegisterScreen(props: { onBackClick: () => void }) {
  const [registerMessage, setRegisterMessage] = useState(
    "Wpisz e-mail i hasło, aby utworzyć konto.",
  );
  const [isRegistering, setRegistering] = useState(false);
  async function handleRegisterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const registerFormData = new FormData(event.currentTarget);
    const email = registerFormData.get("email");
    if (typeof email !== "string") {
      return;
    }
    const password = registerFormData.get("password");
    if (typeof password !== "string") {
      return;
    }
    const registerRequest = { email: email, password: password };
    const registerRequestBody = JSON.stringify(registerRequest);
    setRegisterMessage("Przygotowujemy bezpieczną rejestrację.");
    setRegistering(true);
    try {
      const authResponse = await fetch("/api/auth/csrf");
      if (!authResponse.ok) {
        setRegisterMessage(
          "Nie udało się przygotować rejestracji. Spróbuj ponownie.",
        );
        return;
      }

      const csrfData: unknown = await authResponse.json();
      if (typeof csrfData !== "object" || csrfData === null) {
        setRegisterMessage("Nieprawidłowa odpowiedź API.");
        return;
      }
      if (!("token" in csrfData)) {
        setRegisterMessage("Nieprawidłowa odpowiedź API");
        return;
      }
      if (typeof csrfData.token !== "string" || !csrfData.token.trim()) {
        setRegisterMessage("Nieprawidłowa odpowiedź API");
        return;
      }
      const registerResponse = await fetch("/api/auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": csrfData.token,
        },
        body: registerRequestBody,
        credentials: "same-origin",
      });
      if (registerResponse.status === 201) {
        setRegisterMessage(
          "Konto utworzone. Sprawdź wiadomość w Mailpit i potwierdź e-mail.",
        );
      } else if (registerResponse.status === 503) {
        setRegisterMessage(
          "Konto utworzone, ale nie udało się wysłać mail-a potwierdzającego.",
        );
      } else {
        setRegisterMessage("Nie udało się utworzyć konta.");
      }
    } catch (error) {
      setRegisterMessage("Nie udało się połączyć z API TSM.");
    } finally {
      setRegistering(false);
    }
  }

  return (
    <section className="login-card">
      <button type="button" className="back-button" onClick={props.onBackClick}>
        Wróć
      </button>
      <h1>Rejestracja</h1>
      <p role="status">{registerMessage}</p>
      <form onSubmit={handleRegisterSubmit}>
        <div className="form-field">
          <label htmlFor="email">Podaj adres e-mail</label>
          <input
            type="email"
            id="email"
            name="email"
            autoComplete="username"
            required
          ></input>
        </div>
        <div className="form-field">
          <label htmlFor="password">Podaj hasło</label>
          <input
            type="password"
            id="password"
            name="password"
            autoComplete="new-password"
            required
          ></input>
        </div>
        <button type="submit" className="login-button" disabled={isRegistering}>
          Załóż konto
        </button>
      </form>
    </section>
  );
}
function LoginScreen(props: { onBackClick: () => void }) {
  const [isLoggingIn, setLoggingIn] = useState(false);
  const [loginMessage, setLoginMessage] = useState(
    "Wpisz e-mail i hasło, aby się zalogować.",
  );
  async function handleLoginSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const email = formData.get("email");
    if (typeof email !== "string") {
      return;
    }
    const password = formData.get("password");
    if (typeof password !== "string") {
      return;
    }
    const loginRequest = { email: email, password: password };
    const requestBody = JSON.stringify(loginRequest);
    setLoginMessage("Przygotowuję bezpieczne logowanie...");
    setLoggingIn(true);
    try {
      const csrfResponse = await fetch("/api/auth/csrf");
      if (!csrfResponse.ok) {
        setLoginMessage(
          "Nie udało się przygotować logowania. Spróbuj ponownie.",
        );
        return;
      }

      const csrfData: unknown = await csrfResponse.json();
      if (typeof csrfData !== "object" || csrfData === null) {
        setLoginMessage("Nieprawidłowa odpowiedź API.");
        return;
      }
      if (!("token" in csrfData)) {
        setLoginMessage("Nieprawidłowa odpowiedź API");
        return;
      }
      if (typeof csrfData.token !== "string" || !csrfData.token.trim()) {
        setLoginMessage("Nieprawidłowa odpowiedź API");
        return;
      }
      const loginResponse = await fetch("/api/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": csrfData.token,
        },
        body: requestBody,
        credentials: "same-origin",
      });
      if (loginResponse.status === 401) {
        setLoginMessage(
          "Nie udało się zalogować, sprawdź dane lub stan konta.",
        );
        return;
      }
      if (!loginResponse.ok) {
        setLoginMessage(
          "Logowanie nie powiodło się, spróbuj ponownie później.",
        );
        return;
      }
      setLoginMessage("Sprawdzam sesję...");
      const meResponse = await fetch("/api/auth/me", {
        method: "GET",
        credentials: "same-origin",
      });
      if (!meResponse.ok) {
        setLoginMessage(
          "Logowanie przyjęte, ale nie udało się potwierdzić sesji.",
        );
        return;
      }
      setLoginMessage("Zalogowano. Sesja działa.");
    } catch (error) {
      setLoginMessage("Nie udało się połączyć z API TSM.");
    } finally {
      setLoggingIn(false);
    }
  }

  return (
    <section className="login-card">
      <button type="button" className="back-button" onClick={props.onBackClick}>
        Wróć
      </button>
      <h1>Logowanie</h1>
      <p>{loginMessage}</p>
      <form onSubmit={handleLoginSubmit}>
        <div className="form-field">
          <label htmlFor="email">Podaj adres e-mail</label>
          <input
            type="email"
            id="email"
            name="email"
            autoComplete="username"
            required
          ></input>
        </div>
        <div className="form-field">
          <label htmlFor="password">Podaj hasło</label>
          <input
            type="password"
            id="password"
            name="password"
            autoComplete="current-password"
            required
          ></input>
        </div>
        <button type="submit" className="login-button" disabled={isLoggingIn}>
          Zaloguj się
        </button>
      </form>
    </section>
  );
}

function App() {
  const [screen, setScreen] = useState<
    "guest" | "login" | "register" | "confirm"
  >(window.location.pathname === "/confirm-email" ? "confirm" : "guest");
  return (
    <div className="app-shell">
      {screen === "login" ? (
        <LoginScreen onBackClick={() => setScreen("guest")} />
      ) : screen === "register" ? (
        <RegisterScreen onBackClick={() => setScreen("guest")} />
      ) : screen === "confirm" ? (
        <ConfirmEmailScreen />
      ) : (
        <GuestHome
          onLoginClick={() => setScreen("login")}
          onRegisterClick={() => setScreen("register")}
        />
      )}
      <StatusPanel title="Status API" />
    </div>
  );
}
createRoot(app).render(<App />);

function StatusPanel(props: { title: string }) {
  const [message, setMessage] = useState("Nie sprawdzono połączenia z TSM");
  const [isLoading, setIsLoading] = useState(false);
  async function handleCheckConnection() {
    try {
      setMessage("Łaczenie z API TSM....");
      setIsLoading(true);
      const response = await fetch("api/status");
      if (!response.ok) {
        setMessage("API zwróciło błąd");
        return;
      }
      const data: unknown = await response.json();
      const { name, version, isRunning } = parseApiStatus(data);
      if (isRunning === true) {
        setMessage(`${name} — wersja ${version} — API działa poprawnie`);
      } else {
        setMessage(`${name} — wersja ${version} — API zgłasza, że nie działa.`);
      }
    } catch (error) {
      setMessage("Nie udało się połączyć");
    } finally {
      setIsLoading(false);
    }
  }
  return (
    <section className="status-card">
      <h2>{props.title}</h2>
      <StatusMessage text={message} />
      <button
        type="button"
        onClick={handleCheckConnection}
        disabled={isLoading}
      >
        {isLoading ? "Sprawdzanie..." : "Sprawdź połączenie z TSM"}
      </button>
    </section>
  );
}
function StatusMessage(props: { text: string }) {
  return <p role="status">{props.text}</p>;
}

function ConfirmEmailScreen() {
  const params = new URLSearchParams(window.location.hash.slice(1));
  const userId = params.get("userId");
  const token = params.get("token");
  const [confirmationMessage, setConfirmationMessage] = useState(
    userId && token
      ? "Przygotowuję potwierdzenie e-mail..."
      : "Link potwierdzający jest nieprawidłowy",
  );

  const [isConfirming, setConfirming] = useState(false);

  async function handleConfirmEmail() {
    if (!userId || !token) return;
    setConfirming(true);
    try {
    } catch (error) {
      setConfirmationMessage("Nie udało się potwierdzić adresu e-mail");
    } finally {
      setConfirming(false);
    }
  }

  return (
    <section>
      <h1>Potwierdzenie e-maila</h1>
      <p>{confirmationMessage}</p>
      <button
        type="button"
        onClick={handleConfirmEmail}
        disabled={!userId || !token || isConfirming}
      >
        Potwierdź e-mail
      </button>
    </section>
  );
}
