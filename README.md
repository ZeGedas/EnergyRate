# EnergyRate

**Energetinių gėrimų katalogo ir atsiliepimų REST API sistema**

## 1. Sprendžiamo uždavinio aprašymas

### 1.1. Sistemos paskirtis

„EnergyRate“ – sistema, skirta energetinių gėrimų mėgėjams dalytis nuomone apie gėrimus, atrasti naujus produktus ir susipažinti su kitų vartotojų patirtimi.

Sistema leidžia naudotojams:

- Peržiūrėti gamintojų katalogą ir jų produktus.
- Skaityti kitų vartotojų atsiliepimus.
- Kurti, redaguoti ir šalinti savo atsiliepimus.
- Palyginti gėrimus pagal jų įvertinimus.

Administratoriai gali valdyti gamintojų bei gėrimų katalogą ir šalinti netinkamą turinį. Sistema taip pat numato automatinį vidutinio gėrimo įvertinimo apskaičiavimą.

### 1.2. Taikomosios srities hierarchija

```text
Gamintojas
    └── Gėrimas
          └── Atsiliepimas
```

### 1.3. Naudotojų rolės

| Rolė | Galimybės |
|---|---|
| Svečias | Peržiūrėti gamintojus, gėrimus ir atsiliepimus, registruotis bei prisijungti. |
| Narys | Atlikti svečio veiksmus, kurti, redaguoti ir šalinti savo atsiliepimus. |
| Administratorius | Valdyti gamintojus ir gėrimus, šalinti bet kurio naudotojo atsiliepimus. |

### 1.4. Funkciniai reikalavimai

1. Sistema leidžia naudotojams registruotis ir prisijungti.
2. Administratorius gali kurti, peržiūrėti, redaguoti ir šalinti gamintojus.
3. Administratorius gali valdyti konkretaus gamintojo gėrimus.
4. Svečias gali peržiūrėti gamintojų, gėrimų ir atsiliepimų sąrašus bei atskirus įrašus.
5. Narys gali palikti gėrimo atsiliepimą su įvertinimu nuo 1 iki 5 ir komentaru.
6. Narys gali redaguoti ir šalinti tik savo atsiliepimus.
7. Vienas narys gali palikti tik vieną atsiliepimą apie tą patį gėrimą.
8. Sistema apskaičiuoja gėrimo vidutinį įvertinimą.
9. Tikrinamas įvestų duomenų teisingumas: įvertinimas turi būti nuo 1 iki 5, kofeino kiekis negali būti neigiamas, o privalomi laukai turi būti užpildyti.
10. Palaikomi hierarchiniai metodai, leidžiantys gauti konkretaus gamintojo gėrimus ir konkretaus gėrimo atsiliepimus.

## 2. Pasirinktos technologijos

| Technologija | Paskirtis ir pasirinkimo priežastis |
|---|---|
| **ASP.NET Core Web API (.NET, C#)** | REST API kūrimui, maršrutizavimui ir integracijai su autentifikavimo mechanizmais. |
| **React + Vite** | Komponentais grįstai naudotojo sąsajai kurti ir patogiam bendravimui su REST API per HTTP ir JSON. |
| **PostgreSQL** | Atvirojo kodo reliacinė duomenų bazė. |
| **JWT** | Žetonais pagrįstai naudotojų autentifikacijai. |
| **Postman** | API metodų testavimui, HTTP užklausų siuntimui ir testų rinkiniams saugoti. |
| **Microsoft Azure** | Galima debesijos platforma sistemos komponentams talpinti. |
| **Entity Framework Core** | Darbui su duomenų baze per C# objektus ir duomenų bazės migracijoms valdyti. |

## 3. Sistemos architektūra

Pagrindiniai numatyti sistemos komponentai:

- **Frontend:** React + Vite naudotojo sąsaja.
- **Backend:** ASP.NET Core Web API.
- **Duomenų bazė:** PostgreSQL.
- **ORM:** Entity Framework Core.
- **Autentifikacija:** JWT.
- **Testavimas:** Postman.

## 4. Pagrindiniai API endpointai

| Resursas | Endpointas |
|---|---|
| Gamintojai | `GET /api/brands` |
| Konkretaus gamintojo informacija | `GET /api/brands/{id}` |
| Gamintojo gėrimai | `GET /api/brands/{brandId}/drinks` |
| Konkretaus gėrimo informacija | `GET /api/brands/{brandId}/drinks/{drinkId}` |
| Gėrimo atsiliepimai | `GET /api/brands/{brandId}/drinks/{drinkId}/reviews` |

Šie endpointai pateikia pagrindinių hierarchinių išteklių pavyzdžius. Kitos CRUD operacijos atliekamos naudojant atitinkamus `POST`, `PUT` ir `DELETE` metodus.

## 5. Testavimas

API metodai testuojami naudojant Postman.

Testavimo metu tikrinama:

- Sėkmingų užklausų vykdymas.
- Duomenų kūrimas, atnaujinimas ir šalinimas.
- Neegzistuojančių išteklių apdorojimas (`404 Not Found`).
- Netinkamų duomenų validacija.
- Tinkami HTTP atsakymų kodai.

## 6. Paleidimas

### Reikalavimai

- Įdiegtas su projektu suderinamas .NET SDK.
- PostgreSQL duomenų bazė.
- Visual Studio 2022 arba kita suderinama kūrimo aplinka.

### Paleidimo veiksmai

1. Atsisiųsk arba klonuok projekto saugyklą.
2. Atidaryk `EnergyRate.sln` arba `EnergyRate.csproj`.
3. Sukurk PostgreSQL duomenų bazę.
4. Sukonfigūruok `ConnectionStrings:Default` prisijungimo eilutę naudodamas vietinę konfigūraciją arba User Secrets.
5. Atkurk priklausomybes:

   ```bash
   dotnet restore
   ```

6. Jei projekte yra Entity Framework Core migracijos, pritaikyk jas:

   ```bash
   dotnet ef database update
   ```

7. Paleisk projektą:

   ```bash
   dotnet run
   ```

8. Jei įjungta `Development` aplinka ir sukonfigūruotas Swagger, atidaryk programos vietinį `/swagger` adresą.


