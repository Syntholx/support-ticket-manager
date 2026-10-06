import { TicketDetail } from "../ticket";
import { formatTicketStatus } from "../ticket";

export function TicketDetailsScreen(props: {
  selectedTicketId: number | null;
  ticketDetail: TicketDetail | null;
  onBackClick: () => void;
  isLoadingTicketDetail: boolean;
  ticketDetailError: string;
  onRetryClick: () => void;
  onCloseTicket: () => void;
  isClosingTicket: boolean;
  closeTicketMessage: string;
}) {
  return (
    <section className="welcome-card">
      <button type="button" className="back-button" onClick={props.onBackClick}>
        Wróć
      </button>
      <h1>Zgłoszenie #{props.selectedTicketId}</h1>
      {props.isLoadingTicketDetail && <p>Ładowanie szczegółów...</p>}

      {!props.isLoadingTicketDetail && props.ticketDetailError !== "" && (
        <>
          <p>{props.ticketDetailError}</p>
          <button
            type="button"
            className="login-button"
            onClick={props.onRetryClick}
          >
            Spróbuj ponownie
          </button>
        </>
      )}
      {!props.isLoadingTicketDetail &&
        props.ticketDetailError === "" &&
        props.ticketDetail !== null && (
          <>
            <h2>{props.ticketDetail.title}</h2>
            <p className="ticket-status">
              Status: {formatTicketStatus(props.ticketDetail.status)}
            </p>
            <p className="priority-ticket">
              Priorytet {props.ticketDetail.priority}/5
            </p>
            <p>{props.ticketDetail.description}</p>
            {props.ticketDetail.status !== "Closed" && (
              <button
                type="button"
                className="login-button"
                onClick={props.onCloseTicket}
                disabled={props.isClosingTicket}
              >
                Zamknij zgłoszenie
              </button>
            )}
            <p>{props.closeTicketMessage}</p>
          </>
        )}
    </section>
  );
}
