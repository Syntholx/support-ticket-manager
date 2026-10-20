import { useEffect, useState } from "react";
import { getCsrfToken } from "../get-csrf-token";
export function ConfirmEmailScreen(props: { onLoginClick: () => void }) {
  const [confirmationLink] = useState(() => {
    const params = new URLSearchParams(window.location.hash.slice(1));
    return {
      userId: params.get("userId"),
      token: params.get("token"),
    };
  });

  useEffect(() => {
    window.history.replaceState(
      window.history.state,
      "",
      window.location.pathname + window.location.search,
    );
  }, []);

  const { userId, token } = confirmationLink;

  const [confirmationMessage, setConfirmationMessage] = useState(
    userId && token
      ? "Kliknij przycisk, aby potwierdzić adres e-mail"
      : "Link potwierdzający jest nieprawidłowy",
  );
  const [isConfirmed, setConfirmed] = useState(false);
  const [isConfirming, setConfirming] = useState(false);

  async function handleConfirmEmail() {
    if (!userId || !token || isConfirming || isConfirmed) return;
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
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <h1>Potwierdzenie e-maila</h1>
      <p role="status">{confirmationMessage}</p>
      <div className="flex flex-wrap gap-2 ">
        <button
          type="button"
          onClick={handleConfirmEmail}
          disabled={!userId || !token || isConfirming || isConfirmed}
          className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
        >
          {isConfirming ? "Potwierdzanie..." : "Potwierdź e-mail"}
        </button>

        <button
          type="button"
          disabled={isConfirming}
          onClick={props.onLoginClick}
          className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
        >
          Przejdź do logowania
        </button>
      </div>
    </section>
  );
}
