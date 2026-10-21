import { TicketDetail } from "../ticket";
import { useState, useEffect } from "react";
import { TicketStatusBadge } from "./TicketStatusBadge";
import { TicketPriorityBadge } from "../components/TicketPriorityBadge";

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
  isSupport: boolean;
  isStartingTicket: boolean;
  startTicketMessage: string;
  onStartTicket: () => void;
  isReopenTicket: boolean;
  reopenTicketMessage: string;
  onReopenTicket: () => void;
  isChangingPriority: boolean;
  priorityMessage: string;
  onChangePriority: (priority: number) => void;
}) {
  const showFallbackHeading =
    props.isLoadingTicketDetail ||
    props.ticketDetailError !== "" ||
    props.ticketDetail === null;
  const [selectedPriority, setSelectedPriority] = useState("2");
  useEffect(() => {
    if (props.ticketDetail !== null) {
      setSelectedPriority(String(props.ticketDetail.priority));
    }
  }, [props.ticketDetail]);
  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <button
        type="button"
        className="cursor-pointer border-none bg-transparent font-semibold text-[#1d4ed8] disabled:text-[#64748b] disabled:cursor-not-allowed"
        onClick={props.onBackClick}
        disabled={
          props.isClosingTicket ||
          props.isStartingTicket ||
          props.isReopenTicket ||
          props.isChangingPriority
        }
      >
        Wróć
      </button>
      <p className="text-[12px] text-[#64748b] mt-[16px] mb-[8px]">
        Zgłoszenie #{props.selectedTicketId}
      </p>
      {showFallbackHeading && <h1>Szczegóły zgłoszenia</h1>}
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
            <h1 className="text-[28px] font-semibold mt-0 mb-[16px] break-words">
              {props.ticketDetail.title}
            </h1>
            <div className="flex gap-[8px] mb-[16px] flex-wrap">
              <TicketStatusBadge status={props.ticketDetail.status} />
              <TicketPriorityBadge
                status={props.ticketDetail.status}
                priority={props.ticketDetail.priority}
              />
            </div>
            <section className="mb-[24px] break-words">
              <h2 className="text-[18px] mt-0 mb-[12px] font-semibold">
                Opis problemu
              </h2>
              <p className="whitespace-pre-wrap break-words">
                {props.ticketDetail.description}
              </p>
            </section>
            {(props.isSupport || props.ticketDetail.status !== "Closed") && (
              <section className="border-0 border-t border-solid border-[#dce5ef] pt-5">
                <h2 className="text-[18px] mt-0 mb-[12px] font-semibold">
                  Operacje zgłoszenia
                </h2>
                {props.isSupport && (
                  <div className="w-full flex flex-col gap-1.5 mb-[16px]">
                    <label htmlFor="change-priority">Nowy priorytet</label>
                    <select
                      id="change-priority"
                      disabled={
                        props.isClosingTicket ||
                        props.isReopenTicket ||
                        props.isStartingTicket ||
                        props.isChangingPriority
                      }
                      value={selectedPriority}
                      onChange={(event) =>
                        setSelectedPriority(event.currentTarget.value)
                      }
                      className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
                    >
                      <option value="1">Priorytet: 1</option>
                      <option value="2">Priorytet: 2</option>
                      <option value="3">Priorytet: 3</option>
                      <option value="4">Priorytet: 4</option>
                      <option value="5">Priorytet: 5</option>
                    </select>
                    <button
                      type="button"
                      className="self-start min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] mr-2 px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
                      disabled={
                        Number(selectedPriority) ===
                          props.ticketDetail.priority ||
                        props.isClosingTicket ||
                        props.isReopenTicket ||
                        props.isStartingTicket ||
                        props.isChangingPriority
                      }
                      onClick={() =>
                        props.onChangePriority(Number(selectedPriority))
                      }
                    >
                      {props.isChangingPriority
                        ? "Zapisywanie..."
                        : "Zapisz priorytet"}
                    </button>
                    {props.priorityMessage !== "" && (
                      <p role="status" className="m-0">
                        {props.priorityMessage}
                      </p>
                    )}
                  </div>
                )}
                <div className="flex flex-wrap gap-2">
                  {props.isSupport &&
                    props.ticketDetail.status === "Closed" && (
                      <button
                        type="button"
                        onClick={props.onReopenTicket}
                        disabled={
                          props.isClosingTicket ||
                          props.isStartingTicket ||
                          props.isReopenTicket ||
                          props.isChangingPriority
                        }
                        className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8]  px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
                      >
                        {props.isReopenTicket
                          ? "Otwieranie..."
                          : "Otwórz ponownie zgłoszenie"}
                      </button>
                    )}

                  {props.isSupport && props.ticketDetail.status === "Open" && (
                    <button
                      type="button"
                      disabled={
                        props.isClosingTicket ||
                        props.isStartingTicket ||
                        props.isReopenTicket ||
                        props.isChangingPriority
                      }
                      onClick={props.onStartTicket}
                      className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8]  px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
                    >
                      {props.isStartingTicket
                        ? "Rozpoczynanie..."
                        : "Rozpocznij obsługę"}
                    </button>
                  )}

                  {props.ticketDetail.status !== "Closed" && (
                    <button
                      type="button"
                      className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white disabled:bg-[#f1f5f9] disabled:text-[#64748b] disabled:cursor-not-allowed"
                      onClick={props.onCloseTicket}
                      disabled={
                        props.isClosingTicket ||
                        props.isStartingTicket ||
                        props.isReopenTicket ||
                        props.isChangingPriority
                      }
                    >
                      {props.isClosingTicket
                        ? "Zamykanie..."
                        : "Zamknij zgłoszenie"}
                    </button>
                  )}
                </div>

                {props.startTicketMessage !== "" && (
                  <p role="status">{props.startTicketMessage}</p>
                )}
                {props.reopenTicketMessage !== "" && (
                  <p role="status">{props.reopenTicketMessage}</p>
                )}
              </section>
            )}
            <p role="status">{props.closeTicketMessage}</p>
          </>
        )}
    </section>
  );
}
