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
  page: number;
  pageSize: number;
  maxPage: number;
  onPreviousPage: () => void;
  onNextPage: () => void;
  isSupport: boolean;
}) {
  const [createMessage, setCreateMessage] = useState("");
  const [isCreatingTicket, setCreatingTicket] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [createdTicketId, setCreatedTicketId] = useState<number | null>(null);
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
        "Title and description cannot contain only spaces.",
      );
      return;
    }
    const createTicketRequest = { title: title, description: description };
    setCreateMessage("Preparing ticket…");
    setCreatingTicket(true);
    setCreatedTicketId(null);
    try {
      const formElement = event.currentTarget;
      const csrfToken = await getCsrfToken();
      if (csrfToken === null) {
        setCreateMessage(
          "Could not prepare a secure request to create the ticket.",
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
          "Session expired. Refresh the page and sign in again.",
        );
        return;
      }
      if (createResponse.status === 400) {
        setCreateMessage(
          "Ticket rejected. Check the title and description. If the problem persists, refresh the page.",
        );
        return;
      }
      if (createResponse.status !== 201) {
        setCreateMessage("Could not create the ticket");
        return;
      }
      prop.onTicketCreated();
      formElement.reset();
      let createdData: unknown;

      try {
        createdData = await createResponse.json();
      } catch {
        setCreateMessage(
          "Ticket created, but the response could not be read. Refresh the list.",
        );
        return;
      }

      if (
        typeof createdData !== "object" ||
        createdData === null ||
        !("id" in createdData) ||
        typeof createdData.id !== "number" ||
        !Number.isSafeInteger(createdData.id) ||
        createdData.id <= 0
      ) {
        setCreateMessage(
          "Ticket created, but its ID could not be read. Refresh the list.",
        );
        return;
      }

      setCreatedTicketId(createdData.id);
      setCreateMessage("Ticket created.");
    } catch (error) {
      setCreateMessage("Connection error. Check your connection and try again. ");
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
      <h1>Dashboard</h1>
      <p className="text-[14px] text-[#64748b] mt-[0px] mb-[16px]">
        {prop.isSupport ? "Role: Support" : "Role: User"}
      </p>
      <p>Welcome! You are signed in.</p>
      <button
        type="button"
        className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        onClick={prop.onArchiveClick}
      >
        Ticket archive
      </button>
      <section className="border border-[#dce5ef] rounded-[12px] p-4 border-solid mt-4">
        <h2 className="mt-0">
          Active tickets
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
          className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
          onClick={prop.onRefreshTickets}
          disabled={prop.isLoadingTickets}
        >
          {prop.isLoadingTickets ? "Refreshing…" : "Refresh list"}
        </button>
        {prop.ticketsError !== "" && <p>{prop.ticketsError}</p>}
        {prop.isLoadingTickets && <p>Loading tickets…</p>}
        {!prop.isLoadingTickets &&
          prop.ticketsError === "" &&
          (prop.tickets.length === 0 ? (
            <p>
              {prop.page === 1
                ? "You have no active tickets yet"
                : "No tickets on this page. Go back to the previous page."}
            </p>
          ) : filteredTickets.length === 0 ? (
            <p>No tickets match your search</p>
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
      <section className="border border-[#dce5ef] rounded-[12px] p-4 bg-[#f8fafc] mt-[24px] border-solid bg-white">
        <h2 className="mt-0">New ticket</h2>
        <form onSubmit={handleCreateTicketSubmit}>
          <div className="mb-4 flex w-full flex-col gap-1.5">
            <label htmlFor="title">Enter a ticket title</label>
            <input
              className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
              id="title"
              name="title"
              required
              maxLength={200}
            ></input>
          </div>
          <div className="mb-4 flex w-full flex-col gap-1.5">
            <label htmlFor="description">Describe the issue</label>

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
            {isCreatingTicket ? "Creating…" : "Create ticket"}
          </button>
          {createMessage !== "" && <p role="status">{createMessage}</p>}
          {createdTicketId !== null && (
            <button
              type="button"
              className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b] mt-[8px]"
              onClick={() => {
                if (createdTicketId !== null) {
                  prop.onTicketClick(createdTicketId);
                }
              }}
            >
              Open ticket #{createdTicketId}
            </button>
          )}
        </form>
      </section>
      <section>
        <p>{prop.message}</p>
        <button
          type="button"
          className="[font-family:inherit] leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#cbd5e1] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#334155] hover:bg-[#f1f5f9]"
          onClick={prop.onLogoutClick}
          disabled={prop.isLoggingOut}
        >
          Sign out
        </button>
      </section>
    </section>
  );
}
