# EnergyRate

**Energetinių gėrimų katalogo ir atsiliepimų REST API sistema**

## 1. Sprendžiamo uždavinio aprašymas

### 1.1. Sistemos paskirtis

„EnergyRate“ – sistema, skirta energetinių gėrimų mėgėjams dalytis nuomone apie gėrimus, atrasti naujus produktus ir susipažinti su kitų naudotojų patirtimi.

Sistema leidžia naudotojams:

- Peržiūrėti gamintojų katalogą ir jų produktus.
- Skaityti kitų naudotojų atsiliepimus.
- Kurti, redaguoti ir šalinti savo atsiliepimus.
- Palyginti gėrimus pagal jų įvertinimus.

Administratoriai gali valdyti gamintojų bei gėrimų katalogą ir šalinti netinkamą turinį. Sistema taip pat apskaičiuoja vidutinį gėrimo įvertinimą pagal naudotojų atsiliepimus.

### 1.2. Taikomosios srities hierarchija

```text
Gamintojas
    └── Gėrimas
          └── Atsiliepimas
```

### 1.3. Naudotojų rolės

| Rolė | Galimybės |
|---|---|
| Svečias (Guest) | Peržiūrėti gamintojus, gėrimus ir atsiliepimus. Privalo būti prisijungęs. |
| Narys (Member) | Atlikti svečio veiksmus, kurti, redaguoti ir šalinti savo atsiliepimus. |
| Administratorius (Admin) | Valdyti gamintojus ir gėrimus, peržiūrėti turinį ir šalinti bet kurio naudotojo atsiliepimus. |

Visos trys rolės yra autentifikuoti naudotojai. Neprisijungęs naudotojas negali pasiekti apsaugotų API endpointų.

### 1.4. Funkciniai reikalavimai

1. Sistema leidžia naudotojams registruotis ir prisijungti.
2. Sistema palaiko tris naudotojų roles: Guest, Member ir Admin.
3. Administratorius gali kurti, peržiūrėti, redaguoti ir šalinti gamintojus.
4. Administratorius gali valdyti konkretaus gamintojo gėrimus.
5. Prisijungęs naudotojas gali peržiūrėti gamintojų, gėrimų ir atsiliepimų sąrašus bei atskirus įrašus.
6. Narys gali palikti gėrimo atsiliepimą su įvertinimu nuo 1 iki 5 ir komentaru.
7. Narys gali redaguoti ir šalinti tik savo atsiliepimus.
8. Administratorius gali šalinti bet kurio naudotojo atsiliepimus.
9. Vienas narys gali palikti tik vieną atsiliepimą apie tą patį gėrimą.
10. Sistema apskaičiuoja gėrimo vidutinį įvertinimą.
11. Tikrinamas įvestų duomenų teisingumas: įvertinimas turi būti nuo 1 iki 5, kofeino kiekis negali būti neigiamas, o privalomi laukai turi būti užpildyti.
12. Palaikomi hierarchiniai metodai, leidžiantys gauti konkretaus gamintojo gėrimus ir konkretaus gėrimo atsiliepimus.
13. Užtikrinama, kad prie apsaugotų endpointų negalėtų prieiti neprisijungę arba atitinkamų teisių neturintys naudotojai.

## 2. Pasirinktos technologijos

