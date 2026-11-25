import { FormEvent, useState } from "react";
import { getCsrfToken } from "../get-csrf-token";

export function ForgotPasswordScreen(props: { onBackClick: () => void }) {
  const [isSending, setSending] = useState(false);
  const [message, setMessage] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (isSending) return;
    const formData = new FormData(event.currentTarget);
    const email = formData.get("email");
    if (typeof email !== "string") return;
    if (email.trim() === "") {
      setMessage("Enter your email address");
      return;
    }
    setSending(true);
    setMessage("Requesting a password reset…");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setMessage(
          "Could not prepare a secure password reset request. Try again.",
        );
        return;
      }
      const resetPasswordResponse = await fetch(`/api/auth/forgot-password`, {
        method: "POST",
        credentials: "same-origin",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": csrfToken,
        },
        body: JSON.stringify({ email: email.trim() }),
      });
      if (resetPasswordResponse.status === 202) {
        setMessage(
          "If your account requires this action, you will receive an email with further instructions.",
        );
        return;
      }
      if (resetPasswordResponse.status === 400) {
        setMessage("Check your email address");
        return;
      }
      if (resetPasswordResponse.status === 429) {
        setMessage("Too many attempts. Please try again later.");
        return;
      }

      setMessage(
        "Could not accept the password reset request. Please try again later.",
      );
      return;
    } catch (error) {
      setMessage(
        "Connection error. Check your connection and try again.",
      );
      return;
    } finally {
      setSending(false);
    }
  }

  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <button
        type="button"
        disabled={isSending}
        className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8] disabled:text-[#64748b] disabled:cursor-not-allowed"
        onClick={props.onBackClick}
      >
        Back
      </button>
      <h1>Forgot password</h1>
      <p>
        Enter your email address. If your account requires this action, we will email you
        further instructions.
      </p>
      <form onSubmit={handleSubmit}>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="email">Enter your email address.</label>
          <input
            disabled={isSending}
            className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            type="email"
            id="email"
            name="email"
            required
            maxLength={254}
            autoComplete="email"
          ></input>
        </div>
        <button
          type="submit"
          disabled={isSending}
          className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
        >
          {isSending ? "Sending…" : "Send reset link"}
        </button>
      </form>
      <p role="status">{message}</p>
    </section>
  );
}
