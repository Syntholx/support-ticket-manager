import { useState, FormEvent } from "react";
import { getCsrfToken } from "../get-csrf-token";
export function LoginScreen(props: {
  onBackClick: () => void;
  onLoginSuccess: () => void;
}) {
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
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setLoginMessage("Nie udało się bezpiecznie przygotować logowania.");
        return;
      }
      const loginResponse = await fetch("/api/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": csrfToken,
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
      props.onLoginSuccess();
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
