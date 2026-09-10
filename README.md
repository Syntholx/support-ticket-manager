# Support Ticket Manager

Aplikacja C#/.NET do obsługi zgłoszeń wsparcia, z konsolą i rozwijanym API. Projekt powstaje
etapami jako pierwszy projekt backendowy w portfolio.

## MVP 7 — v0.7.0 (10.09.2026)

Ukończony etap edukacyjny: tworzenie i obsługa zgłoszeń przez HTTP,
z walidacją oraz 60 testami (29 jednostkowych i 31 integracyjnych API).
Reguły pozostają w `Ticket`, tworzenie w `TicketService`, a zapytania
w `TicketQueries`. Konsola i API współdzielą kod Core, nie pamięć procesu.

| Metoda i ścieżka | Działanie | Odpowiedzi |
|---|---|---|
| GET `/api/tickets` | Aktywna kolejka, malejąco według priorytetu | 200, również dla `[]` |
| GET `/api/tickets/archived` | Zamknięte zgłoszenia | 200 |
| GET `/api/tickets/{id:int}` | Szczegóły zgłoszenia | 200 / 404 |
| POST `/api/tickets` | Utworzenie zgłoszenia | 201 + Location / 400 |
| POST `/api/tickets/{id:int}/start` | Open → InProgress | 200 / 404 / 409 |
| POST `/api/tickets/{id:int}/close` | Open lub InProgress → Closed | 200 / 404 / 409 |
| POST `/api/tickets/{id:int}/reopen` | Closed → Open | 200 / 404 / 409 |
| POST `/api/tickets/{id:int}/priority` | Zmiana priorytetu na 1–5 | 200 / 400 / 404 |

Tworzenie przyjmuje JSON `{"title":"Problem","description":"Opis problemu","priority":3}`.
Serwis nadaje ID i status `Open`. Tytuł/opis nie mogą być null, puste ani składać
się z samych białych znaków. Zmiana priorytetu przyjmuje `{"priority":4}`;
dozwolona jest również dla zamkniętego zgłoszenia i nie zmienia jego statusu.
Start, close i reopen nie potrzebują body. Odpowiedź sukcesu zawiera Ticket.
Brak zasobu daje 404, konflikt stanu 409, niepoprawne dane 400. Błędy zwracane
przez kod endpointów mają pole `message`. Uszkodzony JSON lub nieczytelny typ
może zostać odrzucony wcześniej przez ASP.NET Core bez tego pola.

Weryfikacja: build Release bez błędów i ostrzeżeń, 60/60 testów. Testy obejmują
m.in. Location i pola zgłoszenia przez GET po tworzeniu, odrzucenie null w opisie,
braku opisu, tekstowego priorytetu, uszkodzonego JSON i granic 0/1/5/6.
Autor sprawdził ręcznie pełny przebieg tworzenia, rozpoczęcia, zamknięcia,
przejścia do archiwum i ponownego otwarcia. To nie jest deklaracja pokrycia
wszystkich możliwych przypadków ani test certyfikatu HTTPS przez TestServer.

### Demo interfejsu

