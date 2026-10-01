import { createRoot } from "react-dom/client";
const heading = document.getElementById("status-heading-root");
if (!(heading instanceof HTMLElement)) {
  throw new Error("Nie znaleziono elementu");
}
createRoot(heading).render(<StatusHeading title="Status TSM" />);
function StatusHeading(props: { title: string }) {
  return <h1>{props.title}</h1>;
}
