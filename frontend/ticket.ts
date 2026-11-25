export type TicketSummary = {
  id: number;
  title: string;
  status: "Open" | "InProgress" | "Closed";
  priority: number;
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
    throw new Error("The API response is not an object");
  }
  if (!("id" in data) || typeof data.id !== "number") {
    throw new Error("Missing or invalid ID");
  }

  if (!("title" in data) || typeof data.title !== "string") {
    throw new Error("Missing or invalid title");
  }
  if (!data.title.trim()) {
    throw new Error("Ticket title cannot be empty");
  }
  if (!("status" in data) || typeof data.status !== "string") {
    throw new Error("Missing or invalid status");
  }
  if (!("priority" in data) || typeof data.priority !== "number") {
    throw new Error("Missing or invalid priority");
  }
  if (data.priority < 1 || data.priority > 5) {
    throw new Error("Priority must be between 1 and 5");
  }
  if (!Number.isInteger(data.priority)) {
    throw new Error("Priority is not an integer.");
  }

  if (
    data.status !== "Open" &&
    data.status !== "InProgress" &&
    data.status !== "Closed"
  ) {
    throw new Error("Invalid ticket status.");
  }

  return {
    id: data.id,
    title: data.title,
    status: data.status,
    priority: data.priority,
  };
}
export function parseTicketDetails(data: unknown): TicketDetail {
  if (typeof data !== "object" || data === null) {
    throw new Error("The API response is not an object");
  }
  if (!("status" in data) || typeof data.status !== "string") {
    throw new Error("Missing or invalid status");
  }
  if (
    data.status !== "Open" &&
    data.status !== "InProgress" &&
    data.status !== "Closed"
  ) {
    throw new Error("Invalid ticket status.");
  }
  if (!("priority" in data) || typeof data.priority !== "number") {
    throw new Error("Missing or invalid priority");
  }
  if (data.priority < 1 || data.priority > 5) {
    throw new Error("Priority must be between 1 and 5");
  }
  if (!Number.isInteger(data.priority)) {
    throw new Error("Priority is not an integer.");
  }
  if (!("id" in data) || typeof data.id !== "number") {
    throw new Error("Missing or invalid ID");
  }
  if (!("title" in data) || typeof data.title !== "string") {
    throw new Error("Missing or invalid title");
  }
  if (!("description" in data) || typeof data.description !== "string") {
    throw new Error("Missing or invalid description");
  }
  if (!data.title.trim() || !data.description.trim()) {
    throw new Error("Title and description are required");
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
    return "Open";
  }
  if (status === "InProgress") {
    return "In progress";
  }
  return "Closed";
}