[Wypróbuj demo na portfolio (PL)](https://szymon-michalek.dev/tsm-demo/).
Osobna makieta w repozytorium portfolio używa przykładowych danych w JavaScript,
bez połączenia z API. Role, użytkownicy i operacje są symulowane. Odświeżenie
usuwa zmiany. Interfejs przygotował asystent na prośbę autora; nie jest to
zaliczenie frontendu w kursie. Backend pozostaje głównym obszarem nauki.

### Ograniczenia wydania

- API uruchamia się lokalnie; opublikowanie kodu nie oznacza publicznego hostingu API.
- Brak bazy, kont, autoryzacji, przypisywania pracowników i trwałego zapisu.
- Lista w pamięci oraz `Max + 1` nie zabezpieczają równoczesnych zapisów.
  Wersja jest demonstracyjna, niegotowa do produkcyjnej obsługi wielu użytkowników.
- Konsola, API i demo mają osobne dane. Nie wpisuj danych wrażliwych do demo.
- Następny etap nauki: podstawy SQL, następnie potrzebne DI/async i EF Core.

## Uruchomienie API

Uruchomienie lokalnego API z katalogu repozytorium (.NET SDK 10):

```powershell
dotnet dev-certs https --check --trust
dotnet run --project src/SupportTicketManager.Api/SupportTicketManager.Api.csproj --launch-profile https
```

Adres: `https://localhost:7280`.
Jeśli sprawdzenie nie znajdzie zaufanego certyfikatu, wykonaj
`dotnet dev-certs https --trust` i zaakceptuj zaufanie do lokalnego certyfikatu
deweloperskiego. Nie jest to certyfikat do publicznego wdrożenia.
Profil `https` udostępnia także HTTP pod `http://localhost:5231`; nie wymusza
przekierowania HTTP na HTTPS. Profil `http` uruchamia tylko ten drugi adres.

- `GET /api/tickets` — aktywne zgłoszenia, malejąco według priorytetu;
- `GET /api/tickets/archived` — zamknięte zgłoszenia;
- `GET /api/tickets/{id:int}` — szczegóły (200) lub 404 z komunikatem zawierającym ID;
- `/api/status` i `/api/name` — pomocnicze endpointy z lekcji.

Status zgłoszenia w JSON jest tekstem; we wspólnej logice pozostaje enumem.
Przykładowe dane API: ID 1 Open, ID 2 Closed, ID 3 InProgress. Aktywna kolejka
zwraca ID 3 przed ID 1. Dane są wyłącznie w pamięci, bez trwałego zapisu.

Stan weryfikacji: 29 testów jednostkowych oraz ręczne sprawdzenie HTTP i HTTPS
przez autora. Testy jednostkowe nie sprawdzają podłączenia endpointów.
Pusta aktywna kolejka zwraca 200 i `[]`. Brak liczbowego ID zwraca 404 z JSON,
a `/api/tickets/abc` nie pasuje do trasy i zwraca 404 bez komunikatu endpointu.
Tworzenie i zmiany przez API opisano powyżej. Baza oraz integracja z frontendem
pozostają kolejnymi etapami.
API działa lokalnie, bez uwierzytelniania i publicznego hostingu. Publikacja
kodu na GitHubie nie uruchamia serwera dostępnego przez internet.

Podział projektów: `SupportTicketManager.Core` — wspólne reguły i zapytania;
`SupportTicketManager` — interaktywna konsola; `SupportTicketManager.Api` — HTTP;
`SupportTicketManager.Tests` — testy logiki i integracyjne HTTP. API ma 3 własne przykładowe zgłoszenia,
konsola 5. Dodanie zgłoszenia w konsoli nie zmienia listy osobnego procesu API.

## Cel

Aplikacja ma pokazać praktyczne użycie logiki biznesowej: rejestrowanie zgłoszeń,
priorytetyzację, zmianę statusu, wyszukiwanie, filtrowanie i kontrolę czasu
obsługi. Docelowo projekt będzie rozwijany wraz z nauką kolejnych elementów
.NET — od aplikacji konsolowej do testów, bazy danych i API.

## Użytkownik i problem

Pierwszym użytkownikiem aplikacji jest pracownik wsparcia. Potrzebuje szybko
zobaczyć, które zgłoszenia wymagają najpilniejszej reakcji, bez ręcznego
przeglądania całej kolejki.

## MVP 1 — v0.1.0

Pierwsza ukończona wersja potrafi:

- przechowywać przykładowe zgłoszenia;
- wyświetlać wszystkie zgłoszenia;
- filtrować pilne zgłoszenia;
- sprawdzać, czy istnieje zgłoszenie krytyczne;
- liczyć otwarte zgłoszenia;
- sortować zgłoszenia od najwyższego priorytetu;
- prezentować proste podsumowanie kolejki;
- kontrolować rozpoczęcie obsługi, zamknięcie i ponowne otwarcie zgłoszenia;
- bezpiecznie zmieniać priorytet;
- odrzucać niepoprawny priorytet i status podczas tworzenia zgłoszenia.

## Model zgłoszenia

Każde zgłoszenie zawiera:

- `Id` — jednoznaczny identyfikator;
- `Title` — krótki tytuł problemu;
- `Description` — dokładniejszy opis problemu;
- `Priority` — pilność od `1` do `5`;
- `Status` — aktualny etap obsługi.

## Reguły biznesowe MVP

Priorytety:

- `1` — Low;
- `2` — Normal;
- `3` — High;
- `4` — Urgent;
- `5` — Critical.

Statusy: `Open`, `InProgress` i `Closed`.

- zgłoszenie pilne ma `Priority >= 4`;
- zgłoszenie krytyczne ma `Priority == 5`;
- otwarte zgłoszenie ma status inny niż `Closed`;
- zgłoszenie w toku ma status `InProgress`;
- natychmiastowej reakcji wymaga zgłoszenie jednocześnie krytyczne i otwarte;
- kolejka może być sortowana od najwyższego priorytetu.

## MVP 2 — v0.2.0

Druga ukończona wersja dodaje interaktywną obsługę aplikacji przez pracownika
wsparcia. Menu pozwala:

- wyświetlić wszystkie zgłoszenia posortowane od najwyższego priorytetu;
- wyświetlić zgłoszenia pilne;
- wyświetlić podsumowanie kolejki;
- rozpocząć obsługę zgłoszenia wskazanego przez `Id`;
- zamknąć albo ponownie otworzyć wskazane zgłoszenie;
- zmienić priorytet zgłoszenia na wartość od `1` do `5`;
- zakończyć program w kontrolowany sposób.

Program rozróżnia błędny tekst, poprawną liczbę spoza menu, nieistniejące `Id`,
niedozwoloną zmianę stanu oraz priorytet spoza zakresu. Wyszukiwanie jednego
zgłoszenia jest skupione w `FindTicketById`, a reguły zmian pozostają w klasie
`Ticket`.

## Technologie

- C#
- .NET 10
- ASP.NET Core Minimal API i JSON
- LINQ
- xUnit i WebApplicationFactory (testy jednostkowe oraz integracyjne API)
- Git i GitHub

## Uruchomienie konsoli

```powershell
dotnet run --project src/SupportTicketManager/SupportTicketManager.csproj
```

## MVP 3 — v0.3.0

Trzecia ukończona wersja porządkuje odpowiedzialności aplikacji:

- `Ticket` przechowuje dane i reguły pojedynczego zgłoszenia;
- `TicketQueries` wyszukuje, filtruje, sortuje i liczy zgłoszenia;
- `TicketConsoleView` odpowiada za menu i prezentowanie wyników;
- `TicketConsoleApplication` steruje pętlą programu i obsługą operacji;
- `SampleTicketData` tworzy dane demonstracyjne;
- `Program.cs` tworzy potrzebne obiekty i uruchamia aplikację.

Menu pozwala również wyświetlić zamknięte zgłoszenia oraz szczegóły jednego
zgłoszenia wyszukanego po `Id`. Wyszukiwanie obsługuje poprawne `Id`, brak
zgłoszenia oraz tekst, którego nie można zamienić na liczbę.

## MVP 4 — v0.4.0

Czwarta wersja pozwala pracownikowi utworzyć zgłoszenie podczas działania
programu. Użytkownik podaje tytuł, opis i priorytet, a aplikacja:

- odrzuca pusty tytuł, pusty opis i tekst złożony wyłącznie z białych znaków;
- odrzuca niepoprawny numer priorytetu oraz wartość spoza zakresu `1–5`;
- automatycznie nadaje kolejne unikalne `Id`;
- tworzy zgłoszenie ze statusem `Open` i dodaje je do bieżącej kolekcji;
- pokazuje identyfikator utworzonego zgłoszenia.

Aktywna kolejka zawiera wyłącznie zgłoszenia `Open` i `InProgress`. Status
`Closed` jest jednocześnie archiwum: zamknięcie usuwa zgłoszenie z aktywnej
kolejki, a ponowne otwarcie automatycznie je do niej przywraca. Nie jest
potrzebna osobna właściwość `IsArchived`, więc model zachowuje jedno źródło
prawdy o stanie zgłoszenia.

Właściwości `Id`, `Title`, `Description`, `Priority` i `Status` można odczytać,
ale ich settery są prywatne. Dane pozostają przechowywane wyłącznie w pamięci
podczas działania aplikacji.

## MVP 5 — v0.5.0

Piąta wersja wprowadza statusy typu `TicketStatus` (`enum`) i klasę
`TicketService`. Serwis zapamiętuje referencję do listy w polu `private readonly`,
ustala następne ID, tworzy zgłoszenie, dodaje je do listy i zwraca obiekt.
Opcja `10` przekazuje mu dane; odczyt i komunikaty pozostają w konsoli.
Walidacja konstruktora `Ticket` nadal chroni model niezależnie od źródła danych.

W wydaniu MVP 5 projekt testowy zawierał **22 testy xUnit**:

- 9 testów serwisu: ID dla pustej i niepustej listy, tworzenie, kolejne ID,
  odrzucanie błędnego priorytetu, tytułu i opisu oraz zachowanie istniejącej listy;
- 13 testów `Ticket`: zamknięcie, rozpoczęcie i ponowne otwarcie dla wszystkich
  trzech statusów początkowych oraz granice priorytetu `0`, `1`, `5`, `6`.

Testy sprawdzają opisane przypadki, nie gwarantują poprawności całego programu.
Podłączenie menu pozostaje sprawdzane ręcznie. Dane nadal istnieją tylko w pamięci;
w tej wersji nie było API, bazy danych ani interfejsu przeglądarkowego.

## Budowanie i testy

Wymagane: .NET SDK 10. Pierwsze uruchomienie pobiera pakiety z NuGet.
Polecenia wykonaj w głównym folderze repozytorium:

```powershell
dotnet build SupportTicketManager.slnx
dotnet test SupportTicketManager.slnx
```

Przed budowaniem zakończ uruchomioną konsolę opcją `0` lub `Ctrl+C`, aby proces
nie blokował pliku wykonywalnego. Nowym zachowaniom towarzyszą potrzebne testy;
po zmianie uruchamiany jest cały istniejący zestaw.

Projekt jest rozwijany w ramach nauki z pomocą mentora AI przy wyjaśnieniach,
przykładach, przeglądzie kodu i dokumentacji. Nie jest przedstawiany jako praca
wykonana całkowicie bez pomocy.

## Status

**MVP 7 ukończone — `v0.7.0` (10.09.2026).** 60 testów: 29 jednostkowych,
31 integracyjnych API. Kod API dostępny na GitHubie, demo interfejsu osobno
na portfolio. Brak połączenia demo z API i brak produkcyjnego wdrożenia backendu.

Poniższe informacje opisują wcześniejsze wydania.

**MVP 6 ukończone — `v0.6.0` (07.09.2026).** 29 testów jednostkowych:
9 serwisu, 13 modelu Ticket i 7 zapytań. Build bez błędów i ostrzeżeń.
Sprawdzono aktywne zgłoszenia, archiwum, szczegóły, brak ID, błędną trasę oraz
pustą kolejkę przez HTTPS. Publikacja tylko na GitHubie; portfolio pozostaje
przy v0.5.0 zgodnie z ówczesną decyzją autora. Był to stan wydania MVP 6.

Poniższe informacje opisują wcześniejsze wydania.

**MVP 5 ukończone — `v0.5.0` (06.09.2026).** 22 testy jednostkowe przechodziły.
Po refaktoryzacji ręcznie sprawdzono tworzenie z menu, aktywną kolejkę,
zamknięcie, archiwum, ponowne otwarcie i zakończenie programu.
Kolejnym etapem było wprowadzenie odczytowego API w MVP 6.

**MVP 4 ukończone — wersja `v0.4.0` (05.09.2026).** Pełny test regresji objął
tworzenie poprawnych zgłoszeń, wszystkie błędne dane wejściowe, kolejne `Id`,
aktywną kolejkę, pilne zgłoszenia, archiwum, zamknięcie i ponowne otwarcie.
Projekt kompiluje się bez błędów i ostrzeżeń.

**MVP 3 ukończone — wersja `v0.3.0`.** Rozdzielono odpowiedzialności aplikacji,
a wydanie opublikowano wraz z kodem źródłowym i aktualizacją portfolio.

**MVP 2 ukończone — wersja `v0.2.0`.** Projekt zawiera model `Ticket`,
pięć przykładowych zgłoszeń, filtrowanie pilnych i zamkniętych zgłoszeń,
wykrywanie zgłoszeń krytycznych i będących w toku, liczenie otwartych zgłoszeń,
sortowanie według priorytetu oraz regułę natychmiastowej reakcji dla otwartego
zgłoszenia krytycznego.

Projekt obsługuje kontrolowane zmiany stanu: rozpoczęcie obsługi wyłącznie dla
zgłoszenia `Open`, zamknięcie zgłoszenia `Open` lub `InProgress` oraz ponowne
otwarcie wyłącznie zgłoszenia `Closed`. Pozwala również zmienić priorytet tylko
na wartość od `1` do `5`. Właściwości `Status` i `Priority` mają prywatne
settery, dlatego kod zewnętrzny nie może zmieniać ich z pominięciem metod
obiektu.

Interaktywne menu wyświetla pełną i pilną kolejkę, sortowanie według priorytetu
oraz podsumowanie. Pozwala wyszukać zgłoszenie po `Id` i wykonać dozwoloną
zmianę statusu albo priorytetu, pokazując wynik operacji. Konstruktor
odrzuca priorytet spoza zakresu `1–5` oraz status inny niż `Open`, `InProgress`
lub `Closed`. Sprawdzono poprawne wartości graniczne oraz przypadki odrzucane.
