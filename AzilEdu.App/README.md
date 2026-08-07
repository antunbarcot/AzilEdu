# AzilEdu

Aplikacija za upravljanje azilom za životinje — evidencija životinja, volontera, donatora i djelatnika, s AI-potpomognutim funkcijama za opise, sažetke i provjeru unosa.

## Pokretanje projekta

Rješenje sadrži dva projekta koja se pokreću zajedno:

- **AzilEdu.Api** — ASP.NET Core Web API, SQLite baza, JWT autentikacija, AI servisi
- **AzilEdu.App** — Blazor Server aplikacija (korisničko sučelje)

### Koraci

1. Otvori `AzilEdu.slnx` u Visual Studiju.
2. Provjeri da je postavljen zajednički profil pokretanja (Multiple startup projects: AzilEdu.Api i AzilEdu.App).
3. Pokreni rješenje (F5 ili Ctrl+F5). Oba projekta se pokreću istovremeno.
4. Pri prvom pokretanju API automatski primjenjuje sve migracije i puni bazu početnim podacima (demo životinje i demo korisnici).
5. App je dostupan na `https://localhost:7240` (ili prema vlastitim postavkama), API na `https://localhost:7205`.

### Ručna provjera migracija

U Package Manager Console, uz odabran `AzilEdu.Api` kao startup projekt: Update-Database
Baza se nalazi u datoteci `AzilEdu.db` unutar `AzilEdu.Api` mape.

## Demo računi

| Uloga | Email | Što vidi |
|---|---|---|
| Administrator | admin@aziledu.local | sve module, dashboard, korisnike i AI funkcije |
| Djelatnik | employee@aziledu.local | životinje, multimediju, zadatke, donatore, donacije i AI pomoć |
| Volonter | volunteer@aziledu.local | svoje volonterske zadatke i njihov AI sažetak |
| Donator | donor@aziledu.local | svoje donacije i prikaz životinja za udomljavanje |

Lozinke demo računa nisu navedene u ovoj dokumentaciji; postavljaju se lokalno pri seedanju baze i ne dijele se u repozitoriju.

## Relacije korisničkog identiteta

- **AppUser–AppRole** — veza mnogo-na-mnogo preko povezne tablice `AppUserRole`. Jedan korisnički račun može imati više uloga istovremeno (npr. Employee i Donor na istom računu).
- **AppUser–Volunteer** — jedan-na-jedan (opcionalno), preko `VolunteerId` na `AppUser`. Povezuje račun s volonterskim profilom i njegovim zadacima.
- **AppUser–Donor** — jedan-na-jedan (opcionalno), preko `DonorId` na `AppUser`. Povezuje račun s donatorskim profilom i njegovim donacijama.
- **AppUser–Employee** — jedan-na-jedan (opcionalno), preko `EmployeeId` na `AppUser`. Povezuje račun s profilom djelatnika.

`AppUser` ne zamjenjuje poslovne entitete (Volunteer, Donor, Employee) — račun čuva podatke za prijavu i ovlasti, a odvojene tablice čuvaju poslovne podatke, čime se izbjegava dupliciranje.

## Razlika između 401 i 403

- **401 Unauthorized** — identitet nije potvrđen: token nedostaje, neispravan je ili je istekao. API ne zna tko šalje zahtjev.
- **403 Forbidden** — identitet je potvrđen (token je valjan), ali prijavljeni korisnik nema traženu ulogu ili vezani poslovni profil za tu akciju.

## AI endpointi i podaci koji se šalju provideru

Svi AI pozivi idu isključivo kroz API (`AiController`); Blazor App nikad ne komunicira s AI providerom izravno.

| Endpoint | Svrha | Podaci poslani provideru |
|---|---|---|
| `POST api/ai/text` (purpose: animal-adoption) | Opis životinje za udomljavanje | Ime, vrsta, pasmina, spol, starost, postojeći opis |
| `POST api/ai/text` (purpose: donor-thank-you) | Zahvala donatoru | Odabrani ton, ime donatora, tip donacije, vrijednost/sadržaj, datum |
| `POST api/ai/text` (purpose: social-post) | Objava za društvene mreže | Ime, vrsta, pasmina, status i opis životinje |
| `GET api/ai/daily-summary` | Dnevni operativni sažetak | Agregirani brojevi: ukupno životinja, dostupne za udomljenje, otvoreni i zakašnjeli zadaci, donacije u zadnjih 7 dana |
| `GET api/ai/volunteer-summary/mine` | Sažetak volonterovih zadataka | Naslov, tip, povezana životinja, status i rok za do 10 otvorenih zadataka prijavljenog volontera |
| `POST api/ai/animal-intake` | Pametni unos životinje iz slobodnog teksta | Tekst bilješke koju unese djelatnik |
| `POST api/ai/animal-data-check` | Provjera kvalitete podataka prije spremanja | Ime, vrsta, pasmina, spol, starost, datum dolaska, status, opis (bez internih ID-eva baze) |

Lozinke, hash lozinki i API ključevi nikad se ne šalju AI servisu. Osobni podaci donatora i volontera svode se na minimum potreban za konkretan zadatak.

## Prebacivanje između Mock i OpenAI načina

Zadani način rada je `Mock` (u `appsettings.json`), pa aplikacija radi i bez API ključa. Za uključivanje stvarnog OpenAI providera, ključ se **nikad** ne upisuje u `appsettings.json` niti se commita u Git — koristi se user secrets:
cd AzilEdu.Api
dotnet user-secrets init
dotnet user-secrets set "Ai:Provider" "OpenAI"
dotnet user-secrets set "Ai:ApiKey" "OVDJE-IDE-LOKALNI-KLJUC"
dotnet user-secrets set "Ai:Model" "gpt-5.6-luna"

Za povratak na lokalni Mock način:
dotnet user-secrets set "Ai:Provider" "Mock"
Nakon promjene providera potrebno je ponovno pokrenuti API. Aktivni provider i model prikazani su na dashboardu.

## Poznata ograničenja

- Potpuni rollback svih migracija naredbom `Update-Database 0` na SQLite bazi trenutno ne prolazi do kraja zbog FK-rebuild mehanizma migracije `AddAnimalStatusRelation` (SQLite ne dopušta isključivanje provjere stranih ključeva unutar transakcije). Čisto stanje baze umjesto toga se postiže brisanjem `.db` datoteke prije pokretanja `Update-Database`.
- Broj AI poziva po korisniku trenutno nije ograničen (rate limiting), što u produkcijskom okruženju s vanjskim providerom može dovesti do neočekivanog troška.

## Prijedlozi za sljedeću verziju

1. Uvesti ograničenje broja AI poziva po korisniku i po vremenskom razdoblju (rate limiting), uz vidljiv broj preostalih poziva u sučelju.
2. Razdvojiti migraciju `AddAnimalStatusRelation` na dvije zasebne migracije kako bi PRAGMA naredba za isključivanje provjere stranih ključeva mogla ispravno raditi izvan transakcije, omogućujući potpuni rollback baze na SQLite-u.