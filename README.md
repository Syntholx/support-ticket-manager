# Support Ticket Manager

Aplikacja webowa do obsługi zgłoszeń wsparcia: frontend React i TypeScript
oraz API napisane w C# i ASP.NET Core.
Użytkownik zgłasza problem i śledzi jego status, a pracownik wsparcia zarządza
kolejką, ustala priorytety i prowadzi zgłoszenia od otwarcia do zamknięcia.

Projekt działa lokalnie; nie ma publicznego serwera API ani działającego demo online.
Interfejs obejmuje obsługę konta, aktywne zgłoszenia, archiwum i operacje Support.
Cookies sesyjne, CSRF, limity ruchu i kontrolę równoczesnego zapisu obsługuje backend.
Publiczne wdrożenie jest osobnym zadaniem wymagającym odbioru środowiska —
[instrukcja produkcyjna](docs/PRODUCTION.md).

## Co rozwiązuje projekt?

TSM porządkuje obsługę problemów, które bez wspólnej kolejki łatwo zgubić w wiadomościach.
Każde zgłoszenie ma autora, opis, priorytet i status. Wsparcie widzi aktywną kolejkę
według pilności, a zgłoszenia zamknięte trafiają do archiwum bez usuwania ich z bazy.

Dane zgłoszeń i kont są przechowywane w SQL Server i pozostają po restarcie API.
Użytkownik korzysta z interfejsu w przeglądarce, który komunikuje się z API przez HTTP i JSON.

## Funkcje interfejsu

- Rejestracja, potwierdzanie e-maila, ponowna wysyłka potwierdzenia, logowanie i wylogowanie.
- Zamówienie linku resetującego i ustawienie nowego hasła.
- Lista aktywnych zgłoszeń i archiwum z osobną paginacją po 5 elementów.
- Wyszukiwanie po tytule, skróty wyszukiwania, sortowanie ID rosnąco/malejąco i reset widoku.
- Tworzenie zgłoszenia z możliwością natychmiastowego otwarcia jego szczegółów.
- Szczegóły, zamykanie, a dla Support także rozpoczęcie obsługi, ponowne otwarcie i priorytet.
- Komunikaty ładowania, błędów i pustych wyników oraz blokady kontrolek podczas operacji.

Wyszukiwanie i sortowanie dotyczą wyłącznie pobranej strony danych, nie całej bazy.
Domyślny widok klienta sortuje tę stronę według ID; kolejność serwera opisana niżej
decyduje o tym, które zgłoszenia trafiają do danej strony.

## Funkcje i uprawnienia

| Operacja                             | Zalogowany użytkownik   | Rola Support          |
| ------------------------------------ | ----------------------- | --------------------- |
| Utworzenie zgłoszenia                | W swoim imieniu         | W swoim imieniu       |
| Aktywna kolejka, archiwum, szczegóły | Tylko własne zgłoszenia | Wszystkie zgłoszenia  |
| Zamknięcie                           | Tylko własne zgłoszenie | Dowolne zgłoszenie    |
| Rozpoczęcie obsługi                  | Niedozwolone            | Dozwolone             |
| Ponowne otwarcie                     | Niedozwolone            | Dozwolone             |
| Zmiana priorytetu                    | Niedozwolona            | Dozwolona, zakres 1–5 |

- Rejestracja, logowanie, odczyt zalogowanego konta i wylogowanie wykorzystują ASP.NET Core Identity.
- Wszystkie endpointy zgłoszeń wymagają uwierzytelnienia.
- Autor jest pobierany z tożsamości zalogowanego konta, nie z JSON-a klienta.
- Nowe zgłoszenie otrzymuje priorytet **2** i status **Open**.
  Dodatkowe pola `priority` i `ownerId` przy tworzeniu nie zmieniają tych zasad.
- Cudze zgłoszenie jest dla zwykłego użytkownika niedostępne: szczegóły i zamknięcie
  zwracają 404, a listy go nie zawierają.
