export function DemoNotice() {
  return (
    <aside aria-label="About the public demo" className="mb-5 rounded-2xl border border-solid border-[#fcd34d] bg-[#fffbeb] p-5 text-[#78350f]">
      <h2 className="m-0 text-[18px] font-semibold">TSM public demo</h2>
      <p className="mb-0">Please do not enter personal or confidential information. Visitors share the demo accounts and tickets. Demo data is reset periodically.</p>
      <p className="mb-0">Registration, password changes and email delivery are disabled.</p>
      <dl className="mb-0 break-words">
        <dt className="font-semibold">User</dt>
        <dd className="ml-0">user@demo.example — <code>Demo-User!2026</code></dd>
        <dt className="mt-2 font-semibold">Support</dt>
        <dd className="ml-0">support@demo.example — <code>Demo-Support!2026</code></dd>
      </dl>
    </aside>
  );
}
