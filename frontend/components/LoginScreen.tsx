import { useState, FormEvent } from "react";
import { getCsrfToken } from "../get-csrf-token";
import { isDemoMode } from "../demo";
export function LoginScreen(props: {
  onBackClick: () => void;
  onLoginSuccess: () => void;
  onForgotPasswordClick: () => void;
  onResendConfirmationClick: () => void;
}) {
  const [isLoggingIn, setLoggingIn] = useState(false);
  const [loginMessage, setLoginMessage] = useState(
    "Enter your email and password to sign in.",
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
    setLoginMessage("Preparing secure sign-in…");
    setLoggingIn(true);
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setLoginMessage("Could not prepare secure sign-in.");
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
          "Could not sign in. Check your credentials and account status.",
        );
        return;
      }
      if (!loginResponse.ok) {
        setLoginMessage(
          "Sign-in failed. Try again later.",
        );
        return;
      }
      setLoginMessage("Checking session…");
      props.onLoginSuccess();
    } catch (error) {
      setLoginMessage("Could not connect to the TSM API.");
    } finally {
      setLoggingIn(false);
    }
  }

  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <button
        type="button"
        className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8]"
        onClick={props.onBackClick}
      >
        Back
      </button>
      <h1>Sign in</h1>
      <p>{loginMessage}</p>
      <form onSubmit={handleLoginSubmit}>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="email">Enter your email address</label>
          <input
            className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            type="email"
            id="email"
            name="email"
            autoComplete="username"
            required
          ></input>
        </div>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="password">Enter your password</label>
          <input
            className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            type="password"
            id="password"
            name="password"
            autoComplete="current-password"
            required
          ></input>
        </div>
        <button
          type="submit"
          className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
          disabled={isLoggingIn}
        >
          {isLoggingIn ? "Sign in..." : "Sign in"}
        </button>
      </form>
      {!isDemoMode && (
        <>
          <button
            type="button"
            className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8] mt-[16px]"
            onClick={props.onForgotPasswordClick}
          >
            Forgot password
          </button>
          <button
            type="button"
            className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8] mt-[16px]"
            onClick={props.onResendConfirmationClick}
          >
            Resend confirmation email
          </button>
        </>
      )}
    </section>
  );
}
