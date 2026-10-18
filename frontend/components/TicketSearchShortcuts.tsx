export function TicketSearchShortcuts(props: {
  query: string;
  onQueryChange: (value: string) => void;
}) {
  const shortcuts = [
    { label: "Szukaj testów", query: "test" },
    { label: "Szukaj błędów", query: "błąd" },
    { label: "Szukaj logowania", query: "logowanie" },
    { label: "Szukaj płatności", query: "płatność" },
  ];
  return (
    <>
      {shortcuts.map((shortcut) => (
        <button
          key={shortcut.query}
          type="button"
          className="[font-family:inherit] self-start leading-[inherit] min-h-[44px] cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-3 py-1.5 text-[14px] font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:border-[#cbd5e1] disabled:bg-[#f1f5f9] disabled:text-[#64748b]"
          disabled={props.query === shortcut.query}
          onClick={() => props.onQueryChange(shortcut.query)}
        >
          {shortcut.label}
        </button>
      ))}
    </>
  );
}
