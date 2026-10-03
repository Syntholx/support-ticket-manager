export function CheckingSession(props: {
  message: string;
  onRetryClick: () => void;
  hasSessionError: boolean;
}) {
  return (
    <section className="welcome-card">
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
