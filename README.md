# Support Ticket Manager

Konsolowa aplikacja C#/.NET do obsługi zgłoszeń wsparcia. Projekt powstaje
etapami jako pierwszy projekt backendowy w portfolio.

## Cel

Aplikacja ma pokazać praktyczne użycie logiki biznesowej: rejestrowanie zgłoszeń,
priorytetyzację, zmianę statusu, wyszukiwanie, filtrowanie i kontrolę czasu
obsługi. Docelowo projekt będzie rozwijany wraz z nauką kolejnych elementów
.NET — od aplikacji konsolowej do testów, bazy danych i API.

## Użytkownik i problem

Pierwszym użytkownikiem aplikacji jest pracownik wsparcia. Potrzebuje szybko
zobaczyć, które zgłoszenia wymagają najpilniejszej reakcji, bez ręcznego
przeglądania całej kolejki.

## Pierwsze MVP

Pierwsza wersja będzie potrafiła:

- przechowywać przykładowe zgłoszenia;
- wyświetlać wszystkie zgłoszenia;
- filtrować pilne zgłoszenia;
- sprawdzać, czy istnieje zgłoszenie krytyczne;
- liczyć otwarte zgłoszenia;
- sortować zgłoszenia od najwyższego priorytetu;
- prezentować proste podsumowanie kolejki.

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

## Poza zakresem pierwszego MVP

- wprowadzanie, edycja i usuwanie zgłoszeń przez użytkownika;
- konta użytkowników i logowanie;
- zapis do pliku lub bazy danych;
- API i frontend;
- automatyczne obliczanie SLA.

## Technologie

- C#
- .NET 10
- LINQ
- Git i GitHub

## Uruchomienie

```powershell
dotnet run --project src/SupportTicketManager/SupportTicketManager.csproj
```

## Status

Ukończono odczytową część pierwszego etapu MVP. Projekt zawiera model `Ticket`,
pięć przykładowych zgłoszeń, filtrowanie pilnych i zamkniętych zgłoszeń,
wykrywanie zgłoszeń krytycznych i będących w toku, liczenie otwartych zgłoszeń,
sortowanie według priorytetu oraz regułę natychmiastowej reakcji dla otwartego
zgłoszenia krytycznego.

Projekt obsługuje już kontrolowane zmiany stanu: rozpoczęcie obsługi wyłącznie
dla zgłoszenia `Open` oraz zamknięcie zgłoszenia `Open` lub `InProgress`.
Właściwość `Status` ma prywatny setter, dlatego kod zewnętrzny nie może wpisać
dowolnego stanu z pominięciem reguł obiektu.

Najbliższy krok to utrwalenie cyklu życia zgłoszenia i ochrona kolejnych reguł
modelu. Wprowadzanie danych przez użytkownika, baza danych i API nadal pozostają
poza bieżącym etapem.
