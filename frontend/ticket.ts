export type TicketSummary = {
  id: number;
  title: string;
  status: "Open" | "InProgress" | "Closed";
};

export type TicketDetail = {
  id: number;
  title: string;
  description: string;
  priority: number;
  status: "Open" | "InProgress" | "Closed";
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
  if (!("status" in data) || typeof data.status !== "string") {
    throw new Error("Brak status lub nieprawidłowy typ");
  }
  if (
    data.status !== "Open" &&
    data.status !== "InProgress" &&
    data.status !== "Closed"
  ) {
    throw new Error("Status nie zawiera dopuszczalnych statusów.");
  }

  return {
    id: data.id,
    title: data.title,
    status: data.status,
  };
}
export function parseTicketDetails(data: unknown): TicketDetail {
  if (typeof data !== "object" || data === null) {
    throw new Error("Odpowiedź API nie jest obiektem");
  }
  if (!("status" in data) || typeof data.status !== "string") {
    throw new Error("Brak status lub nieprawidłowy typ");
  }
  if (
    data.status !== "Open" &&
    data.status !== "InProgress" &&
    data.status !== "Closed"
  ) {
    throw new Error("Status nie zawiera dopuszczalnych statusów.");
  }
  if (!("priority" in data) || typeof data.priority !== "number") {
    throw new Error("Brak priority lub nieprawidłowy typ");
  }
  if (data.priority < 1 || data.priority > 5) {
    throw new Error("Priority musi mieścić się w przedziale 1 - 5");
  }
  if (!Number.isInteger(data.priority)) {
    throw new Error("Priority nie jest liczbą całkowitą.");
  }
  if (!("id" in data) || typeof data.id !== "number") {
    throw new Error("Brak id lub nieprawidłowy typ");
  }
  if (!("title" in data) || typeof data.title !== "string") {
    throw new Error("Brak title lub nieprawidłowy typ");
  }
  if (!("description" in data) || typeof data.description !== "string") {
    throw new Error("Brak description lub nieprawidłowy typ");
  }
  if (!data.title.trim() || !data.description.trim()) {
    throw new Error("Title lub opis nie może być puste");
  }

  return {
    id: data.id,
    title: data.title,
    description: data.description,
    priority: data.priority,
    status: data.status,
  };
}

export function formatTicketStatus(
  status: "Open" | "InProgress" | "Closed",
): string {
  if (status === "Open") {
    return "Otwarte";
  }
  if (status === "InProgress") {
    return "W trakcie";
  }
  return "Zamknięte";
}
