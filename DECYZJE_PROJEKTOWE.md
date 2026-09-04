# Decyzje projektowe — Support Ticket Manager

Ten plik zapisuje istotne decyzje podjęte podczas rozwoju projektu. Nie jest
dziennikiem każdej zmiany ani listą przyszłych funkcji.

## Reguły biznesowe należą do klasy `Ticket`

- **Problem:** kod interfejsu mógłby powtarzać warunki statusu i priorytetu.
- **Decyzja:** reguły jednego zgłoszenia oraz kontrolowane zmiany znajdują się w
  metodach klasy `Ticket`, m.in. `IsUrgent`, `TryClose` i `TryChangePriority`.
- **Uzasadnienie:** zmiana reguły odbywa się w jednym miejscu, a kod menu tylko
  koordynuje operację i prezentuje jej wynik.
- **Konsekwencja:** `Status` i `Priority` mają `private set`; kod zewnętrzny
  korzysta z metod obiektu.

## Niepoprawny obiekt nie może zostać utworzony

- **Problem:** `private set` nie sprawdza wartości przekazanych konstruktorowi.
- **Decyzja:** konstruktor odrzuca priorytet spoza `1–5` i nieznany status przez
  odpowiedni wyjątek przed przypisaniem właściwości.
- **Uzasadnienie:** każdy utworzony `Ticket` powinien od początku spełniać
  podstawowe reguły modelu.

## Jedna metoda wyszukuje zgłoszenie po `Id`

- **Problem:** opcje zmieniające stan potrzebują tego samego wyszukiwania.
- **Decyzja:** `FindTicketById(List<Ticket>, int)` używa `FirstOrDefault` i zwraca
  `Ticket?`.
- **Uzasadnienie:** zapytanie LINQ nie jest kopiowane do każdej opcji menu, a
  brak wyniku jest jawnie obsługiwany jako `null`.

## MVP 2 pozostaje aplikacją konsolową

- **Problem:** MVP 1 nie pozwalało pracownikowi wybierać operacji podczas pracy
  programu.
- **Decyzja:** wersja `v0.2.0` używa pętli, `Console.ReadLine`, `TryParse` i
  `switch`, aby udostępnić operacje `1–7` oraz zakończenie przez `0`.
- **Alternatywa:** API i baza danych.
- **Uzasadnienie odroczenia:** najpierw utrwalamy C#, OOP, kolekcje, LINQ i
  walidację wejścia. API oraz trwały zapis należą do kolejnych etapów.