- Support ma dostęp również do historycznych zgłoszeń bez autora.
- Rejestracja nie przyznaje roli Support. Nadaje się ją osobnym poleceniem lokalnym.

## Model i reguły

Zgłoszenie zawiera `id`, `title`, `description`, `priority`, `status` i `ownerId`.
Identyfikator nadaje SQL Server; luki w numeracji są dopuszczalne.
`OwnerId` jest opcjonalnym kluczem obcym do kont Identity ze względu na starsze dane.
Nowe zgłoszenia tworzone przez API zawsze mają autora.

| Operacja            | Stan początkowy     | Stan końcowy |
| ------------------- | ------------------- | ------------ |
| Rozpoczęcie obsługi | Open                | InProgress   |
| Zamknięcie          | Open lub InProgress | Closed       |
| Ponowne otwarcie    | Closed              | Open         |

Niedozwolona zmiana statusu zwraca konflikt i nie zapisuje zmiany.
Zmiana priorytetu jest dozwolona także dla Closed i nie zmienia statusu.
Aktywna kolejka obejmuje Open oraz InProgress, sortowane malejąco po priorytecie.
Przy równym priorytecie kolejność to Id rosnąco. Archiwum obejmuje Closed, również według Id.

Tytuł i opis nie mogą być null, puste ani składać się wyłącznie z białych znaków.
Reguły modelu znajdują się w `Ticket`; baza dodatkowo wymusza priorytet 1–5
przez ograniczenie CHECK. Status w C# jest enumem, a w JSON tekstem.

## Technologie i architektura

Frontend: React, TypeScript, React Router, Tailwind CSS, Vite,
Vitest, React Testing Library, user-event i jsdom.

Backend: C#, .NET 10, ASP.NET Core Minimal API, Entity Framework Core 10,
SQL Server, ASP.NET Core Identity, LINQ, xUnit i WebApplicationFactory.

Przepływ: **komponent React → fetch → endpoint → serwis → model / EF Core → SQL Server**.
Po zakończeniu operacji endpoint dobiera odpowiedź HTTP.

- `frontend/components` — ekrany i wspólne elementy interfejsu.
- `frontend/hooks` — odczyt danych, sesja i operacje zgłoszeń.
- `frontend/validation` — współdzielona logika walidacji.
- `frontend/tests` — testy logiki i zachowania komponentów.
- `src/SupportTicketManager.Core` — Ticket, TicketStatus i reguły zgłoszenia.
- `src/SupportTicketManager.Api/Endpoints` — odbiór żądań, kontrola dostępu i odpowiedzi HTTP.
- `src/SupportTicketManager.Api/Services` — operacje i asynchroniczny zapis/odczyt przez TicketDatabaseService.
- `src/SupportTicketManager.Api/Data` — TicketDbContext i migracje bazy.
- `src/SupportTicketManager.Api/Identity` — model konta i lokalne nadawanie roli Support.
- `src/SupportTicketManager.Api/Program.cs` — konfiguracja DI, SQL, Identity, cookies, JSON i tras.
- `tests/SupportTicketManager.Tests` — testy reguł, HTTP, bazy i uprawnień.

Serwis i kontekst są rejestrowane jako Scoped. Serwis zwraca
`TicketOperationResult`, a endpoint mapuje wynik na HTTP.
`Add` przygotowuje dodanie encji; `SaveChangesAsync` zapisuje zmianę w SQL.
Operacje wymagające Support chroni polityka endpointów — same metody serwisu
nie są niezależną granicą autoryzacji dla innych aplikacji.

## Uruchomienie lokalne

### Wymagania

- .NET SDK 10.
- Działający lokalny SQL Server, np. w Docker Desktop, dostępny pod
  `localhost,1433` albo `127.0.0.1,1433`.
- Lokalny login SQL z uprawnieniami potrzebnymi do migracji.
  Testy dodatkowo wymagają tworzenia i usuwania baz.
