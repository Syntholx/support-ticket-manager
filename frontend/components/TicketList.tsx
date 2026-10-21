import { TicketStatusBadge } from "../components/TicketStatusBadge";
import { TicketSummary } from "../ticket";
import { TicketPriorityBadge } from "../components/TicketPriorityBadge";
export function TicketList(prop: {
  tickets: TicketSummary[];
  onTicketClick: (id: number) => void;
}) {
  return (
    <ul className="m-0 mt-4 list-none p-0">
      {prop.tickets.map((ticket) => (
        <li
          key={ticket.id}
          className="mb-3 flex w-full flex-col items-start gap-2 rounded-xl border border-solid border-[#dce5ef] bg-white p-3"
        >
          <p className="m-0 text-[12px] text-[#64748b]">#{ticket.id}</p>
          <button
            type="button"
            className="[font-family:inherit] leading-[inherit] inline-block min-h-[44px] max-w-full cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-[#eff6ff] px-3 py-2 text-left text-[16px] font-semibold whitespace-normal [word-break:break-word] text-[#1e40af] hover:bg-[#dbeafe]"
            onClick={() => prop.onTicketClick(ticket.id)}
          >
            {ticket.title}
          </button>
          <div className="flex flex-wrap gap-2">
            <TicketStatusBadge status={ticket.status} />
            <TicketPriorityBadge
              status={ticket.status}
              priority={ticket.priority}
            />
          </div>
        </li>
      ))}
    </ul>
  );
}
