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
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <button type="button" className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8]" onClick={props.onBackClick}>
        Wróć
      </button>
      <h1>Rejestracja</h1>
      <p role="status">{registerMessage}</p>
      <form onSubmit={handleRegisterSubmit}>
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
            autoComplete="new-password"
            required
          ></input>
        </div>
        <button type="submit" className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white" disabled={isRegistering}>
          Załóż konto
        </button>
      </form>
    </section>
  );
}
