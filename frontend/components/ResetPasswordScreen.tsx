import { useState, useEffect, FormEvent } from "react";
import { getCsrfToken } from "../get-csrf-token";
import { passwordsMatch } from "../validation/passwords";

export function ResetPasswordScreen(props: { onLoginClick: () => void }) {
  const [resetLink] = useState(() => {
    const params = new URLSearchParams(window.location.hash.slice(1));
    return {
      email: params.get("email"),
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
  const hasResetLink = Boolean(resetLink.email?.trim() && resetLink.token);
  const [message, setMessage] = useState("");
  const [isResetting, setIsResetting] = useState(false);
  const [isResetSuccess, setResetSuccess] = useState(false);
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (isResetting || isResetSuccess) return;

    const formData = new FormData(event.currentTarget);
    const password = formData.get("password");
    const newPassword = formData.get("confirm-password");

    if (typeof password !== "string" || typeof newPassword !== "string") {
      return;
    }

    if (!passwordsMatch(password, newPassword)) {
      setMessage("The passwords must match.");
      return;
    }

    setIsResetting(true);
    setMessage("");

    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setMessage("Could not prepare the request. Try again.");
        return;
      }
      const resetPasswordResponse = await fetch(`/api/auth/reset-password`, {
        method: "POST",
        credentials: "same-origin",
        headers: {
          "X-CSRF-TOKEN": csrfToken,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          email: resetLink.email,
          token: resetLink.token,
          password: password,
        }),
      });
      if (resetPasswordResponse.status === 200) {
        setResetSuccess(true);
        setMessage("Password changed. You can now sign in.");
        return;
      }
      if (resetPasswordResponse.status === 400) {
        setMessage(
          "Could not change the password. The link may be invalid or expired, or the password may not meet the requirements",
        );
        return;
      }
      if (resetPasswordResponse.status === 429) {
        setMessage("Too many attempts. Please try again later.");
        return;
      }
      setMessage("Could not change your password. Try again later");
      return;
    } catch (error) {
      setMessage("Could not connect to the server. Try again.");
    } finally {
      setIsResetting(false);
    }
  }
  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <h1>Set a new password</h1>
      <p>Enter a new password for your account.</p>
      {!hasResetLink && (
        <p>
          The reset link is invalid or incomplete. Request a new
          email.
        </p>
      )}

      {hasResetLink && !isResetSuccess && (
        <form onSubmit={handleSubmit}>
          <div className="flex flex-col mb-[16px] gap-[6px]">
            <label htmlFor="password">Enter your new password</label>

            <input
              disabled={isResetting}
              className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
              id="password"
              name="password"
              type="password"
              required
              minLength={12}
              maxLength={128}
              autoComplete="new-password"
            ></input>
            <p className="text-[14px] text-[#64748b] m-0">
              12–128 characters, including an uppercase letter, a lowercase letter, a digit and a special character.
            </p>
          </div>
          <div className="flex flex-col mb-[16px] gap-[6px]">
            <label htmlFor="confirm-password">Confirm new password</label>
            <input
              disabled={isResetting}
              className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
              id="confirm-password"
              name="confirm-password"
              type="password"
              required
              minLength={12}
              maxLength={128}
              autoComplete="new-password"
            ></input>
          </div>
          <button
            disabled={isResetting}
            type="submit"
            className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
          >
            {isResetting ? "Changing password…" : "Change password"}
          </button>
        </form>
      )}
      <p role="status">{message}</p>
      <button
        type="button"
        disabled={isResetting}
        onClick={props.onLoginClick}
        className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8] mt-[16px] disabled:text-[#64748b] disabled:cursor-not-allowed"
      >
        Go to sign-in
      </button>
    </section>
  );
}
