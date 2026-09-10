# MVP 7 — v0.7.0 (2026-09-10)

Tworzenie i pełny cykl obsługi zgłoszeń przez API ASP.NET Core.

- POST tworzenia: DTO, walidacja, 201, Location i dane zgłoszenia.
- POST start/close/reopen: reguły Ticket, 200/404/409 i odpowiedzi JSON.
- POST zmiany priorytetu: zakres 1–5, 400 bez zmiany danych, 404 dla braku ID.
  Zmiana działa także przy Closed; status pozostaje bez zmian.
- 60 testów: 29 jednostkowych i 31 integracyjnych; build Release bez ostrzeżeń.
- Testy granic, odrzucania danych, null/braku opisu, uszkodzonego JSON,
  niepoprawnego typu oraz późniejszego odczytu zmienionego obiektu.
- Autor potwierdził ręcznie pełny cykl i przechodzenie między kolejką a archiwum.
- [Demo interfejsu na portfolio](https://szymon-michalek.dev/tsm-demo/):
  przykładowe dane, bez połączenia z API, symulowane role i operacje.
  UI przygotowane przez asystenta na prośbę autora, oddzielnie od nauki backendu.

Ograniczenia: API nadal lokalne, bez bazy, kont i ochrony równoczesnych zapisów.
Nie jest to produkcyjny system wieloużytkownikowy. Demo nie wysyła zgłoszeń
do backendu; po odświeżeniu przywraca dane. Kolejny kierunek nauki: SQL.

## Historia — MVP 6, v0.6.0 (2026-09-07)

Pierwsze odczytowe API ASP.NET Core, działające obok aplikacji konsolowej.

- Wspólna biblioteka Core: model, reguły, serwis i zapytania bez kopiowania kodu.
- GET /api/tickets: aktywne zgłoszenia malejąco według priorytetu.
- GET /api/tickets/archived: zamknięte zgłoszenia.
- GET /api/tickets/{id:int}: dane zgłoszenia lub 404 z komunikatem i ID.
- JSON ze statusami tekstowymi; enum pozostaje typem wewnątrz aplikacji.
- Lokalne HTTPS i instrukcja certyfikatu deweloperskiego w README.
- 29 testów xUnit (9 serwisu, 13 Ticket, 7 TicketQueries), build bez ostrzeżeń.

Autor sprawdził ręcznie HTTPS: aktywne ID 3, 1; archiwum ID 2; szczegóły 200;
brak ID 99 -> 404; abc -> brak dopasowania trasy; pusta kolejka -> 200 i [].
Testy jednostkowe sprawdzają logikę, nie podłączenie HTTP.

Ograniczenia: tylko odczyt przez API, dane w pamięci, osobne listy konsoli i API,
brak bazy danych, logowania i frontendu. HTTPS jest lokalne; API nie jest wdrożone
publicznie. Portfolio celowo pozostaje bez aktualizacji.

## Historia — MVP 5, v0.5.0 (2026-09-06)

Tworzenie zgłoszeń jest teraz realizowane przez `TicketService`, niezależnie od
odczytu danych z konsoli. Statusy korzystają z typu `TicketStatus`, a najważniejsze
operacje otrzymały powtarzalne testy jednostkowe.

- `TicketStatus`: Open, InProgress, Closed; zachowane reguły przejść i walidacja.
- `TicketService`: współdzielona lista, kolejne ID, tworzenie i zwracanie obiektu.
- Opcja 10 używa serwisu, bez podwójnego dodawania zgłoszeń.
- 22 testy xUnit: 9 serwisu oraz 13 zmian statusu i priorytetu.
- README zawiera polecenia budowania i `dotnet test`.

Weryfikacja: 22/22 testy zaliczone; użytkownik potwierdził ręcznie tworzenie,
aktywne zgłoszenia, zamknięcie, archiwum, ponowne otwarcie i wyjście z menu.

Wydanie nadal jest aplikacją konsolową. Dane znikają po zakończeniu procesu.
API, baza danych i frontend pozostają kolejnymi etapami.
