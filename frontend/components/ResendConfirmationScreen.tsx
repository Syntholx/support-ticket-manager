import { useState, FormEvent } from "react";
import { getCsrfToken } from "../get-csrf-token";

export function ResendConfirmationScreen(props: { onBackLogin: () => void }) {
  const [isSending, setIsSending] = useState(false);
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
    setIsSending(true);
    setMessage("");
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setMessage(
          "Could not send the request securely. Try again.",
        );
        return;
      }
      const resendConfirmationResponse = await fetch(
        `/api/auth/resend-confirmation`,
        {
          method: "POST",
          credentials: "same-origin",
          headers: {
            "X-CSRF-TOKEN": csrfToken,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ email: email.trim() }),
        },
      );
      if (resendConfirmationResponse.status === 202) {
        setMessage(
          "If your account needs confirmation, you will receive an email with further instructions.",
        );
        return;
      }
      if (resendConfirmationResponse.status === 400) {
        setMessage("Check your email address");
        return;
      }
      if (resendConfirmationResponse.status === 429) {
        setMessage("Too many attempts. Please try again later");
        return;
      }
      setMessage("Could not accept the request. Try again later.");
      return;
    } catch (error) {
      setMessage("Could not connect to the server. Try again.");
      return;
    } finally {
      setIsSending(false);
    }
  }
  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <h1>Resend confirmation</h1>
      <p>Enter the email address you registered with</p>

      <form onSubmit={handleSubmit}>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="email">Email address</label>
          <input
            disabled={isSending}
            className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            id="email"
            name="email"
            type="email"
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
          {isSending ? "Sending…" : "Resend confirmation link"}
        </button>
      </form>
      <p role="status">{message}</p>

      <button
        type="button"
        disabled={isSending}
        onClick={props.onBackLogin}
        className="cursor-pointer mt-[16px] border-none bg-transparent font-semibold text-[#1d4ed8] disabled:text-[#64748b] disabled:cursor-not-allowed"
      >
        Back to sign-in
      </button>
    </section>
  );
}
