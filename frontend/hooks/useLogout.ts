import { useState } from "react";
import { getCsrfToken } from "../get-csrf-token";

export function useLogout(onLogoutSuccess: () => void) {
  const [logoutMessage, setLogoutMessage] = useState("");
  const [isLoggingOut, setLoggingOut] = useState(false);
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
      setLogoutMessage("");
      onLogoutSuccess();
    } catch (error) {
      setLogoutMessage(
        "Błąd API lub sieci, sprawdź połączenie i spróbuj ponownie.",
      );
    } finally {
      setLoggingOut(false);
    }
  }
  return {
    isLoggingOut,
    logoutMessage,
    handleLogoutClick,
  };
}
