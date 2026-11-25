import { useState, FormEvent } from "react";
import { getCsrfToken } from "../get-csrf-token";
export function RegisterScreen(props: { onBackClick: () => void }) {
  const [registerMessage, setRegisterMessage] = useState(
    "Enter your email and password to create an account.",
  );
  const [isRegistering, setRegistering] = useState(false);
  async function handleRegisterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const registerFormData = new FormData(event.currentTarget);
    const email = registerFormData.get("email");
    if (typeof email !== "string") {
      return;
    }
    const password = registerFormData.get("password");
    if (typeof password !== "string") {
      return;
    }
    const registerRequest = { email: email, password: password };
    const registerRequestBody = JSON.stringify(registerRequest);
    setRegisterMessage("Preparing secure registration.");
    setRegistering(true);
    try {
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setRegisterMessage(
          "Could not prepare secure registration.",
        );
        return;
      }
      const registerResponse = await fetch("/api/auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": csrfToken,
        },
        body: registerRequestBody,
        credentials: "same-origin",
      });
      if (registerResponse.status === 201) {
        setRegisterMessage(
          "Account created. Check Mailpit and confirm your email.",
        );
      } else if (registerResponse.status === 503) {
        setRegisterMessage(
          "Account created, but the confirmation email could not be sent.",
        );
      } else {
        setRegisterMessage("Could not create the account.");
      }
    } catch (error) {
      setRegisterMessage("Could not connect to the TSM API.");
    } finally {
      setRegistering(false);
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
      <h1>Register</h1>
      <p role="status">{registerMessage}</p>
      <form onSubmit={handleRegisterSubmit}>
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
            autoComplete="new-password"
            required
          ></input>
        </div>
        <button
          type="submit"
          className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
          disabled={isRegistering}
        >
          {isRegistering ? "Register..." : "Create account"}
        </button>
      </form>
    </section>
  );
}
