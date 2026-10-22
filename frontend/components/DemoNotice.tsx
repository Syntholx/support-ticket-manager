export function DemoNotice() {
  return (
    <aside aria-label="Informacje o publicznym demo" className="mb-5 rounded-2xl border border-solid border-[#fcd34d] bg-[#fffbeb] p-5 text-[#78350f]">
      <h2 className="m-0 text-[18px] font-semibold">Publiczne demo TSM</h2>
      <p className="mb-0">Nie wpisuj danych osobowych ani poufnych informacji. Konta i zgłoszenia są wspólne dla odwiedzających. Dane są okresowo przywracane.</p>
      <p className="mb-0">Rejestracja, zmiana haseł i wysyłka e-maili są wyłączone.</p>
      <dl className="mb-0 break-words">
        <dt className="font-semibold">Użytkownik</dt>
        <dd className="ml-0">user@demo.example — <code>Demo-User!2026</code></dd>
        <dt className="mt-2 font-semibold">Support</dt>
        <dd className="ml-0">support@demo.example — <code>Demo-Support!2026</code></dd>
      </dl>
    </aside>
  );
}
