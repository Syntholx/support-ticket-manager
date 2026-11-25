// @vitest-environment jsdom
import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, expect, test, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { ResetPasswordScreen } from "../components/ResetPasswordScreen";

afterEach(() => {
  cleanup();
  vi.unstubAllGlobals();
});

test("pokazuje error dla różnych haseł", async () => {
  const user = userEvent.setup();
  window.history.replaceState(
    null,
    "",
    "/reset-password#email=test%40example.com&token=testowy-token",
  );
  const fetchMock = vi.fn();
  vi.stubGlobal("fetch", fetchMock);
  render(<ResetPasswordScreen onLoginClick={() => {}} />);
  await user.type(
    screen.getByLabelText("Enter your new password"),
    "PierwszeHaslo123!",
  );
  await user.type(
    screen.getByLabelText("Confirm new password"),
    "DrugieHaslo123",
  );
  await user.click(screen.getByRole("button", { name: "Change password" }));
  expect(screen.getByText("The passwords must match.")).toBeTruthy();
  expect(fetchMock).not.toHaveBeenCalled();
});

test("bez danych linku nie pokazuje formularza", () => {
  window.history.replaceState(null, "", "/reset-password");
  render(<ResetPasswordScreen onLoginClick={() => {}} />);
  expect(screen.queryByLabelText("Enter your new password")).toBeNull();
  expect(
    screen.getByText(
      "The reset link is invalid or incomplete. Request a new email.",
    ),
  ).toBeTruthy();
});

test("sprawdzamy callback logowania, czy jest wywoływany", async () => {
  const onLoginClick = vi.fn();
  const user = userEvent.setup();
  window.history.replaceState(null, "", "reset-password");
  render(<ResetPasswordScreen onLoginClick={onLoginClick} />);
  await user.click(
    screen.getByRole("button", { name: "Go to sign-in" }),
  );
  expect(onLoginClick).toHaveBeenCalledTimes(1);
});

test("po sukcesie resetu ukrywa formularz", async () => {
  const fetchMock = vi.fn();

  fetchMock.mockResolvedValueOnce(
    new Response(JSON.stringify({ token: "testowy-csrf" }), {
      status: 200,
      headers: { "Content-Type": "application/json" },
    }),
  );

  fetchMock.mockResolvedValueOnce(new Response(null, { status: 200 }));

  vi.stubGlobal("fetch", fetchMock);
  const user = userEvent.setup();
  window.history.replaceState(
    null,
    "",
    "/reset-password#email=test%40example.com&token=testowy-token",
  );
  render(<ResetPasswordScreen onLoginClick={() => {}} />);
  expect(screen.getByLabelText("Enter your new password")).toBeTruthy();
  await user.type(screen.getByLabelText("Enter your new password"), "DrugieHaslo123!");
  await user.type(
    screen.getByLabelText("Confirm new password"),
    "DrugieHaslo123!",
  );
  await user.click(screen.getByRole("button", { name: "Change password" }));
  expect(
    await screen.findByText("Password changed. You can now sign in."),
  ).toBeTruthy();
  expect(screen.queryByLabelText("Enter your new password")).toBeNull();
  expect(fetchMock).toHaveBeenCalledTimes(2);
  expect(fetchMock).toHaveBeenNthCalledWith(
    2,
    "/api/auth/reset-password",
    expect.objectContaining({
      method: "POST",
      credentials: "same-origin",
      headers: expect.objectContaining({
        "Content-Type": "application/json",
        "X-CSRF-TOKEN": "testowy-csrf",
      }),
      body: JSON.stringify({
        email: "test@example.com",
        token: "testowy-token",
        password: "DrugieHaslo123!",
      }),
    }),
  );
});
test("po odpowiedzi 400 pokazuje error i zachowuje formularz", async () => {
  const fetchMock = vi.fn();
  fetchMock.mockResolvedValueOnce(
    new Response(JSON.stringify({ token: "testowy-csrf" }), {
      status: 200,
      headers: {
        "Content-Type": "application/json",
      },
    }),
  );
  fetchMock.mockResolvedValueOnce(new Response(null, { status: 400 }));
  vi.stubGlobal("fetch", fetchMock);
  const user = userEvent.setup();
  window.history.replaceState(
    null,
    "",
    "/reset-password#email=test%40example.com&token=testowy-token",
  );
  render(<ResetPasswordScreen onLoginClick={() => {}} />);
  await user.type(screen.getByLabelText("Enter your new password"), "DrugieHaslo123!");
  await user.type(
    screen.getByLabelText("Confirm new password"),
    "DrugieHaslo123!",
  );
  await user.click(screen.getByRole("button", { name: "Change password" }));
  expect(
    await screen.findByText(
      "Could not change the password. The link may be invalid or expired, or the password may not meet the requirements",
    ),
  ).toBeTruthy();
  expect(screen.getByLabelText("Enter your new password")).toBeTruthy();
  expect(fetchMock).toHaveBeenCalledTimes(2);
  const submitButton = screen.getByRole<HTMLButtonElement>("button", {
    name: "Change password",
  });
  expect(submitButton.disabled).toBe(false);
});

test("po błędzie połączenia, zachowuje formularz i pozwala ponowić", async () => {
  const fetchMock = vi.fn();

  fetchMock.mockResolvedValueOnce(
    new Response(JSON.stringify({ token: "testowy-csrf" }), {
      status: 200,
      headers: { "Content-Type": "application/json" },
    }),
  );
  fetchMock.mockRejectedValueOnce(new TypeError("Failed to fetch"));

  vi.stubGlobal("fetch", fetchMock);
  const user = userEvent.setup();
  window.history.replaceState(
    null,
    "",
    "/reset-password#email=test%40example.com&token=testowy-token",
  );
  render(<ResetPasswordScreen onLoginClick={() => {}} />);
  await user.type(screen.getByLabelText("Enter your new password"), "DrugieHaslo123!");
  await user.type(
    screen.getByLabelText("Confirm new password"),
    "DrugieHaslo123!",
  );
  await user.click(screen.getByRole("button", { name: "Change password" }));
  expect(await screen.findByLabelText("Enter your new password")).toBeTruthy();
  expect(
    await screen.findByText(
      "Could not connect to the server. Try again.",
    ),
  ).toBeTruthy();
  const submitButton = screen.getByRole<HTMLButtonElement>("button", {
    name: "Change password",
  });
  expect(submitButton.disabled).toBe(false);
  expect(fetchMock).toHaveBeenCalledTimes(2);
  fetchMock.mockResolvedValueOnce(
    new Response(JSON.stringify({ token: "testowy-csrf" }), {
      status: 200,
      headers: { "Content-Type": "application/json" },
    }),
  );

  fetchMock.mockResolvedValueOnce(new Response(null, { status: 200 }));

  await user.click(screen.getByRole("button", { name: "Change password" }));
  expect(
    await screen.findByText("Password changed. You can now sign in."),
  ).toBeTruthy();
  expect(screen.queryByLabelText("Enter your new password")).toBeNull();
  expect(fetchMock).toHaveBeenCalledTimes(4);
});
