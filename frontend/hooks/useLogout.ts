import { useState } from "react";
import { getCsrfToken } from "../get-csrf-token";

export function useLogout(onLogoutSuccess: () => void) {
  const [logoutMessage, setLogoutMessage] = useState("");
  const [isLoggingOut, setLoggingOut] = useState(false);
  async function handleLogoutClick() {
    setLogoutMessage("Signing out…");
    setLoggingOut(true);
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setLogoutMessage("Could not prepare secure sign-out.");
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
          "Could not sign out. Check your connection and try again.",
        );
        return;
      }
      setLogoutMessage("");
      onLogoutSuccess();
    } catch (error) {
      setLogoutMessage(
        "API or network error. Check your connection and try again.",
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
