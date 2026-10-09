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
    <section className="welcome-card">
      <button type="button" onClick={prop.onBackClick} className="back-button">
        Wróć do aktywnych
      </button>
      <h1>Archiwum zgłoszeń</h1>
      <p>Zamknięte zgłoszenia</p>
      <button
        type="button"
        onClick={prop.onRefreshClick}
        className="refresh-tickets-button"
        disabled={prop.isLoadingTickets}
      >
        Odswież listę
      </button>
      {prop.ticketsError !== "" && <p>{prop.ticketsError}</p>}
      {prop.isLoadingTickets && <p>Ładowanie archiwum...</p>}
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        (prop.tickets.length === 0 ? (
          <p>Nie masz jeszcze zamkniętych zgłoszeń.</p>
        ) : (
          <ul className="ticket-list">
            {prop.tickets.map((ticket) => (
              <li key={ticket.id}>
                <button
                  type="button"
                  className="ticket-title-button"
                  onClick={() => prop.onTicketClick(ticket.id)}
                >
                  #{ticket.id} - {ticket.title}
                </button>
                <span className="ticket-status">
                  Status: {formatTicketStatus(ticket.status)}
                </span>
              </li>
            ))}
          </ul>
        ))}
    </section>
  );
}
