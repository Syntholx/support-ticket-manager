import { TicketSummary } from "../ticket";

export function TicketPriorityBadge(props: {
  priority: number;
  status: TicketSummary["status"];
}) {
  return (
    <span
      className={`m-0 inline-block rounded-full px-2.5 py-1 text-[14px] font-semibold 
                ${
                  props.status === "Closed"
                    ? "bg-[#f1f5f9] text-[#334155]"
                    : getPriorityClasses(props.priority)
                }`}
    >
      {props.status === "Closed" ? "Priorytet zgłoszenia" : "Priorytet"}:{" "}
      {props.priority}/5
    </span>
  );
}

function getPriorityClasses(priority: TicketSummary["priority"]): string {
  if (priority >= 4) {
    return "bg-[#fee2e2] text-[#991b1b]";
  }

  return "bg-[#f1f5f9] text-[#334155]";
}
