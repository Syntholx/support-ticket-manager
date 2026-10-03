export function GuestHome(props: {
  onLoginClick: () => void;
  onRegisterClick: () => void;
}) {
  return (
    <section className="welcome-card">
      <h1>Support Ticket Manager</h1>
      <p>Zaloguj się, aby przeglądać, tworzyć i sledzić zgłoszenia.</p>
      <button type="button" onClick={props.onLoginClick}>
        Zaloguj się
      </button>
      <button type="button" onClick={props.onRegisterClick}>
        Utwórz konto
      </button>
    </section>
  );
}
