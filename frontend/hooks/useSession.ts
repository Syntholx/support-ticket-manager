import { useState } from "react";
import { useEffect } from "react";

export function useSession(shouldCheckOnMount: boolean) {
  const [hasSessionError, setSessionError] = useState(false);
  const [sessionMessage, setSessionMessage] = useState("Checking session…");
  const [isAuthenticated, setAuthenticated] = useState(false);
  const [isCheckingSession, setCheckingSession] = useState(shouldCheckOnMount);
  const [isSupport, setSupport] = useState(false);
  async function checkSession() {
    setCheckingSession(true);
    setSupport(false);
    setSessionError(false);
    setSessionMessage("Checking session…");
    try {
      const meResponse = await fetch("/api/auth/me", {
        method: "GET",
        credentials: "same-origin",
      });
      if (meResponse.status === 200) {
        const sessionData: unknown = await meResponse.json();
        if (
          typeof sessionData !== "object" ||
          sessionData === null ||
          !("isSupport" in sessionData) ||
          typeof sessionData.isSupport !== "boolean"
        ) {
          throw new Error("Invalid session response");
        }
        setSupport(sessionData.isSupport);
        setAuthenticated(true);
      } else if (meResponse.status === 401) {
        setAuthenticated(false);
      } else {
        setSessionError(true);
        setSessionMessage(
          "Could not check your session. Check the API and refresh the page.",
        );
      }
    } catch {
      setSessionError(true);
      setSessionMessage(
        "Could not check your session. Check the API and refresh the page.",
      );
    } finally {
      setCheckingSession(false);
    }
  }
  useEffect(() => {
    if (!shouldCheckOnMount) return;
    checkSession();
  }, []);
  return {
    checkSession,
    isAuthenticated,
    isCheckingSession,
    sessionMessage,
    hasSessionError,
    setAuthenticated,
    isSupport,
  };
}