| Technologija | Paskirtis ir pasirinkimo priežastis |
|---|---|
| **ASP.NET Core Web API (.NET 10, C#)** | REST API kūrimui, maršrutizavimui ir autentifikavimo bei autorizacijos mechanizmų integracijai. |
| **PostgreSQL** | Atvirojo kodo reliacinė duomenų bazė sistemos duomenims saugoti. |
| **Entity Framework Core** | Darbui su duomenų baze per C# objektus ir duomenų bazės migracijoms valdyti. |
| **JWT** | Žetonais pagrįstai naudotojų autentifikacijai ir autorizacijai. |
| **BCrypt.Net-Next** | Slaptažodžių maišoms generuoti ir slaptažodžiams saugiai tikrinti. |
| **Postman** | API metodų testavimui, HTTP užklausų siuntimui ir automatizuotiems testų rinkiniams vykdyti. |

## 3. Sistemos architektūra

Pagrindiniai sistemos komponentai:

- **Backend:** ASP.NET Core Web API.
- **Duomenų bazė:** PostgreSQL.
- **ORM:** Entity Framework Core.
- **Autentifikacija ir autorizacija:** JWT, rolėmis ir naudotojo ID pagrįsta prieigos kontrolė.
- **Slaptažodžių apsauga:** BCrypt.Net-Next.
- **Testavimas:** Postman.

Sistema sukurta taikant REST architektūros principus. API priima HTTP užklausas, apdoroja duomenis ir grąžina JSON formato atsakymus.

## 4. Autentifikacija ir autorizacija

### 4.1. JWT autentifikacija

Naudotojų autentifikacija įgyvendinta naudojant JWT (JSON Web Token).

Prisijungusiam naudotojui išduodami du žetonai:

- **Access Token** – trumpesnės galiojimo trukmės žetonas, naudojamas prieigai prie apsaugotų API endpointų.
- **Refresh Token** – ilgesnės galiojimo trukmės žetonas, naudojamas naujam Access Token gauti.

Access Token saugoma naudotojo identifikavimo informacija ir jo rolė. Tai leidžia API patikrinti, kas siunčia užklausą ir kokius veiksmus jam leidžiama atlikti.

Refresh Token duomenų bazėje saugomas ne atviru tekstu, o jo SHA-256 maišos reikšme. Atnaujinant žetonus naudojama jų rotacija – panaudotas Refresh Token pakeičiamas nauju.

### 4.2. Atsijungimas ir žetonų panaikinimas

Atsijungimo metu panaikinamas naudotojo Refresh Token ir atnaujinama jo `TokenVersion` reikšmė.

Access Token tikrinimo metu ši reikšmė palyginama su duomenų bazėje saugoma reikšme. Jei jos nesutampa, žetonas laikomas negaliojančiu.

Tokiu būdu atsijungus panaikinamas ne tik Refresh Token galiojimas, bet ir visi anksčiau išduoti to naudotojo Access Token.

### 4.3. Prieigos kontrolė

API naudoja rolėmis pagrįstą autorizaciją. Neprisijungusiam naudotojui grąžinamas `401 Unauthorized`, o prisijungusiam naudotojui, neturinčiam reikiamų teisių, – `403 Forbidden`.

| Veiksmas | Guest | Member | Admin |
|---|:---:|:---:|:---:|
| Peržiūrėti gamintojus, gėrimus ir atsiliepimus | ✓ | ✓ | ✓ |
| Kurti gamintojus | ✗ | ✗ | ✓ |
| Redaguoti gamintojus | ✗ | ✗ | ✓ |
| Šalinti gamintojus | ✗ | ✗ | ✓ |
| Kurti gėrimus | ✗ | ✗ | ✓ |
| Redaguoti gėrimus | ✗ | ✗ | ✓ |
| Šalinti gėrimus | ✗ | ✗ | ✓ |
| Kurti atsiliepimus | ✗ | ✓ | ✗ |
| Redaguoti savo atsiliepimus | ✗ | ✓ | ✗ |
| Šalinti savo atsiliepimus | ✗ | ✓ | ✓ |
| Šalinti kitų naudotojų atsiliepimus | ✗ | ✗ | ✓ |

Narys gali keisti arba šalinti tik jam priklausančius atsiliepimus. Administratorius gali šalinti visų naudotojų atsiliepimus.

## 5. Pagrindiniai API endpointai

### 5.1. Autentifikacija

| Metodas | Endpointas | Paskirtis |
|---|---|---|
| `POST` | `/api/auth/register` | Naujo naudotojo registracija |
| `POST` | `/api/auth/login` | Prisijungimas |
| `POST` | `/api/auth/refresh` | Access Token atnaujinimas |
| `POST` | `/api/auth/logout` | Atsijungimas ir žetonų panaikinimas |

Registruojantis sukuriamas Member rolės naudotojas. Kitos rolės priskiriamos atskirai.

### 5.2. Gamintojai

| Metodas | Endpointas | Paskirtis |
|---|---|---|
| `GET` | `/api/brands` | Gamintojų sąrašas |
| `GET` | `/api/brands/{id}` | Konkretaus gamintojo informacija |
| `POST` | `/api/brands` | Gamintojo sukūrimas |
| `PUT` | `/api/brands/{id}` | Gamintojo redagavimas |
| `DELETE` | `/api/brands/{id}` | Gamintojo pašalinimas |

### 5.3. Gėrimai

| Metodas | Endpointas | Paskirtis |
|---|---|---|
| `GET` | `/api/brands/{brandId}/drinks` | Gamintojo gėrimų sąrašas |
| `GET` | `/api/brands/{brandId}/drinks/{drinkId}` | Konkretaus gėrimo informacija |
| `POST` | `/api/brands/{brandId}/drinks` | Gėrimo sukūrimas |
| `PUT` | `/api/brands/{brandId}/drinks/{drinkId}` | Gėrimo redagavimas |
| `DELETE` | `/api/brands/{brandId}/drinks/{drinkId}` | Gėrimo pašalinimas |

### 5.4. Atsiliepimai

| Metodas | Endpointas | Paskirtis |
|---|---|---|
| `GET` | `/api/brands/{brandId}/drinks/{drinkId}/reviews` | Gėrimo atsiliepimų sąrašas |
| `GET` | `/api/brands/{brandId}/drinks/{drinkId}/reviews/{reviewId}` | Konkretaus atsiliepimo informacija |
| `POST` | `/api/brands/{brandId}/drinks/{drinkId}/reviews` | Atsiliepimo sukūrimas |
| `PUT` | `/api/brands/{brandId}/drinks/{drinkId}/reviews/{reviewId}` | Atsiliepimo redagavimas |
| `DELETE` | `/api/brands/{brandId}/drinks/{drinkId}/reviews/{reviewId}` | Atsiliepimo pašalinimas |

Visiems GET endpointams taip pat reikalingas galiojantis Access Token.

## 6. Testavimas

API testuojamas naudojant Postman. Projekte parengtas automatizuotų testų rinkinys, apimantis autentifikaciją, autorizaciją ir pagrindines CRUD operacijas.

Testavimo metu tikrinama:

- Sėkminga ir nesėkminga registracija bei prisijungimas.
- Access Token ir Refresh Token išdavimas bei atnaujinimas.
- Refresh Token rotacija ir ankstesnio žetono negaliojimas.
- Atsijungimas ir abiejų žetonų panaikinimas.
- Prieigos kontrolė pagal naudotojo roles.
- Neprisijungusio naudotojo prieigos blokavimas (`401 Unauthorized`).
- Nepakankamas naudotojo teisių lygis (`403 Forbidden`).
- Gamintojų, gėrimų ir atsiliepimų kūrimas, redagavimas bei šalinimas.
- Atsiliepimų savininko tikrinimas.
- Neegzistuojančių išteklių apdorojimas (`404 Not Found`).
- Netinkamų arba pasikartojančių duomenų validacija (`400`, `409`, `422`, priklausomai nuo situacijos).

Testų rinkinyje naudojami atskiri Guest, Member ir Admin naudotojai. Kolekcijos užklausos vykdomos nustatyta tvarka naudojant Postman Collection Runner.

## 7. Paleidimas

### Reikalavimai

- Įdiegtas .NET 10 SDK.
- PostgreSQL duomenų bazė.
- Visual Studio 2022 su tinkamais .NET įrankiais arba kita suderinama kūrimo aplinka.
- Entity Framework Core CLI įrankis (`dotnet-ef`).

### Paleidimo veiksmai

1. Atsisiųsk arba klonuok projekto saugyklą.
2. Atidaryk `EnergyRate.sln` arba `EnergyRate.csproj`.
3. Sukurk PostgreSQL duomenų bazę.
4. Sukonfigūruok `ConnectionStrings:Default` prisijungimo eilutę naudodamas vietinę konfigūraciją arba User Secrets.
5. Sukonfigūruok JWT nustatymus, įskaitant slaptąjį raktą. Slaptojo rakto nereikėtų laikyti viešai prieinamame konfigūracijos faile.
6. Atkurk projekto priklausomybes:

   ```bash
   dotnet restore
   ```

7. Jei projekte yra Entity Framework Core migracijos, pritaikyk jas:

   ```bash
   dotnet ef database update
   ```

8. Paleisk projektą:

   ```bash
   dotnet run
   ```

9. Jei įjungta `Development` aplinka ir sukonfigūruotas Swagger, atidaryk programos vietinį `/swagger` adresą.
