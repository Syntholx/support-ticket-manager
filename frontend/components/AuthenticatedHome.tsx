import { TicketSummary } from "../ticket";
export function AuthenticatedHome(prop: {
  onLogoutClick: () => void;
  message: string;
  isLoggingOut: boolean;
  tickets: TicketSummary[];
}) {
  return (
    <section className="welcome-card">
      <h1>Panel użytkownika</h1>
      <p>Witaj! Jesteś zalogowany.</p>
      <h2>Aktywne zgłoszenia</h2>
      {prop.tickets.length === 0 ? (
        <p>Nie masz jeszcze aktywnych zgłoszeń</p>
      ) : (
        <ul>
          {prop.tickets.map((ticket) => (
            <li key={ticket.id}>
              #{ticket.id} - {ticket.title}
            </li>
          ))}
        </ul>
      )}

      <p>{prop.message}</p>
      <button
        type="button"
        onClick={prop.onLogoutClick}
        disabled={prop.isLoggingOut}
      >
        Wyloguj się
      </button>
    </section>
  );
}
