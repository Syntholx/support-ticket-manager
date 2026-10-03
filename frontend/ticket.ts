export type TicketSummary = {
  id: number;
  title: string;
};
export function parseTicket(data: unknown): TicketSummary {
  if (typeof data !== "object" || data === null) {
    throw new Error("Odpowiedź API nie jest obiektem");
  }
  if (!("id" in data) || typeof data.id !== "number") {
    throw new Error("Brak id lub nieprawidłowy typ");
  }

  if (!("title" in data) || typeof data.title !== "string") {
    throw new Error("Brak title lub nieprawidłowy typ");
  }
  if (!data.title.trim()) {
    throw new Error("Title zgłoszenia nie może być puste");
  }
  return {
    id: data.id,
    title: data.title,
  };
}
