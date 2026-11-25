// @vitest-environment jsdom
import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, expect, test, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { ResendConfirmationScreen } from "../components/ResendConfirmationScreen";

afterEach(() => {
  cleanup();
  vi.unstubAllGlobals();
});

test("shows a message and keeps the form after HTTP 202", async () => {
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
    screen.getByLabelText("Email address"),
    "testowanie@example.com",
  );
  await user.click(
    screen.getByRole("button", { name: "Resend confirmation link" }),
  );
  expect(
    await screen.findByText(
      "If your account needs confirmation, you will receive an email with further instructions.",
    ),
  ).toBeTruthy();
  const submitButton = screen.getByRole<HTMLButtonElement>("button", {
    name: "Resend confirmation link",
  });
  expect(screen.getByLabelText("Email address")).toBeTruthy();
  expect(submitButton.disabled).toBe(false);
  expect(fetchMock).toHaveBeenCalledTimes(2);
});
