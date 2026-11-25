import { expect, test } from "vitest";
import { passwordsMatch } from "../validation/passwords";

test("treats identical passwords as equal", () => {
  const password = "przykładowe-hasło";
  const confirmation = "przykładowe-hasło";

  expect(passwordsMatch(password, confirmation)).toBe(true);
});

test("treats different passwords as unequal", () => {
  const passwordOk: string = "hasloktorejestokej";
  const passwordWrong: string = "hasloktoreniejestokej";

  expect(passwordsMatch(passwordOk, passwordWrong)).toBe(false);
});

test("treats trailing spaces as part of the password", () => {
  const paswordwithoutspace: string = "przykladowe-haslo";
  const passwordwithspace: string = "przykladowe-haslo ";
  expect(passwordsMatch(paswordwithoutspace, passwordwithspace)).toBe(false);
});
