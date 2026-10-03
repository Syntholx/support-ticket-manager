export type TicketSummary = {
  id: number;
  title: string;
  priority: number;
  status: "Open" | "InProgress";
};
