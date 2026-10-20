// @vitest-environment jsdom
import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, expect, test, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { ResetPasswordScreen } from "../components/ResetPasswordScreen";

afterEach(() => {
  cleanup();
  vi.unstubAllGlobals();
});

test("pokazuje błąd dla różnych haseł", async () => {
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
    screen.getByLabelText("Podaj nowe hasło"),
    "PierwszeHaslo123!",
  );
  await user.type(
    screen.getByLabelText("Powtórz nowe hasło"),
    "DrugieHaslo123",
  );
  await user.click(screen.getByRole("button", { name: "Zmień hasło" }));
  expect(screen.getByText("Podane hasła muszą być identyczne.")).toBeTruthy();
  expect(fetchMock).not.toHaveBeenCalled();
});

test("bez danych linku nie pokazuje formularza", () => {
  window.history.replaceState(null, "", "/reset-password");
  render(<ResetPasswordScreen onLoginClick={() => {}} />);
  expect(screen.queryByLabelText("Podaj nowe hasło")).toBeNull();
  expect(
    screen.getByText(
      "Link resetujący jest nieprawidłowy lub brakuje jego danych. Zamów nową wiadomość.",
    ),
  ).toBeTruthy();
});

test("sprawdzamy callback logowania, czy jest wywoływany", async () => {
  const onLoginClick = vi.fn();
  const user = userEvent.setup();
  window.history.replaceState(null, "", "reset-password");
  render(<ResetPasswordScreen onLoginClick={onLoginClick} />);
  await user.click(
    screen.getByRole("button", { name: "Przejdź do logowania" }),
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
  expect(screen.getByLabelText("Podaj nowe hasło")).toBeTruthy();
  await user.type(screen.getByLabelText("Podaj nowe hasło"), "DrugieHaslo123!");
  await user.type(
    screen.getByLabelText("Powtórz nowe hasło"),
    "DrugieHaslo123!",
  );
  await user.click(screen.getByRole("button", { name: "Zmień hasło" }));
  expect(
    await screen.findByText("Hasło zostało zmienione. Możesz się zalogować."),
  ).toBeTruthy();
  expect(screen.queryByLabelText("Podaj nowe hasło")).toBeNull();
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
test("po odpowiedzi 400 pokazuje błąd i zachowuje formularz", async () => {
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
  await user.type(screen.getByLabelText("Podaj nowe hasło"), "DrugieHaslo123!");
  await user.type(
    screen.getByLabelText("Powtórz nowe hasło"),
    "DrugieHaslo123!",
  );
  await user.click(screen.getByRole("button", { name: "Zmień hasło" }));
  expect(
    await screen.findByText(
      "Nie udało się zmienić hasła. Link może być nieprawidłowy lub wygasły albo hasło nie spełnia wymagań",
    ),
  ).toBeTruthy();
  expect(screen.getByLabelText("Podaj nowe hasło")).toBeTruthy();
  expect(fetchMock).toHaveBeenCalledTimes(2);
  const submitButton = screen.getByRole<HTMLButtonElement>("button", {
    name: "Zmień hasło",
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
  await user.type(screen.getByLabelText("Podaj nowe hasło"), "DrugieHaslo123!");
  await user.type(
    screen.getByLabelText("Powtórz nowe hasło"),
    "DrugieHaslo123!",
  );
  await user.click(screen.getByRole("button", { name: "Zmień hasło" }));
  expect(await screen.findByLabelText("Podaj nowe hasło")).toBeTruthy();
  expect(
    await screen.findByText(
      "Nie udało się połączyć z serwerem. Spróbuj ponownie.",
    ),
  ).toBeTruthy();
  const submitButton = screen.getByRole<HTMLButtonElement>("button", {
    name: "Zmień hasło",
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

  await user.click(screen.getByRole("button", { name: "Zmień hasło" }));
  expect(
    await screen.findByText("Hasło zostało zmienione. Możesz się zalogować."),
  ).toBeTruthy();
  expect(screen.queryByLabelText("Podaj nowe hasło")).toBeNull();
  expect(fetchMock).toHaveBeenCalledTimes(4);
});
