export function GuestHome(props: {
  onLoginClick: () => void;
  onRegisterClick: () => void;
}) {
  return (
    <section className="rounded-2xl border border-solid border-[#dce5ef] bg-white p-7 [&>h1]:mt-0">
      <h1>Support Ticket Manager</h1>
      <p>Zaloguj się, aby przeglądać, tworzyć i sledzić zgłoszenia.</p>

      <div className="flex flex-wrap gap-3">
        <button
          type="button"
          className="[font-family:inherit] leading-[inherit] text-[length:inherit] min-h-[44px] max-w-full cursor-pointer rounded-[10px] border-none bg-[#1d4ed8] px-4 py-2.5 font-semibold text-white hover:bg-[#1e40af]"
          onClick={props.onLoginClick}
        >
          Zaloguj się
        </button>
        <button
          type="button"
          className="[font-family:inherit] leading-[inherit] text-[length:inherit] min-h-[44px] max-w-full cursor-pointer rounded-[10px] border border-solid border-[#b7c7e4] bg-white px-4 py-2.5 font-semibold text-[#1d4ed8] hover:bg-[#eff6ff]"
          onClick={props.onRegisterClick}
        >
          Utwórz konto
        </button>
      </div>
    </section>
  );
}
