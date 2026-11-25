export function TicketSort(props: {
  order: string;
  onOrderChange: (value: string) => void;
}) {
  return (
    <>
      <label htmlFor="ticket-sort">Sort by</label>
      <select
        className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
        id="ticket-sort"
        value={props.order}
        onChange={(event) => props.onOrderChange(event.currentTarget.value)}
      >
        <option value="asc">ID ascending</option>
        <option value="desc">ID descending</option>
      </select>
    </>
  );
}
