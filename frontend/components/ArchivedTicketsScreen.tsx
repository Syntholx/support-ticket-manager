import { TicketSummary } from "../ticket";
import { TicketList } from "../components/TicketList";
import { useState } from "react";
import { TicketSearch } from "./TicketSearch";
import { TicketSort } from "./TicketSort";
import { TicketSearchShortcuts } from "./TicketSearchShortcuts";
export function ArchivedTicketsScreen(prop: {
  tickets: TicketSummary[];
  isLoadingTickets: boolean;
  ticketsError: string;
  onRefreshClick: () => void;
  onTicketClick: (id: number) => void;
  onBackClick: () => void;
  page: number;
  onPreviousPage: () => void;
  pageSize: number;
  onNextPage: () => void;
  maxPage: number;
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
        Back to active tickets
      </button>
      <h1>Ticket archive</h1>
      <p className="text-[14px] text-[#64748b]">
        Priority reflects the closed ticket’s original priority.
      </p>
      <p>
        Closed tickets
        {!prop.isLoadingTickets && prop.ticketsError === "" && (
          <>: {prop.tickets.length}</>
        )}
      </p>
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
              Sorting applies to the tickets loaded on this page.
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
        Reset filters
      </button>
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        searchQuery.trim() !== "" && (
          <p className="text-[#64748b] mt-0 mb-[16px] text-[14px]">
            {`Results: ${filteredTickets.length} of ${prop.tickets.length} loaded`}
          </p>
        )}
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        prop.tickets.length > 0 &&
        filteredTickets.length === 0 && (
          <p className="text-[14px] text-[#64748b] mt-0 mb-[16px]">
            Other pages may contain more tickets.
          </p>
        )}

      <button
        type="button"
        onClick={prop.onRefreshClick}
        className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        disabled={prop.isLoadingTickets}
      >
        {prop.isLoadingTickets ? "Refreshing…" : "Refresh list"}
      </button>
      {prop.ticketsError !== "" && <p>{prop.ticketsError}</p>}
      {!prop.isLoadingTickets && prop.ticketsError !== "" && (
        <p className="text-[#64748b] text-[14px] mt-[8px] mb-[16px]">
          Could not refresh. Please try again.
        </p>
      )}

      {prop.isLoadingTickets && <p>Loading archive…</p>}
      {!prop.isLoadingTickets &&
        prop.ticketsError === "" &&
        (prop.tickets.length === 0 ? (
          <p>
            {prop.page === 1
              ? "You have no closed tickets yet"
              : "No tickets on this page. Go back to the previous page."}
          </p>
        ) : filteredTickets.length === 0 ? (
          <p>No tickets match your search.</p>
        ) : (
          <TicketList
            tickets={sortedTickets}
            onTicketClick={prop.onTicketClick}
          />
        ))}
      <nav className="flex items-center gap-[8px] flex-wrap mt-[16px]">
        <button
          type="button"
          className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
          onClick={prop.onPreviousPage}
          disabled={prop.isLoadingTickets || prop.page <= 1}
        >
          Previous
        </button>
        <p className="m-0">Page {prop.page}</p>
        <button
          type="button"
          className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
          onClick={prop.onNextPage}
          disabled={
            prop.isLoadingTickets ||
            prop.ticketsError !== "" ||
            prop.tickets.length < prop.pageSize ||
            prop.page >= prop.maxPage
          }
        >
          Next
        </button>
      </nav>
    </section>
  );
}
