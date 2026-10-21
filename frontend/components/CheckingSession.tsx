export function CheckingSession(props: {
  message: string;
  onRetryClick: () => void;
  hasSessionError: boolean;
}) {
  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <h1>Stan sesji</h1>
      <p role="status">{props.message}</p>
      {props.hasSessionError && (
        <button
          type="button"
          className="min-h-[44px] rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white"
          onClick={props.onRetryClick}
        >
          Spróbuj ponownie
        </button>
      )}
    </section>
  );
}
