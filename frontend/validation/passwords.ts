export function passwordsMatch(
  password: string,
  confirmation: string,
): boolean {
  const passed: boolean = password === confirmation;
  return passed;
}
