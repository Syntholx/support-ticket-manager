import { FormEvent, useState } from "react";
import { formatTicketStatus, TicketSummary } from "../ticket";
import { getCsrfToken } from "../get-csrf-token";

export function AuthenticatedHome(prop: {
  onLogoutClick: () => void;
  message: string;
  isLoggingOut: boolean;
  tickets: TicketSummary[];
  isLoadingTickets: boolean;
  ticketsError: string;
  onTicketCreated: () => void;
  onRefreshTickets: () => void;
  onTicketClick: (id: number) => void;
  onArchiveClick: () => void;
}) {
  const [createMessage, setCreateMessage] = useState("");
  const [isCreatingTicket, setCreatingTicket] = useState(false);
  async function handleCreateTicketSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const title = formData.get("title");
    const description = formData.get("description");
    if (typeof title !== "string" || typeof description !== "string") {
      return;
    }
    if (!title.trim() || !description.trim()) {
      setCreateMessage(
        "Tytuł i opis nie mogą składać się wyłącznie ze spacji.",
      );
      return;
    }
    const createTicketRequest = { title: title, description: description };
    setCreateMessage("Przygotowuję zgłoszenie..");
    setCreatingTicket(true);
    try {
      const formElement = event.currentTarget;
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setCreateMessage(
          "Nie udało się przygotować bezpiecznego utworzenia nowego zgłoszenia",
        );
        return;
      }
      const createResponse = await fetch("/api/tickets", {
        method: "POST",
        credentials: "same-origin",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": csrfToken,
        },
        body: JSON.stringify(createTicketRequest),
      });
      if (createResponse.status === 401) {
        setCreateMessage(
          "Sesja wygasła, odswież stronę i zaloguj się ponownie",
        );
        return;
      }
      if (createResponse.status === 400) {
        setCreateMessage(
          "Zgłoszenie zostało odrzucone. Sprawdź tytuł i opis, a jeśli problem się powtarza, odswież stronę.",
        );
        return;
      }
      if (createResponse.status !== 201) {
        setCreateMessage("Nie udało się utworzyć zgłoszenia");
        return;
      }
      prop.onTicketCreated();
      formElement.reset();
      setCreateMessage("Zgłoszenie utworzone.");
    } catch (error) {
      setCreateMessage("Błąd API lub połączenia, sprawdź i spróbuj ponownie. ");
    } finally {
      setCreatingTicket(false);
    }
  }
  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <h1>Panel użytkownika</h1>
      <p>Witaj! Jesteś zalogowany.</p>
      <button
        type="button"
        className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        onClick={prop.onArchiveClick}
      >
        Archiwum zgłoszeń
      </button>
      <h2>Aktywne zgłoszenia</h2>
      <button
        type="button"
        className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        onClick={prop.onRefreshTickets}
        disabled={prop.isLoadingTickets}
      >
        Odswież listę
      </button>
      {prop.ticketsError !== "" && <p>{prop.ticketsError}</p>}
      {prop.isLoadingTickets && <p>Ładowanie zgłoszeń...</p>}
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        (prop.tickets.length === 0 ? (
          <p>Nie masz jeszcze aktywnych zgłoszeń</p>
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
      <h2>Nowe zgłoszenie</h2>
      <form onSubmit={handleCreateTicketSubmit}>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="title">Podaj tytuł zgłoszenia</label>
          <input className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5" id="title" name="title" required maxLength={200}></input>
        </div>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="description">Podaj opis problemu</label>

          <textarea className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            id="description"
            name="description"
            required
            maxLength={5000}
            rows={5}
          ></textarea>
        </div>
        <button
          type="submit"
          className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white"
          disabled={isCreatingTicket}
        >
          Utwórz zgłoszenie
        </button>
        {createMessage !== "" && <p role="status">{createMessage}</p>}
      </form>

      <p>{prop.message}</p>
      <button
        type="button"
        className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#cbd5e1] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#334155] hover:bg-[#f1f5f9]"
        onClick={prop.onLogoutClick}
        disabled={prop.isLoggingOut}
      >
        Wyloguj się
      </button>
    </section>
  );
}
