# Decyzje projektowe — Support Ticket Manager

Ten plik zapisuje istotne decyzje podjęte podczas rozwoju projektu. Nie jest
dziennikiem każdej zmiany ani listą przyszłych funkcji.

## MVP 5 — typ statusu, serwis i testy (06.09.2026)

- **Problem:** status był tekstem, tworzenie zgłoszenia należało do kodu konsoli,
  a sprawdzenia reguł wymagały ręcznego powtarzania.
- **Decyzja:** `TicketStatus` nazywa statusy; konstruktor nadal waliduje wartość.
  `TicketService` otrzymuje istniejącą listę przez konstruktor, nadaje ID i tworzy
  zgłoszenia. Menu i serwis współdzielą listę. `readonly` blokuje podmianę pola,
  nie modyfikację zawartości listy. `TicketQueries` zachowuje odczyty kolekcji.
- **Walidacja:** konsola daje komunikaty wejścia, a konstruktor chroni model także
  przy bezpośrednim wywołaniu serwisu. Obiekt jest dodawany dopiero po poprawnym
  skonstruowaniu; odrzucenie nie powinno zmieniać istniejącej kolekcji.
- **Testy:** osobny projekt xUnit z odwołaniem do aplikacji, 9 testów serwisu
  i 13 testów obiektu. Każdy test przygotowuje własne dane. Wymagania określają
  wynik, stan i przypadki graniczne; sama liczba zaliczonych testów nie wystarcza.
- **Alternatywy:** pozostawienie tworzenia w konsoli lub duża przebudowa wszystkich
  operacji. Wybrano małe wydzielenie, które można wykorzystać później w API.
- **Ograniczenia:** lista jest w pamięci, brak obsługi współbieżnego tworzenia
  i trwałego ID. Testy jednostkowe nie potwierdzają podłączenia menu.
- **Dalsza praca:** małe zachowanie -> potrzebne testy -> cały zestaw -> kolejna
  zmiana. Nie odkładamy testowania do końca kolejnego MVP.

## MVP 4 — tworzenie zgłoszeń i archiwum wynikające ze statusu (05.09.2026)

- **Problem:** użytkownik nie mógł dodawać zgłoszeń, a zamknięte elementy były
  częścią pełnej kolejki.
- **Decyzja:** opcja `10` pobiera tytuł, opis i priorytet, automatycznie wylicza
  kolejne `Id`, tworzy `Ticket` ze statusem `Open` i dodaje go do kolekcji w
  pamięci. `GetActiveTickets` zwraca tylko `Open` i `InProgress`.
- **Walidacja:** `string.IsNullOrWhiteSpace` odrzuca brak tytułu i opisu, a
  priorytet wymaga poprawnej konwersji oraz zakresu `1–5`. Konstruktor pozostaje
  ostatnią ochroną poprawności obiektu.
- **Archiwum:** `Closed` oznacza automatyczne archiwum. Ponowne otwarcie zmienia
  status na `Open` i przywraca zgłoszenie do aktywnej kolejki. Nie dodano
  `IsArchived`, ponieważ dublowałoby informację zapisaną w `Status`.
- **Ochrona modelu:** wszystkie właściwości `Ticket` mają `private set`, dzięki
  czemu kod zewnętrzny nie może ominąć konstruktora i metod kontrolujących stan.
- **Ograniczenie:** kolekcja nadal istnieje tylko w pamięci; po ponownym
  uruchomieniu dane tworzone przez użytkownika znikają.

## MVP 3 — rozdzielenie odpowiedzialności aplikacji (05.09.2026)

- **Problem:** Program.cs łączył operacje kolekcji, prezentację i sterowanie.
- **Decyzja:** TicketQueries grupuje metody odczytujące przekazaną kolekcję.
  TicketConsoleView prezentuje listę, szczegóły, podsumowanie i menu.
  TicketConsoleApplication steruje pętlą programu i obsługuje wybór operacji,
  a SampleTicketData tworzy dane demonstracyjne. Reguły pojedynczego
  zgłoszenia pozostają w Ticket.
- **Uzasadnienie:** łatwiej wskazać miejsce zmiany bez powielania reguł.
  Zapytania otrzymują kolekcję przez parametry; widok otrzymuje obiekt zapytań
  przy wywołaniu podsumowania.
- **Alternatywa:** pozostawienie metod lokalnych lub statyczne narzędzia.
  Metody instancji wybrano na tym etapie do nauki współpracy obiektów;
  nie oznacza to, że operacje bez stanu zawsze wymagają instancji.
- **Konsekwencje:** Program.cs jedynie tworzy potrzebne obiekty i uruchamia
  aplikację. Zachowano wcześniejsze operacje menu, dodano komunikat dla pustej
  listy, listę zamkniętych zgłoszeń i podgląd szczegółów zgłoszenia po Id.
  MVP 3 zostało ukończone jako wersja v0.3.0.

## MVP 6 — wspólny kod i odczytowe API (07.09.2026)

- Problem: udostępnić odczyty przez HTTP bez powielania reguł konsoli.
- Decyzja: Core zawiera wspólne klasy; API mapuje żądania na istniejące zapytania.
  Konsola i API są osobnymi procesami i mają osobne listy w pamięci.
- Alternatywa: kopiowanie klas do API; odrzucono z powodu duplikacji reguł.
- Kontrakt: aktywna kolejka posortowana, archiwum, szczegóły; 200 z pustą listą
  dla braku elementów kolekcji, 404 dla nieistniejącego pojedynczego zgłoszenia.
  Statusy w JSON są tekstem, ale wewnętrznie pozostają enumem.
- Zakres: lokalne HTTP/HTTPS, bez zmian danych przez API, bazy i frontendu.
  HTTPS zabezpiecza transmisję, nie zastępuje uwierzytelniania ani walidacji.
- Publikacja: GitHub v0.6.0; aktualizacja portfolio odroczona przez autora.

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