- Zaufany deweloperski certyfikat HTTPS.
- Node.js 24 i npm (wersja użyta do weryfikacji: Node.js 24.19.0).
- Lokalny serwer SMTP, np. Mailpit, do potwierdzania kont i resetowania hasła.

Repozytorium nie uruchamia kontenera SQL automatycznie i nie zawiera hasła bazy.
Nie używaj serwera produkcyjnego ani ważnych danych do ćwiczeń/testów.

### 1. Pobierz kod i narzędzia

```powershell
git clone https://github.com/Syntholx/support-ticket-manager.git
cd support-ticket-manager
dotnet restore
dotnet tool restore
```

Manifest `dotnet-tools.json` przypina lokalny dotnet-ef do wersji 10.0.12.

### 2. Ustaw połączenie poza repozytorium

Projekt API ma już UserSecretsId. Ustaw połączenie przez User Secrets, zastępując
login i hasło swoimi wartościami. Poniższy tekst jest szablonem, nie działającym sekretem:

```powershell
dotnet user-secrets set "ConnectionStrings:TicketDatabase" 'Server=localhost,1433;Database=SupportTicketManager;User Id=YOUR_LOCAL_SQL_LOGIN;Password=YOUR_LOCAL_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True' --project src/SupportTicketManager.Api
```

Nie wklejaj prawdziwego hasła do README, kodu, commita ani zgłoszenia na GitHubie.
Polecenie z hasłem może trafić do historii terminala — można zamiast tego edytować
lokalny plik User Secrets. User Secrets nie jest szyfrowanym sejfem produkcyjnym.
`TrustServerCertificate=True` służy wyłącznie temu lokalnemu środowisku SQL.

Uruchom lokalny Mailpit z portem SMTP 1025 i panelem HTTP 8025 dostępnymi
tylko lokalnie. Repozytorium nie uruchamia go automatycznie. Dla tego wariantu
ustaw konfigurację API poza repozytorium:

```powershell
dotnet user-secrets set "Email:Host" "127.0.0.1" --project src/SupportTicketManager.Api
dotnet user-secrets set "Email:Port" "1025" --project src/SupportTicketManager.Api
dotnet user-secrets set "Email:From" "tsm@example.com" --project src/SupportTicketManager.Api
dotnet user-secrets set "Email:FrontendBaseUrl" "https://localhost:5500" --project src/SupportTicketManager.Api
```

Panel poczty: **http://localhost:8025**. Wyjątek SMTP bez TLS i logowania działa
wyłącznie w Development dla Host=127.0.0.1 i Port=1025; nie używaj go produkcyjnie.
Testy backendu używają poczty w pamięci; uruchomione API nie potwierdza kont automatycznie.
Konfiguracja rzeczywistego dostawcy SMTP: [instrukcja](docs/PRODUCTION.md).

### 3. Zastosuj migracje

```powershell
dotnet ef database update --project src/SupportTicketManager.Api --startup-project src/SupportTicketManager.Api -- --environment Development
```

Migracje tworzą tabelę Tickets, CHECK priorytetu, tabele Identity oraz powiązanie
autora. Na nowej bazie nie ma przykładowych kont ani zgłoszeń.
Nie uruchamiaj historycznych ćwiczeń SQL jako konfiguracji projektu.

### 4. Uruchom API przez HTTPS

```powershell
dotnet dev-certs https --trust
dotnet run --project src/SupportTicketManager.Api --launch-profile https
```

Adres: **https://localhost:7280**. `GET /api/status` zwraca nazwę i wersję aplikacji;
nie sprawdza połączenia z bazą. Sam adres `/` nie ma strony startowej ani Swagger UI.

Profil uruchamia także HTTP na porcie 5231, ale cookies są Secure:
do rejestracji, logowania i wszystkich operacji używaj HTTPS.
Nie wyłączaj sprawdzania certyfikatu w kliencie.

