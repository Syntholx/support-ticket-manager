import { formatTicketStatus, TicketSummary } from "../ticket";
export function TicketStatusBadge(prop: { status: TicketSummary["status"] }) {
  return (
    <span
      className={`m-0 inline-block rounded-full px-2.5 py-1 text-[14px] font-semibold ${getStatusClasses(prop.status)}`}
    >
      Status: {formatTicketStatus(prop.status)}
    </span>
  );
}

function getStatusClasses(status: TicketSummary["status"]): string {
  if (status === "Open") {
    return "bg-[#dbeafe] text-[#1e40af]";
  }
  if (status === "InProgress") {
    return "bg-[#fef3c7] text-[#92400e]";
  }

  return "bg-[#e2e8f0] text-[#475569]";
}
