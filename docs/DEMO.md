# Publiczne demo TSM

Demo działa pod adresem [tsm.szymon-michalek.dev](https://tsm.szymon-michalek.dev).
To środowisko prezentacyjne na fikcyjnych danych, nie system do obsługi
rzeczywistych spraw. Konta i zgłoszenia są wspólne dla wszystkich odwiedzających.
Nie wpisuj danych osobowych ani poufnych informacji.

## Konta i ograniczenia

Publiczne dane logowania są również widoczne w interfejsie:

| Rola | E-mail | Hasło demonstracyjne |
| --- | --- | --- |
| Użytkownik | user@demo.example | Demo-User!2026 |
| Support | support@demo.example | Demo-Support!2026 |

Nie używaj tych haseł do prawdziwych kont, SQL, SSH ani certyfikatów.
Demo nie pozwala na rejestrację, potwierdzanie adresu, odzyskiwanie haseł
ani wysyłkę e-maili. API dopuszcza w obszarze auth tylko csrf, login, logout i me.
Ukrycie formularzy nie jest jedyną blokadą: wyłączone operacje zwracają 403.

Konta są potwierdzone podczas inicjalizacji. Blokada konta po błędnych hasłach
jest wyłączona tylko dla wspólnych kont demo; limity żądań i CSRF pozostają.
Wylogowanie usuwa cookie bieżącego klienta bez zmiany security stamp całego konta.
Inni odwiedzający pozostają zalogowani. Limit danych to 500 zgłoszeń;
po osiągnięciu limitu tworzenie zwraca 429. Count i insert są w jednej transakcji
z blokadą tabeli, aby równoległe żądania nie przekroczyły limitu.

## Kontenery i połączenia

Pliki wdrożenia znajdują się w `deploy/`. Na VPS kod jest w `/opt/tsm-demo`,
a polecenia Compose wykonuje się w `/opt/tsm-demo/deploy`.

- `web`: frontend zbudowany z VITE_TSM_DEMO=true, Caddy i automatyczny HTTPS.
- `api`: ASP.NET Core w Production, Demo__Enabled=true, proces aplikacji UID 1654.
- `sql`: SQL Server 2022 Express, UID 10001, osobna baza SupportTicketManagerDemo.

Tylko web publikuje porty 80 i 443. Porty API i SQL nie są publikowane na hoście.
Frontend i /api korzystają z jednego originu. API ufa nagłówkom przekazanym
wyłącznie przez skonfigurowane IP Caddy. API łączy się z SQL przez TLS,
z weryfikacją nazwy `sql` i certyfikatu podpisanego prywatnym CA.
Nie ustawiaj TrustServerCertificate=True ani publicznego portu 1433.

Compose używa stałych adresów sieciowych. Nie uruchamiaj drugiej instancji
usługi api przez `compose run`, gdy podstawowy kontener już działa — zajęłaby
ten sam adres IP. Reset wykonuje się przez `compose exec`.

## Sekrety i uprawnienia

Poza Git pozostają `deploy/.env` i `deploy/private/`. Plik .env zawiera
SQL_SA_PASSWORD, TSM_DB_PASSWORD, DP_CERT_PASSWORD oraz ACME_EMAIL.
Hasła infrastruktury są losowe i inne niż publiczne hasła demonstracyjne.
Nie pokazuj wyniku `docker compose config` bez --quiet: może zawierać sekrety.

Pliki prywatne obejmują wewnętrzne CA, certyfikat i klucz SQL oraz PFX Data Protection.
Plik SQL ma SAN DNS:sql. CA służy połączeniu wewnętrznemu; certyfikat strony
internetowej jest osobnym certyfikatem zarządzanym przez Caddy.

Docelowe prawa: `.env` 600, katalog `private` 700, sql.key 600 dla UID 10001,
data-protection.pfx 400 dla UID 1654. Publiczne ca.crt, sql.crt i mssql.conf
muszą być czytelne dla odpowiednich kontenerów. Nie rozwiązuj problemów przez chmod 777.
W Dockerfile API pliki publikacji są kopiowane z --chown=1654:1654.

API nie korzysta z sa. Podczas pierwszej inicjalizacji login tsm_demo miał
tymczasowo db_owner wyłącznie w osobnej bazie demo. Po migracjach rolę odebrano.
Runtime ma SELECT na dbo, INSERT/UPDATE/DELETE na Tickets i UPDATE na AspNetUsers
potrzebne Identity. Kolejne migracje wymagają osobnego kontrolowanego nadania praw;
nie przywracaj db_owner jako stałej konfiguracji.

## Reset danych

Polecenie initialize tworzy schemat, konta i dane tylko w dedykowanej bazie.
Reset wymaga Demo:Enabled, dokładnej nazwy bazy oraz oczekiwanego stanu dwóch kont.
Nie zmienia ich haseł i nie usuwa kont. Zastępuje zgłoszenia 12 przykładami
w transakcji; ID nie są zerowane, więc otwarte stare szczegóły mogą zwrócić 404.
Nie uruchamiaj initialize w ramach zwykłej obsługi działającego demo.

Na wdrożonym serwerze jednostki `deploy/tsm-demo-reset.service` i `.timer`
są zainstalowane w `/etc/systemd/system/`. Timer jest włączony, uruchamia reset
codziennie o 04:00 UTC z losowym opóźnieniem do 60 sekund. Persistent=true
umożliwia wykonanie zaległego zadania po ponownym uruchomieniu timera.
Usługa wymaga działającego kontenera api i nie uruchamia go sama.

Kontrola harmonogramu i wyniku:

```sh
sudo systemctl list-timers tsm-demo-reset.timer --no-pager
sudo systemctl status tsm-demo-reset.service --no-pager -l
sudo journalctl -u tsm-demo-reset.service -n 20 --no-pager
```

Ręczne wykonanie poniższego polecenia zastąpi bieżące zgłoszenia demo przykładami:

```sh
sudo systemctl start tsm-demo-reset.service
```

Sukces to status=0/SUCCESS oraz komunikat o przywróceniu 12 fikcyjnych zgłoszeń.
Po zakończeniu usługa jednorazowa ma stan inactive (dead); timer pozostaje aktywny.

## Obsługa działającego wdrożenia

```sh
cd /opt/tsm-demo/deploy
sudo docker compose --env-file .env config --quiet
sudo docker compose --env-file .env ps
sudo docker compose --env-file .env logs --tail=30 api web
```

Pliki .env i private muszą pozostać na miejscu podczas aktualizacji kodu.
Nie przesyłaj ich ze zwykłą paczką źródeł. Nie wykonuj `down -v` ani prune
wolumenów: baza, klucze API i certyfikaty Caddy są trwałymi danymi.
Zmiana kodu wymaga buildu i odtworzenia odpowiedniego kontenera, nie samego restartu.
Przed zmianą schematu wykonaj kopię i zaplanuj migrację oraz rollback.

## Kopia zapasowa i utrzymanie

22.10.2026 wykonano backup SQL COPY_ONLY z CHECKSUM oraz RESTORE VERIFYONLY.
Kopię pobrano poza VPS; identyczność pliku potwierdzono SHA256.
Nie wykonano jeszcze pełnego testu odtworzenia do odrębnej bazy.
Backup SQL nie obejmuje .env, kluczy CA, PFX, key ring ani magazynu Caddy;
one wymagają osobnej zabezpieczonej kopii i planu odzyskania.

Reset danych nie jest backupem. Harmonogram backupów, retencja i alerty nie są
jeszcze zautomatyzowane. Sprawdzaj wynik resetu, wolne miejsce, logi, aktualizacje
obrazów i zależności. Obrazy mają ruchome tagi; aktualizacja wymaga review i regresji.
Certyfikat SQL wystawiono na rok, PFX na dwa lata, CA na dziesięć lat.
Caddy odnawia publiczny HTTPS; certyfikaty wewnętrzne wymagają własnej kontroli
terminów i zaplanowanej rotacji. Nie podmieniaj PFX bez planu dla istniejących kluczy.

## Weryfikacja wdrożenia

Potwierdzono HTTPS i /api/status, logowanie obu ról, operacje Support i trwałość
zmian po F5, ręczny reset przez systemd oraz obecność aktywnego timera.
Rejestracja i cztery operacje e-mailowe zwróciły 403 w publicznym API.
Automatyczne testy obejmują też zachowanie dwóch sesji wspólnego konta,
CSRF oraz granicę 499/500 zgłoszeń w izolowanych bazach testowych.
Nie jest to pełny audyt bezpieczeństwa, test obciążeniowy ani gwarancja dostępności.
