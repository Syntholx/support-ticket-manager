import { FormEvent, useState } from "react";
import { TicketSummary } from "../ticket";
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
    <section className="welcome-card">
      <h1>Panel użytkownika</h1>
      <p>Witaj! Jesteś zalogowany.</p>
      <h2>Aktywne zgłoszenia</h2>
      <button
        type="button"
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
          <ul>
            {prop.tickets.map((ticket) => (
              <li key={ticket.id}>
                #{ticket.id} - {ticket.title}
              </li>
            ))}
          </ul>
        ))}
      <h2>Nowe zgłoszenie</h2>
      <form onSubmit={handleCreateTicketSubmit}>
        <div className="form-field">
          <label htmlFor="title">Podaj tytuł zgłoszenia</label>
          <input id="title" name="title" required maxLength={200}></input>
        </div>
        <div className="form-field">
          <label htmlFor="description">Podaj opis problemu</label>

          <textarea
            id="description"
            name="description"
            required
            maxLength={5000}
            rows={5}
          ></textarea>
        </div>
        <button
          type="submit"
          className="login-button"
          disabled={isCreatingTicket}
        >
          Utwórz zgłoszenie
        </button>
        {createMessage !== "" && <p role="status">{createMessage}</p>}
      </form>

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
