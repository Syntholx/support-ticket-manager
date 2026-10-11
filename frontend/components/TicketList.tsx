import { formatTicketStatus, TicketSummary } from "../ticket";
export function TicketList(prop: {
  tickets: TicketSummary[];
  onTicketClick: (id: number) => void;
}) {
  return (
    <ul className="m-0 mt-4 list-none p-0">
      {prop.tickets.map((ticket) => (
        <li
          key={ticket.id}
          className="mb-3 flex w-full flex-wrap items-center gap-2 rounded-xl border border-solid border-[#dce5ef] bg-white p-3"
        >
          <button
            type="button"
            className="[font-family:inherit] leading-[inherit] inline-block min-h-[44px] max-w-full cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-[#eff6ff] px-3 py-2 text-left text-[14px] font-semibold whitespace-normal [word-break:break-word] text-[#1e40af] hover:bg-[#dbeafe]"
            onClick={() => prop.onTicketClick(ticket.id)}
          >
            #{ticket.id} - {ticket.title}
          </button>
          <span className="m-0 inline-block rounded-full bg-[#f1f5f9] px-2.5 py-1 text-[14px] font-semibold text-[#334155]">
            Status: {formatTicketStatus(ticket.status)}
          </span>
        </li>
      ))}
    </ul>
  );
}
