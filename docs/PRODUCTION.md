# Bezpieczeństwo i wdrożenie TSM

Kod przygotowywany do wdrożenia — nie deklaracja wykonanego audytu/uruchomionej produkcji.
Wariant bazowy: jedna instancja API, SQL Server, frontend i /api pod jednym originem HTTPS.
Osobne domeny wymagają osobnego projektu cookies/CORS/CSRF; nie AllowAnyOrigin.

## Nowy kontrakt klienta

- GET /api/auth/csrf ustawia cookie i zwraca { token }. Każdy POST wymaga
  tego cookie oraz X-CSRF-TOKEN. Po login/logout pobierz nowy token dla nowej tożsamości.
  Brak/zły token: 400; autoryzacja nadal wcześniej zwraca 401/403.
- POST /api/auth/register { email, password }: 201 oznacza niepotwierdzone konto
  i wysłaną wiadomość. Awaria wysyłki: 503; konto pozostaje, można ponowić wysyłkę.
- POST /api/auth/confirm-email { userId, token }: 200/400. Login przed potwierdzeniem: 401.
- POST /api/auth/resend-confirmation oraz /api/auth/forgot-password { email }: ogólne 202,
  także dla brakującego konta, nieodpowiedniego stanu lub awarii wysyłki.
- POST /api/auth/reset-password { email, token, password }: 200/400.
  Hasło: 12–128 znaków, minimum 4 różne, duża/mała litera, cyfra, znak specjalny.
  Linki ważne godzinę. Reset unieważnia poprzedni token i sesje przez security stamp.
- Logout unieważnia wszystkie sesje konta. Absolutny czas sesji: 30 minut;
  security stamp sprawdzany przy każdym żądaniu. Cookies starej wersji wymagają ponownego loginu.
- Tytuł do 200, opis do 5000 znaków UTF-16, ograniczenia także w SQL.
- Listy: ?page=1&pageSize=50. page 1–10000, pageSize 1–100; wynik nadal tablicą.
  Pusta strona kończy odczyt. Remisy priorytetu i archiwum: Id rosnąco.
- Nakładające się zapisy chroni rowversion; konflikt daje 409, odśwież i świadomie ponów.
  To nie jest ETag/If-Match chroniący stary ekran przed późniejszym, sekwencyjnym zapisem.

Linki prowadzą do frontendu /confirm-email lub /reset-password. Parametry są po #,
więc nie trafiają w URL żądania HTTP. UI odczytuje fragment, usuwa go z paska przez
history.replaceState, pobiera CSRF i wysyła POST po potwierdzeniu użytkownika.
Nie loguj tokenów ani nie ładuj analityki na tych ekranach. Sam GET linku nic nie zmienia.
Ekrany te trzeba zbudować i przetestować w frontend/; obecnie klient ma tylko status API.

## Limity i dostępność

Na instancję: 120 żądań/min/IP, 60/min/konto, 10 POST/min/IP do /api/auth,
32 równoczesne żądania, brak kolejki. 429 i Retry-After, gdy limiter zna czas oczekiwania.
Limity pamięciowe resetują się po restarcie, nie są współdzielone; skalowanie wymaga
wspólnego limitera na bramie/WAF. To nie zastępuje ochrony DDoS.
E-mail: raz na 5 minut/konto/typ, atomowa blokada w SQL, zwalniana przy błędzie wysyłki.
Brak trwałej kolejki SMTP: ponowienie przez endpoint, monitoring awarii i limity dostawcy.
Identity: blokada konta na 15 minut po 5 błędnych hasłach.
Body do 32 KiB, także bez Content-Length; timeout żądania 30 s, SMTP 15 s.
Proxy również musi ograniczać body, połączenia i czas odczytu. SQL respektuje anulowanie żądania.

## Konfiguracja poza repo

| Zmienna środowiskowa | Wymaganie |
|---|---|
| ASPNETCORE_ENVIRONMENT | Production |
| AllowedHosts | Jawne hosty rozdzielone średnikiem, bez * |
| ConnectionStrings__TicketDatabase | Docelowa baza, Encrypt=True;TrustServerCertificate=False |
| DataProtection__KeyPath | Istniejący absolutny katalog na trwałym wolumenie |
| DataProtection__CertificatePath | PFX z kluczem prywatnym |
| DataProtection__CertificatePassword | Sekret hasła PFX |
| Security__KnownProxies__0 | Dokładny IP zaufanego proxy; pomiń przy bezpośrednim TLS |
| Email__Host, Email__Port | SMTP 587 STARTTLS albo 465 TLS |
| Email__Username, Email__Password | Sekrety SMTP |
| Email__From | Zweryfikowany adres nadawcy |
| Email__FrontendBaseUrl | Publiczny HTTPS frontendu, bez query/fragmentu |

