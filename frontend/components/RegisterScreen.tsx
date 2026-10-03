import { useState, FormEvent } from "react";
import { getCsrfToken } from "../get-csrf-token";
export function RegisterScreen(props: { onBackClick: () => void }) {
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
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setRegisterMessage(
          "Nie udało się przygotować bezpiecznie rejestracji.",
        );
        return;
      }
      const registerResponse = await fetch("/api/auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": csrfToken,
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
