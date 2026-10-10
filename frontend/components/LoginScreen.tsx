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
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <button type="button" className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8]" onClick={props.onBackClick}>
        Wróć
      </button>
      <h1>Logowanie</h1>
      <p>{loginMessage}</p>
      <form onSubmit={handleLoginSubmit}>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="email">Podaj adres e-mail</label>
          <input className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            type="email"
            id="email"
            name="email"
            autoComplete="username"
            required
          ></input>
        </div>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="password">Podaj hasło</label>
          <input className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            type="password"
            id="password"
            name="password"
            autoComplete="current-password"
            required
          ></input>
        </div>
        <button type="submit" className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white" disabled={isLoggingIn}>
          Zaloguj się
        </button>
      </form>
    </section>
  );
}
