import { TicketSummary, formatTicketStatus } from "../ticket";
export function ArchivedTicketsScreen(prop: {
  tickets: TicketSummary[];
  isLoadingTickets: boolean;
  ticketsError: string;
  onRefreshClick: () => void;
  onTicketClick: (id: number) => void;
  onBackClick: () => void;
}) {
  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <button type="button" onClick={prop.onBackClick} className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8]">
        Wróć do aktywnych
      </button>
      <h1>Archiwum zgłoszeń</h1>
      <p>Zamknięte zgłoszenia</p>
      <button
        type="button"
        onClick={prop.onRefreshClick}
        className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        disabled={prop.isLoadingTickets}
      >
        {prop.isLoadingTickets ? "Odświeżanie..." : "Odśwież listę"}
      </button>
      {prop.ticketsError !== "" && <p>{prop.ticketsError}</p>}
      {prop.isLoadingTickets && <p>Ładowanie archiwum...</p>}
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        (prop.tickets.length === 0 ? (
          <p>Nie masz jeszcze zamkniętych zgłoszeń.</p>
        ) : (
          <ul className="m-0 mt-4 list-none p-0">
            {prop.tickets.map((ticket) => (
              <li key={ticket.id} className="mb-3 flex w-full flex-wrap items-center gap-2 rounded-xl border border-solid border-[#dce5ef] bg-white p-3">
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
        ))}
    </section>
  );
}
