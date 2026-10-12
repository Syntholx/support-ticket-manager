export function TicketSearch(props: {
  query: string;
  onQueryChange: (value: string) => void;
}) {
  return (
    <div className="mb-4 flex w-full flex-col gap-1.5">
      <label htmlFor="search-Query">Szukaj po tytule</label>
      <input
        type="search"
        id="search-Query"
        name="search-Query"
        value={props.query}
        onChange={(event) => props.onQueryChange(event.currentTarget.value)}
        onKeyDown={(event) => {
          if (event.key === "Escape") {
            props.onQueryChange("");
          }
        }}
        className="[font-family:inherit] leading-[inherit] text-[length:inherit] [font-weight:inherit] min-h-[44px] rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-2.5"
      ></input>
      <button
        type="button"
        className="[font-family:inherit] self-start mt-0.5 leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
        disabled={props.query === ""}
        onClick={() => props.onQueryChange("")}
      >
        Wyczyść
      </button>
    </div>
  );
}
