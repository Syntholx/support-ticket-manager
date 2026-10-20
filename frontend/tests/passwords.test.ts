import { expect, test } from "vitest";
import { passwordsMatch } from "../validation/passwords";

test("identyczne hasła są równe", () => {
  const password = "przykładowe-hasło";
  const confirmation = "przykładowe-hasło";

  expect(passwordsMatch(password, confirmation)).toBe(true);
});

test("różne hasła nie są równe", () => {
  const passwordOk: string = "hasloktorejestokej";
  const passwordWrong: string = "hasloktoreniejestokej";

  expect(passwordsMatch(passwordOk, passwordWrong)).toBe(false);
});

test("spacja na końcu jest czescia hasła", () => {
  const paswordwithoutspace: string = "przykladowe-haslo";
  const passwordwithspace: string = "przykladowe-haslo ";
  expect(passwordsMatch(paswordwithoutspace, passwordwithspace)).toBe(false);
});