### 5. Przygotuj certyfikat i uruchom frontend

Konfiguracja Vite wymaga istniejącego certyfikatu HTTPS dla localhost w formacie
PEM oraz odpowiadającego mu klucza prywatnego:

- `%LOCALAPPDATA%\TSMDevCert\localhost.pem`
- `%LOCALAPPDATA%\TSMDevCert\localhost.key`

Przygotuj te pliki lokalnie i zapewnij zaufanie certyfikatowi w przeglądarce.
Samo `dotnet dev-certs https --trust` nie eksportuje ich pod powyższe ścieżki.
Nie publikuj klucza; ogranicz dostęp do katalogu. Obecna konfiguracja ścieżki
certyfikatu jest przeznaczona dla Windows i wymaga LOCALAPPDATA.

W drugim terminalu, z głównego katalogu repozytorium:

```powershell
cd frontend
npm.cmd ci
npm.cmd run dev
```

Otwórz **https://localhost:5500**. API musi działać równolegle na porcie 7280;
proxy Vite przekazuje tam `/api/`. Port 5500 jest stały i musi być wolny.
Nie zmieniaj go bez dostosowania Email:FrontendBaseUrl. Używaj wszędzie localhost,
aby nie mieszać pochodzenia stron i cookies. Na nowej bazie zarejestruj konto,
otwórz mail w Mailpit, potwierdź adres i dopiero wtedy się zaloguj.

## Korzystanie z API

Najpierw pobierz GET /api/auth/csrf, zachowując cookie w sesji klienta.
Do każdego POST dodaj X-CSRF-TOKEN z odpowiedzi. Po login/logout pobierz go ponownie.
Rejestracja nie loguje ani nie potwierdza konta — wymagane potwierdzenie e-mail.
Nowe trasy kont, format linków i limity: [kontrakt bezpieczeństwa](docs/PRODUCTION.md).
Stare przykłady POST bez CSRF z wersji 1.0.0 nie działają z nowym API.

### Nadanie roli Support lokalnemu kontu

Najpierw zarejestruj i potwierdź e-mail osobnego konta, np. `support@example.com`.
Następnie zatrzymaj API przez Ctrl+C i wykonaj:

```powershell
dotnet run --project src/SupportTicketManager.Api --launch-profile https -- --grant-support support@example.com
```

Polecenie działa tylko w Development i dla lokalnego SQL pod wskazanymi wyżej
adresami. Sprawdza istnienie konta, tworzy rolę w razie potrzeby i przypisuje ją
bez duplikowania. Następnie kończy proces — nie uruchamia serwera.
Zwykły start aplikacji nie nadaje nikomu uprawnień.

Uruchom API ponownie i zaloguj konto Support ponownie, aby nowe cookie zawierało rolę.
Nie jest to publiczny endpoint administracyjny ani mechanizm administracji produkcyjnej.

## Kontrakt HTTP

W tabelach podano odpowiedzi obsługiwane przez aplikację.
Nieprawidłowy JSON może zostać odrzucony przez ASP.NET Core przed endpointem.

| Metoda i ścieżka                   | Dane / cel                             | Odpowiedzi      |
| ---------------------------------- | -------------------------------------- | --------------- |
| GET /api/auth/csrf                 | Token CSRF + cookie                    | 200             |
| POST /api/auth/register            | email, password; wysyłka potwierdzenia | 201 / 400 / 503 |
| POST /api/auth/confirm-email       | userId, token                          | 200 / 400       |
| POST /api/auth/resend-confirmation | email                                  | 202 / 400       |
| POST /api/auth/forgot-password     | email                                  | 202 / 400       |
| POST /api/auth/reset-password      | email, token, password                 | 200 / 400       |
| POST /api/auth/login               | email, password; cookie sesji          | 200 / 400 / 401 |
| GET /api/auth/me                   | Bieżące konto: id, name, isSupport      | 200 / 401       |
| POST /api/auth/logout              | Wylogowanie                            | 200 / 401       |
| GET /api/status                    | Nazwa, wersja, isRunning               | 200             |
| GET /api/name                      | Nazwa aplikacji                        | 200             |

