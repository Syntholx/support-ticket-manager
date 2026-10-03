export async function getCsrfToken(): Promise<string | null> {
  const csrfResponse = await fetch("/api/auth/csrf");
  if (!csrfResponse.ok) {
    return null;
  }

  const csrfData: unknown = await csrfResponse.json();
  if (typeof csrfData !== "object" || csrfData === null) {
    return null;
  }
  if (!("token" in csrfData)) {
    return null;
  }
  if (typeof csrfData.token !== "string" || !csrfData.token.trim()) {
    return null;
  }
  return csrfData.token;
}
