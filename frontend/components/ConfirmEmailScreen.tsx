import { useState } from "react";
import { getCsrfToken } from "../get-csrf-token";
export function ConfirmEmailScreen(props: { onLoginClick: () => void }) {
  const params = new URLSearchParams(window.location.hash.slice(1));
  const userId = params.get("userId");
  const token = params.get("token");
  const [confirmationMessage, setConfirmationMessage] = useState(
    userId && token
      ? "Przygotowuję potwierdzenie e-mail..."
      : "Link potwierdzający jest nieprawidłowy",
  );
  const [isConfirmed, setConfirmed] = useState(false);
  const [isConfirming, setConfirming] = useState(false);

  async function handleConfirmEmail() {
    if (!userId || !token) return;
    setConfirming(true);
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setConfirmationMessage(
          "Nie udało się bezpiecznie wysłać potwierdzenia e-mail",
        );
        return;
      }
      const confirmEmailRequest = { userId, token };
      const requestEmailBody = JSON.stringify(confirmEmailRequest);
      const confirmEmailResponse = await fetch("/api/auth/confirm-email", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": csrfToken,
        },
        body: requestEmailBody,
        credentials: "same-origin",
      });
      if (confirmEmailResponse.status === 200) {
        window.history.replaceState(null, "", window.location.pathname);
        setConfirmed(true);
        setConfirmationMessage("Adres e-mail potwierdzony");
      } else if (confirmEmailResponse.status === 400) {
        setConfirmationMessage("Link jest nieprawidłowy lub wygasł.");
      } else {
        setConfirmationMessage(
          "Nie udało się potwierdzić adresu e-mail. Spróbuj ponownie.",
        );
      }
    } catch (error) {
      setConfirmationMessage(
        "Nie udało się potwierdzić adresu e-mail, możliwy błąd API, sprawdź połączenie.",
      );
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
        disabled={!userId || !token || isConfirming || isConfirmed}
      >
        Potwierdź e-mail
      </button>
      {isConfirmed && (
        <button type="button" onClick={props.onLoginClick}>
          Przejdź do logowania
        </button>
      )}
    </section>
  );
}