Wszystkie poniższe trasy zwracają **401 bez zalogowania**.
Trasy tylko dla Support zwracają **403 zwykłemu użytkownikowi**, także jeśli poda nieistniejące ID.

| Metoda i ścieżka                | Body                                     | Sukces / błędy po autoryzacji |
| ------------------------------- | ---------------------------------------- | ----------------------------- |
| GET /api/tickets                | Brak                                     | 200, również []               |
| GET /api/tickets/archived       | Brak                                     | 200, również []               |
| GET /api/tickets/{id}           | Brak                                     | 200 / 404                     |
| POST /api/tickets               | {"title":"Problem","description":"Opis"} | 201 + Location / 400          |
| POST /api/tickets/{id}/close    | Brak                                     | 200 / 404 / 409               |
| POST /api/tickets/{id}/start    | Brak; Support                            | 200 / 404 / 409               |
| POST /api/tickets/{id}/reopen   | Brak; Support                            | 200 / 404 / 409               |
| POST /api/tickets/{id}/priority | {"priority":4}; Support                  | 200 / 400 / 404 / 409         |

ID w trasie musi być liczbą całkowitą. Odpowiedzi operacji zakończonych sukcesem
zawierają zgłoszenie. Błędy biznesowe mają zazwyczaj obiekt `{"message":"..."}`;
401/403 i błędy frameworka nie muszą zawierać tego pola.

## Testy i kompilacja

### Frontend

Polecenia uruchamiaj w katalogu `frontend` po `npm.cmd ci`:

```powershell
npm.cmd test
npm.cmd run typecheck:tests
node ./node_modules/typescript/bin/tsc --noEmit
node ./node_modules/vite/bin/vite.js build
```

Pierwsze polecenie uruchamia testy raz. Osobna kontrola typów obejmuje testy
i ich importy; główny tsconfig sprawdza aplikację od main.tsx. Build zapisuje
klienta w ignorowanym katalogu `dist`. Ze względu na obecną konfigurację Vite
także build wymaga lokalnych plików certyfikatu; testy mają oddzielny config.

10 testów obejmuje porównywanie haseł oraz zachowanie formularzy resetu i resendu:
walidację, brak danych linku, callback logowania, sukces, odpowiedź 400,
błąd sieci i ponowienie, dane żądania oraz ogólną odpowiedź 202.
Odpowiedzi fetch są symulowane; testy nie uruchamiają API, SQL ani Mailpit
i nie dowodzą rzeczywistej wysyłki poczty lub zmiany hasła na serwerze.
jsdom sprawdza DOM i interakcje, nie rzeczywisty układ graficzny przeglądarki.

Ręcznie sprawdzono główne przepływy konta i zgłoszeń, role, błędy połączenia,
Slow 3G, widok 375 px i klawiaturę. To nie pełny audyt dostępności ani zestaw E2E.
Build może zgłaszać ostrzeżenia o przyszłym native config loader Vite oraz
dyrektywie use client w zależności react-router; nie blokowały kompilacji.

### Backend

Zestaw obejmuje reguły modelu, operacje API,
integrację z SQL, Identity, sesje, własność zgłoszeń i macierz uprawnień.
Nowe scenariusze obejmują CSRF, limity, potwierdzanie e-mail, reset hasła,
blokadę logowania, wygasłe tokeny, paginację, rowversion i walidację konfiguracji.
Sprawdzane są nie tylko kody HTTP, ale również zwrócone dane i stan bazy po operacji.

Testy SQL korzystają z prawdziwego lokalnego SQL Server.
Fabryka podmienia nazwę bazy na unikalną `SupportTicketManagerTests_...`,
wykonuje migracje i po teście usuwa wyłącznie tę bazę.
Nie używa zgłoszeń z bazy aplikacji. Konto SQL musi móc tworzyć i usuwać bazy.

