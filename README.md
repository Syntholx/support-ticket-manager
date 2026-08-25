# Support Ticket Manager

Konsolowa aplikacja C#/.NET do obsługi zgłoszeń wsparcia. Projekt powstaje
etapami jako pierwszy projekt backendowy w portfolio.

## Cel

Aplikacja ma pokazać praktyczne użycie logiki biznesowej: rejestrowanie zgłoszeń,
priorytetyzację, zmianę statusu, wyszukiwanie, filtrowanie i kontrolę czasu
obsługi. Docelowo projekt będzie rozwijany wraz z nauką kolejnych elementów
.NET — od aplikacji konsolowej do testów, bazy danych i API.

## Pierwsze MVP

Pierwsza wersja będzie potrafiła:

- przechowywać przykładowe zgłoszenia;
- wyświetlać zgłoszenia;
- filtrować je według wybranej reguły;
- sprawdzać, czy istnieje zgłoszenie wymagające uwagi;
- prezentować proste podsumowanie.

Dokładny model zgłoszenia i pierwsza reguła biznesowa zostaną ustalone przed
implementacją. Zakres MVP pozostaje celowo mały.

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

Projekt w przygotowaniu. Pierwszy etap implementacji rozpocznie się po ustaleniu
wymagań MVP.
