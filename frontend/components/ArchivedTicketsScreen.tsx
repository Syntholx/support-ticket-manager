import { TicketSummary } from "../ticket";
import { TicketList } from "../components/TicketList";
import { useState } from "react";
import { TicketSearch } from "./TicketSearch";
export function ArchivedTicketsScreen(prop: {
  tickets: TicketSummary[];
  isLoadingTickets: boolean;
  ticketsError: string;
  onRefreshClick: () => void;
  onTicketClick: (id: number) => void;
  onBackClick: () => void;
}) {
  const [searchQuery, setSearchQuery] = useState("");
  const filteredTickets = prop.tickets.filter((ticket) =>
    ticket.title.toLowerCase().includes(searchQuery.trim().toLowerCase()),
  );
  const [sortOrder, setSortOrder] = useState("asc");
  const sortedTickets = [...filteredTickets].sort((a, b) =>
    sortOrder === "asc" ? a.id - b.id : b.id - a.id,
  );

  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <button
        type="button"
        onClick={prop.onBackClick}
        className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8]"
      >
        Wróć do aktywnych
      </button>
      <h1>Archiwum zgłoszeń</h1>
      <p>
        Zamknięte zgłoszenia
        {!prop.isLoadingTickets && prop.ticketsError === "" && (
          <>: {prop.tickets.length}</>
        )}
      </p>
      <TicketSearch query={searchQuery} onQueryChange={setSearchQuery} />
      <div className="mb-4 flex w-full flex-col gap-1.5">
        <label htmlFor="ticket-sort">Kolejność</label>
        <select
          className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
          id="ticket-sort"
          value={sortOrder}
          onChange={(event) => setSortOrder(event.currentTarget.value)}
        >
          <option value="asc">ID rosnąco</option>
          <option value="desc">ID malejąco</option>
        </select>
        {!prop.isLoadingTickets &&
          prop.ticketsError === "" &&
          prop.tickets.length > 0 && (
            <p className="text-[14px] text-[#64748b] m-0">
              Kolejność dotyczy tylko pobranych zgłoszeń.
            </p>
          )}
      </div>
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        searchQuery.trim() !== "" && (
          <p className="text-[#64748b] mt-0 mb-[16px] text-[14px]">
            {`Wyniki:   ${filteredTickets.length}   z   ${prop.tickets.length} pobranych`}
          </p>
        )}

      <button
        type="button"
        onClick={prop.onRefreshClick}
        className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        disabled={prop.isLoadingTickets}
      >
        {prop.isLoadingTickets ? "Odświeżanie..." : "Odśwież listę"}
      </button>
      {prop.ticketsError !== "" && <p>{prop.ticketsError}</p>}
      {!prop.isLoadingTickets && prop.ticketsError !== "" && (
        <p className="text-[#64748b] text-[14px] mt-[8px] mb-[16px]">
          Odświeżanie zakończone błędem. Spróbuj ponownie.
        </p>
      )}

      {prop.isLoadingTickets && <p>Ładowanie archiwum...</p>}
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        (prop.tickets.length === 0 ? (
          <p>Nie masz jeszcze zamkniętych zgłoszeń.</p>
        ) : filteredTickets.length === 0 ? (
          <p>Nie znaleziono zgłoszeń pasujących do wyszukiwania.</p>
        ) : (
          <TicketList
            tickets={sortedTickets}
            onTicketClick={prop.onTicketClick}
          />
        ))}
    </section>
  );
}
