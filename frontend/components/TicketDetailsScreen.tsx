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
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <button
        type="button"
        className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8]"
        onClick={props.onBackClick}
        disabled={props.isClosingTicket}
      >
        Wróć
      </button>
      <h1>Zgłoszenie #{props.selectedTicketId}</h1>
      {props.isLoadingTicketDetail && <p>Ładowanie szczegółów...</p>}

      {!props.isLoadingTicketDetail && props.ticketDetailError !== "" && (
        <>
          <p>{props.ticketDetailError}</p>
          <button
            type="button"
            className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white"
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
            <p className="m-0 mb-4 mr-2 inline-block rounded-full bg-[#f1f5f9] px-2.5 py-1 text-[14px] font-semibold text-[#334155]">
              Status: {formatTicketStatus(props.ticketDetail.status)}
            </p>
            <p className="m-0 mb-4 inline-block rounded-full border-none bg-[#eff6ff] px-2.5 py-1 text-[14px] font-semibold text-[#1e40af]">
              Priorytet {props.ticketDetail.priority}/5
            </p>
            <p>{props.ticketDetail.description}</p>
            {props.ticketDetail.status !== "Closed" && (
              <button
                type="button"
                className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white"
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
