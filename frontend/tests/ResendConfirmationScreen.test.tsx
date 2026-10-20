// @vitest-environment jsdom
import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, expect, test, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { ResendConfirmationScreen } from "../components/ResendConfirmationScreen";

afterEach(() => {
  cleanup();
  vi.unstubAllGlobals();
});

test("po 202 pokazuje komunikat i zachowuje formularz", async () => {
  const fetchMock = vi.fn();
  fetchMock.mockResolvedValueOnce(
    new Response(JSON.stringify({ token: "testowy-csrf" }), {
      status: 200,
      headers: {
        "Content-Type": "application/json",
      },
    }),
  );

  fetchMock.mockResolvedValueOnce(new Response(null, { status: 202 }));
  vi.stubGlobal("fetch", fetchMock);
  const user = userEvent.setup();
  window.history.replaceState(null, "", "/resend-confirmation");
  render(<ResendConfirmationScreen onBackLogin={() => {}} />);
  await user.type(
    screen.getByLabelText("Adres e-mail"),
    "testowanie@example.com",
  );
  await user.click(
    screen.getByRole("button", { name: "Wyślij link potwierdzający ponownie" }),
  );
  expect(
    await screen.findByText(
      "Jeśli konto wymaga potwierdzenia, otrzymasz wiadomość z dalszymi instrukcjami.",
    ),
  ).toBeTruthy();
  const submitButton = screen.getByRole<HTMLButtonElement>("button", {
    name: "Wyślij link potwierdzający ponownie",
  });
  expect(screen.getByLabelText("Adres e-mail")).toBeTruthy();
  expect(submitButton.disabled).toBe(false);
  expect(fetchMock).toHaveBeenCalledTimes(2);
});
