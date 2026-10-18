import { FormEvent, useState } from "react";
import { TicketSummary } from "../ticket";
import { getCsrfToken } from "../get-csrf-token";
import { TicketList } from "./TicketList";
import { TicketSearch } from "../components/TicketSearch";
import { TicketSort } from "../components/TicketSort";
import { TicketSearchShortcuts } from "../components/TicketSearchShortcuts";

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
  const [searchQuery, setSearchQuery] = useState("");
  const filteredTickets = prop.tickets.filter((ticket) =>
    ticket.title.toLowerCase().includes(searchQuery.trim().toLowerCase()),
  );
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
  const [sortOrder, setSortOrder] = useState("asc");
  const sortedTickets = [...filteredTickets].sort((a, b) =>
    sortOrder === "asc" ? a.id - b.id : b.id - a.id,
  );
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
      <h2>
        Aktywne zgłoszenia
        {!prop.isLoadingTickets && prop.ticketsError === "" && (
          <>: {prop.tickets.length} </>
        )}
      </h2>
      <TicketSearch query={searchQuery} onQueryChange={setSearchQuery} />
      <div className="mb-[16px] flex flex-row flex-wrap gap-2">
        <TicketSearchShortcuts
          query={searchQuery}
          onQueryChange={setSearchQuery}
        />
      </div>
      <div className="mb-4 flex w-full flex-col gap-1.5">
        <TicketSort order={sortOrder} onOrderChange={setSortOrder} />
        {!prop.isLoadingTickets &&
          prop.ticketsError === "" &&
          prop.tickets.length > 0 && (
            <p className="text-[14px] text-[#64748b] m-0">
              Kolejność dotyczy tylko pobranych zgłoszeń.
            </p>
          )}
      </div>
      <button
        type="button"
        disabled={searchQuery === "" && sortOrder === "asc"}
        className="[font-family:inherit] self-start mb-[16px] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        onClick={() => {
          setSearchQuery("");
          setSortOrder("asc");
        }}
      >
        Resetuj widok
      </button>
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        searchQuery.trim() !== "" && (
          <p className="text-[#64748b] mt-0 mb-[16px] text-[14px]">
            {`Wyniki: ${filteredTickets.length} z ${prop.tickets.length} pobranych`}
          </p>
        )}
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        prop.tickets.length > 0 &&
        filteredTickets.length === 0 && (
          <p className="text-[14px] text-[#64748b] mt-0 mb-[16px]">
            Niektóre zgłoszenia mogą znajdować się na innych stronach danych.
          </p>
        )}
      <button
        type="button"
        className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        onClick={prop.onRefreshTickets}
        disabled={prop.isLoadingTickets}
      >
        {prop.isLoadingTickets ? "Odświeżanie..." : "Odśwież listę"}
      </button>
      {prop.ticketsError !== "" && <p>{prop.ticketsError}</p>}
      {prop.isLoadingTickets && <p>Ładowanie zgłoszeń...</p>}
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        (prop.tickets.length === 0 ? (
          <p>Nie masz jeszcze aktywnych zgłoszeń</p>
        ) : filteredTickets.length === 0 ? (
          <p>Nie znaleziono zgłoszeń pasujących do wyszukiwania</p>
        ) : (
          <TicketList
            tickets={sortedTickets}
            onTicketClick={prop.onTicketClick}
          />
        ))}
      <h2>Nowe zgłoszenie</h2>
      <form onSubmit={handleCreateTicketSubmit}>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="title">Podaj tytuł zgłoszenia</label>
          <input
            className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            id="title"
            name="title"
            required
            maxLength={200}
          ></input>
        </div>
        <div className="mb-4 flex w-full flex-col gap-1.5">
          <label htmlFor="description">Podaj opis problemu</label>

          <textarea
            className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
            id="description"
            name="description"
            required
            maxLength={5000}
            rows={5}
          ></textarea>
        </div>
        <button
          type="submit"
          className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
          disabled={isCreatingTicket}
        >
          {isCreatingTicket ? "Tworzenie..." : "Utwórz zgłoszenie"}
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
