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
- xUnit (testy jednostkowe)
- Git i GitHub

## Uruchomienie

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

Projekt testowy `tests/SupportTicketManager.Tests` zawiera **22 testy xUnit**:

- 9 testów serwisu: ID dla pustej i niepustej listy, tworzenie, kolejne ID,
  odrzucanie błędnego priorytetu, tytułu i opisu oraz zachowanie istniejącej listy;
- 13 testów `Ticket`: zamknięcie, rozpoczęcie i ponowne otwarcie dla wszystkich
  trzech statusów początkowych oraz granice priorytetu `0`, `1`, `5`, `6`.

Testy sprawdzają opisane przypadki, nie gwarantują poprawności całego programu.
Podłączenie menu pozostaje sprawdzane ręcznie. Dane nadal istnieją tylko w pamięci;
nie ma API, bazy danych ani interfejsu przeglądarkowego.

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

**MVP 5 ukończone — `v0.5.0` (06.09.2026).** 22 testy jednostkowe przechodzą.
Po refaktoryzacji ręcznie sprawdzono tworzenie z menu, aktywną kolejkę,
zamknięcie, archiwum, ponowne otwarcie i zakończenie programu.
Następny etap: wprowadzenie HTTP i pierwszego odczytowego API, z dalszą
aktywną praktyką testowania. Poniższe informacje opisują wcześniejsze wydania.

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
