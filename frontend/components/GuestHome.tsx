export function GuestHome(props: {
  onLoginClick: () => void;
  onRegisterClick: () => void;
}) {
  return (
    <section className="welcome-card">
      <h1>Support Ticket Manager</h1>
      <p>Zaloguj się, aby przeglądać, tworzyć i sledzić zgłoszenia.</p>

      <div className="guest-action">
        <button
          type="button"
          className="guest-login-button"
          onClick={props.onLoginClick}
        >
          Zaloguj się
        </button>
        <button
          type="button"
          className="guest-register-button"
          onClick={props.onRegisterClick}
        >
          Utwórz konto
        </button>
      </div>
    </section>
  );
}
