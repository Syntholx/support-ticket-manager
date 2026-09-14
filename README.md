# Support Ticket Manager

Backend do obsługi zgłoszeń wsparcia napisany w C# i ASP.NET Core.
Użytkownik zgłasza problem i śledzi jego status, a pracownik wsparcia zarządza
kolejką, ustala priorytety i prowadzi zgłoszenia od otwarcia do zamknięcia.

**Wersja 1.0.0 — ukończony etap projektu edukacyjnego, uruchamiany lokalnie.**
Kod jest dostępny do przeglądu i samodzielnego uruchomienia. Nie ma publicznego
serwera API ani działającego demo na portfolio. To nie jest deklaracja gotowości
produkcyjnej; ograniczenia bezpieczeństwa opisano poniżej.

## Co rozwiązuje projekt?

TSM porządkuje obsługę problemów, które bez wspólnej kolejki łatwo zgubić w wiadomościach.
Każde zgłoszenie ma autora, opis, priorytet i status. Wsparcie widzi aktywną kolejkę
według pilności, a zgłoszenia zamknięte trafiają do archiwum bez usuwania ich z bazy.

Dane zgłoszeń i kont są przechowywane w SQL Server i pozostają po restarcie API.
Interakcja odbywa się przez HTTP i JSON, np. za pomocą PowerShell lub klienta HTTP.

## Funkcje i uprawnienia

| Operacja | Zalogowany użytkownik | Rola Support |
|---|---|---|
| Utworzenie zgłoszenia | W swoim imieniu | W swoim imieniu |
| Aktywna kolejka, archiwum, szczegóły | Tylko własne zgłoszenia | Wszystkie zgłoszenia |
| Zamknięcie | Tylko własne zgłoszenie | Dowolne zgłoszenie |
| Rozpoczęcie obsługi | Niedozwolone | Dozwolone |
| Ponowne otwarcie | Niedozwolone | Dozwolone |
| Zmiana priorytetu | Niedozwolona | Dozwolona, zakres 1–5 |

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

| Operacja | Stan początkowy | Stan końcowy |
|---|---|---|
| Rozpoczęcie obsługi | Open | InProgress |
| Zamknięcie | Open lub InProgress | Closed |
| Ponowne otwarcie | Closed | Open |

Niedozwolona zmiana statusu zwraca konflikt i nie zapisuje zmiany.
Zmiana priorytetu jest dozwolona także dla Closed i nie zmienia statusu.
Aktywna kolejka obejmuje Open oraz InProgress, sortowane malejąco po priorytecie.
Przy równym priorytecie kolejność nie jest określona. Archiwum obejmuje Closed.

Tytuł i opis nie mogą być null, puste ani składać się wyłącznie z białych znaków.
Reguły modelu znajdują się w `Ticket`; baza dodatkowo wymusza priorytet 1–5
przez ograniczenie CHECK. Status w C# jest enumem, a w JSON tekstem.

## Technologie i architektura

C#, .NET 10, ASP.NET Core Minimal API, Entity Framework Core 10,
SQL Server, ASP.NET Core Identity, LINQ, xUnit i WebApplicationFactory.

Przepływ: **klient HTTP → endpoint → serwis → model / EF Core → SQL Server**.
Po zakończeniu operacji endpoint dobiera odpowiedź HTTP.

- `src/SupportTicketManager.Core` — Ticket, TicketStatus i reguły zgłoszenia.
- `src/SupportTicketManager.Api/Endpoints` — odbiór żądań, kontrola dostępu i odpowiedzi HTTP.
- `src/SupportTicketManager.Api/Services` — operacje i asynchroniczny zapis/odczyt przez TicketDatabaseService.
- `src/SupportTicketManager.Api/Data` — TicketDbContext i cztery migracje bazy.
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

## Przykład użycia w PowerShell

API musi działać w drugim terminalu. Przykład jest dla nowej lokalnej bazy;
hasło poniżej jest wyłącznie demonstracyjne — nie używaj go poza lokalnym testem.

```powershell
$baseUrl = "https://localhost:7280"
$accountBody = @{
    email = "user@example.com"
    password = "Local-Example!2026"
} | ConvertTo-Json

# Rejestracja: 201; nie oznacza jeszcze zalogowania.
Invoke-RestMethod -Method Post -Uri "$baseUrl/api/auth/register" -ContentType "application/json" -Body $accountBody

# Logowanie: 200. Sesja przechowuje cookie do kolejnych żądań.
Invoke-RestMethod -Method Post -Uri "$baseUrl/api/auth/login" -ContentType "application/json" -Body $accountBody -SessionVariable tsmSession

Invoke-RestMethod -Uri "$baseUrl/api/auth/me" -WebSession $tsmSession

$ticketBody = @{
    title = "Problem z logowaniem"
    description = "Nie mogę zalogować się do systemu firmowego."
} | ConvertTo-Json

$createdTicket = Invoke-RestMethod -Method Post -Uri "$baseUrl/api/tickets" -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($ticketBody)) -WebSession $tsmSession

# POST zwraca 201, Location oraz obiekt z priority=2 i status=Open.
# Korzystamy ze zwróconego ID, nie zakładamy, że wynosi 1.
Invoke-RestMethod -Uri "$baseUrl/api/tickets/$($createdTicket.id)" -WebSession $tsmSession
Invoke-RestMethod -Method Post -Uri "$baseUrl/api/tickets/$($createdTicket.id)/close" -WebSession $tsmSession
Invoke-RestMethod -Uri "$baseUrl/api/tickets/archived" -WebSession $tsmSession
Invoke-RestMethod -Method Post -Uri "$baseUrl/api/auth/logout" -WebSession $tsmSession
```

