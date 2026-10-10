export function CheckingSession(props: {
  message: string;
  onRetryClick: () => void;
  hasSessionError: boolean;
}) {
  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <h1>Stan sesji</h1>
      <p>{props.message}</p>
      {props.hasSessionError && (
        <button type="button" onClick={props.onRetryClick}>
          Spróbuj ponownie
        </button>
      )}
    </section>
  );
}
