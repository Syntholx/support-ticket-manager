# MVP 5 — v0.5.0 (2026-09-06)

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