Po wylogowaniu chronione żądanie zwraca 401. Ponowna rejestracja tego samego
adresu zwraca 400 — istniejącym kontem należy się zalogować.
PowerShell zgłasza wyjątek przy 4xx; po nieudanym przypisaniu zmienna może nadal
zawierać poprzedni wynik.

### Nadanie roli Support lokalnemu kontu

Najpierw zarejestruj osobne konto, np. `support@example.com`, przez endpoint rejestracji.
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

| Metoda i ścieżka | Dane / cel | Odpowiedzi |
|---|---|---|
| POST /api/auth/register | email, password | 201 / 400 |
| POST /api/auth/login | email, password; cookie sesji | 200 / 400 / 401 |
| GET /api/auth/me | Bieżące konto | 200 / 401 |
| POST /api/auth/logout | Wylogowanie | 200 / 401 |
| GET /api/status | Nazwa, wersja, isRunning | 200 |
| GET /api/name | Nazwa aplikacji | 200 |

Wszystkie poniższe trasy zwracają **401 bez zalogowania**.
Trasy tylko dla Support zwracają **403 zwykłemu użytkownikowi**, także jeśli poda nieistniejące ID.

| Metoda i ścieżka | Body | Sukces / błędy po autoryzacji |
|---|---|---|
| GET /api/tickets | Brak | 200, również [] |
| GET /api/tickets/archived | Brak | 200, również [] |
| GET /api/tickets/{id} | Brak | 200 / 404 |
| POST /api/tickets | {"title":"Problem","description":"Opis"} | 201 + Location / 400 |
| POST /api/tickets/{id}/close | Brak | 200 / 404 / 409 |
| POST /api/tickets/{id}/start | Brak; Support | 200 / 404 / 409 |
| POST /api/tickets/{id}/reopen | Brak; Support | 200 / 404 / 409 |
| POST /api/tickets/{id}/priority | {"priority":4}; Support | 200 / 400 / 404 |

ID w trasie musi być liczbą całkowitą. Odpowiedzi operacji zakończonych sukcesem
zawierają zgłoszenie. Błędy biznesowe mają zazwyczaj obiekt `{"message":"..."}`;
401/403 i błędy frameworka nie muszą zawierać tego pola.

## Testy

Zestaw obejmuje **101 przypadków testowych**: reguły modelu, operacje API,
integrację z SQL, Identity, sesje, własność zgłoszeń i macierz uprawnień.
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

Projekt zamyka etap nauki backendu na przykładzie TSM, nie cały kurs.
Przed ewentualnym publicznym wdrożeniem potrzebne są m.in.:

- pełna ochrona CSRF dla logowania i operacji opartych na cookies;
- ograniczanie ruchu i rozmiaru danych, paginacja oraz ochrona przed nadużyciami;
- potwierdzanie e-maila, reset hasła i dopracowanie cyklu życia sesji;
- obsługa konfliktów równoczesnych aktualizacji;
- konfiguracja produkcyjnych sekretów, kluczy Data Protection, TLS, logów i kopii bazy;
- osobna weryfikacja integracji przeglądarkowej i wdrożenia.

Cookies mają HttpOnly, Secure i SameSite=Strict, ale nie zastępuje to powyższych
zabezpieczeń. Wylogowanie usuwa cookie klienta, nie konto ani każdą wcześniejszą
kopię cookie. **Nie wystawiaj obecnej konfiguracji bezpośrednio do internetu.**

Nie ma usuwania zgłoszeń, przypisywania konkretnego pracownika, załączników ani SLA.
Rejestracja jest dostępna anonimowo. Nie ma zaplanowanego resetu publicznej bazy,
ponieważ publiczne demo z bazą nie zostało wdrożone.

## Interfejs i historia projektu

Kod makiety oraz wcześniejszego lokalnego interfejsu zachowano w
[repozytorium portfolio](https://github.com/Syntholx/portfolio/tree/main/tsm-demo).
Nie jest publikowany jako działające demo. Makieta używa danych przykładowych,
a lokalny panel nie został dostosowany do aktualnego logowania i uprawnień API.

Historyczna konsola i kolejne etapy MVP są dostępne w tagach v0.1.0–v0.7.0.
W obecnej wersji pozostają Core, Api i Tests; stara konsola i listowe serwisy nie są wymagane.

## Autorstwo

Projekt powstał w ramach prowadzonej nauki C#/.NET. Autor implementował logikę
i endpointy z przykładami oraz review mentora AI. Część testów, ich adaptację,
interfejs i dokumentację przygotowano z pomocą AI.
Projekt nie jest przedstawiany jako wykonany całkowicie samodzielnie.
