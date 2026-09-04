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

## Poza zakresem MVP 2

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