Production odmawia startu z niebezpiecznym SQL TLS, wildcard hostem, brakiem
trwałych kluczy/certyfikatu lub SMTP. Nie używaj Development do ominięcia kontroli.
HTTP w Production jest odrzucane. Proxy przekazuje X-Forwarded-Proto/For; ufamy
tylko skonfigurowanym IP i jednemu hopowi. Port backendu za proxy nie może być publiczny.
Nie używaj konta SQL sa: runtime ma tylko niezbędne DML, migracje osobne konto.

Klucze Data Protection są szyfrowane certyfikatem. ACL katalogu/PFX tylko dla
aplikacji i administratora, poza webrootem. Backup kluczy, PFX i hasła przechowuj
bezpiecznie; wiele instancji potrzebuje wspólnego key ring. Nie usuwaj starych kluczy.
Rotacja certyfikatu wymaga obsługi odszyfrowania poprzednim certyfikatem lub
zaplanowanego unieważnienia sesji/linków — nie podmieniaj PFX w ciemno.

## Checklista przed publicznym wdrożeniem

1. Backup SQL i próbne odtworzenie; ustalenie RPO/RTO, retencji i alertów.
2. Review migracji HardenTicketStorage, EnforceDescriptionLimit, AccountEmailCooldown.
   Za długie stare dane zatrzymują migrację, nie są obcinane. Skrypt do review:
   `dotnet ef migrations script --idempotent --project src/SupportTicketManager.Api -- --environment Development`.
   API nie migruje automatycznie; osobne konto migracji, okno zmian i plan rollbacku.
3. Istniejące konta potwierdzają e-mail normalnym przepływem, nie masowym update flagi.
   Produkcyjne przyznawanie Support wymaga kontrolowanego procesu administratora;
   developerskie --grant-support celowo działa tylko lokalnie.
4. Staging: rejestracja → e-mail → potwierdzenie → login → zgłoszenie → logout;
   reset hasła, wygasły/fałszywy token, brak CSRF, 401/403/404/409/413/429.
5. Test rzeczywistą przeglądarką: cookies, same-origin, TLS, nagłówki, brak cache.
   TestServer nie weryfikuje polityk przeglądarki ani certyfikatu hostingu.
6. Load test API/SQL/SMTP, ochrona DDoS/WAF, alerty 5xx/429 i limit kosztów poczty.
7. Logi bez haseł/cookies/tokenów/treści zgłoszeń; nie włączaj SensitiveDataLogging.
   Kontrola dostępu i retencji logów. /api/status to liveness, nie test bazy.
8. Regularny restore drill, aktualizacje runtime/pakietów i skan zależności.

Pozostają wybór hostingu/domeny, SMTP (SPF/DKIM/DMARC), sekrety, frontend,
odbiór staging i proces uprawnień. Nie ma publicznego wdrożenia ani automatycznego kasowania bazy.

## Weryfikacja lokalna — 18.09.2026

- Release build: bez błędów i ostrzeżeń.
- 126/126 testów, w tym prawdziwy SQL Server w odrębnych bazach
  SupportTicketManagerTests_*; testy usuwają wyłącznie własną bazę po wykonaniu.
- Kontrola EF: brak różnicy między modelem a ostatnią migracją.
- NuGet --vulnerable --include-transitive: brak znanych podatności według użytego źródła.
- SMTP w testach zastąpione transportem pamięciowym; nie wysyłano prawdziwych e-maili.
- Nie zastosowano migracji do bazy aplikacji, nie uruchomiono publicznego hostingu,
  nie wykonano testu staging/TLS w przeglądarce ani obciążeniowego.
- Zmiany lokalne, bez commita/pusha i bez zmiany sekretów.

## Dokumentacja źródłowa

- [Antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0)
- [Rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0)
- [Identity i e-mail](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-10.0)
- [Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0)