```powershell
dotnet build SupportTicketManager.slnx --configuration Release
dotnet test SupportTicketManager.slnx --configuration Release
```

Przed budowaniem zatrzymaj API, jeśli blokuje plik wykonywalny.
Same testy reguł Ticket, bez połączenia z SQL:

```powershell
dotnet test SupportTicketManager.slnx --filter FullyQualifiedName~TicketTests
```

Zaliczenie testów nie oznacza pełnego audytu bezpieczeństwa ani testu rzeczywistej
przeglądarki/transportu TLS. Testy HTTP korzystają z WebApplicationFactory.

## Ograniczenia i dalszy rozwój

Dodano mechanizmy bezpieczeństwa opisane w [PRODUCTION.md](docs/PRODUCTION.md).
Nie oznacza to zakończonego audytu ani gotowego środowiska produkcyjnego.
Przed publicznym wdrożeniem wymagane są rzeczywisty dostawca SMTP, TLS, sekrety,
backup/restore, konfiguracja hostingu oraz testy przeglądarkowe i obciążeniowe.
Limity ruchu są per instancja; publiczny ruch wymaga też ochrony na bramie.

Listy API nie zwracają totalCount ani hasNext. Pełna strona pozwala przejść dalej,
więc kolejna może być pusta. Reset widoku czyści wyszukiwanie i sortowanie,
nie numer strony. Numery stron nie są utrwalane po odświeżeniu przeglądarki.
Przyciski są blokowane podczas operacji, ale nawigacja wstecz przeglądarki podczas
POST nie ma pełnej ochrony cyklu życia żądania. F5 na ekranie resetu/potwierdzenia
usuwa dane linku z pamięci; trzeba ponownie otworzyć wiadomość.

Nie ma usuwania zgłoszeń, przypisywania konkretnego pracownika, załączników ani SLA.
Rejestracja jest dostępna anonimowo. Nie ma zaplanowanego resetu publicznej bazy,
ponieważ publiczne demo z bazą nie zostało wdrożone.

## Interfejs i historia projektu

Klient React i TypeScript znajduje się w `frontend/`, w tym samym repo co backend.
Rozpoczął się od ekranu statusu API, a obecnie obejmuje przepływy opisane powyżej.
`node_modules` odtwarza `npm.cmd ci`; `dist` jest wynikiem budowania i nie jest
potrzebny podczas pracy przez serwer Vite. Oba katalogi są ignorowane w Git.
`invalid-status.json` służy do ręcznych prób walidacji; po próbach może zawierać
poprawne dane. Normalne działanie klienta korzysta z API, nie tego pliku.
Projekt służy obecnie nauce frontendu; istniejące API jest backendem ćwiczeniowym.

Kod makiety oraz wcześniejszego lokalnego interfejsu zachowano w
[repozytorium portfolio](https://github.com/Syntholx/portfolio/tree/main/tsm-demo).
Nie jest publikowany jako działające demo. Makieta używa danych przykładowych,
a lokalny panel nie został dostosowany do aktualnego logowania i uprawnień API.

Historyczna konsola i kolejne etapy MVP są dostępne w tagach v0.1.0–v0.7.0.
W obecnej wersji pozostają Core, Api i Tests; stara konsola i listowe serwisy nie są wymagane.

## Autorstwo

Projekt rozpoczął się w ramach prowadzonej nauki C#/.NET, następnie rozwijano
klienta React i TypeScript. Autor implementował frontend z kontraktami, przykładami
i review mentora AI. Backend oraz testy powstawały z pomocą AI; część testów
backendowych i dokumentację przygotował asystent. Testy frontendu autor pisał
w ramach prowadzonej nauki na przykładach i z korektami.
Projekt nie jest przedstawiany jako wykonany całkowicie samodzielnie.
