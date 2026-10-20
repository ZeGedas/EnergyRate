1.	Sprendžiamo uždavinio aprašymas
1.1.	Sistemos paskirtis
„EnergyRate“ skirta energetinių gėrimų mėgėjams dalytis nuomone apie gėrimus, atrasti naujus gėrimus, sužinoti kitų pirkėjų nuomonę apie juos. Naudotojai gali peržiūrėti gamintojus ir jų produktus, skaityti kitų vartotojų atsiliepimus bei palikti savo atsiliepimus. Administratoriai prižiūri gamintojų ir gėrimų katalogą, šalina netinkamą turinį. Sistema automatiškai skaičiuoja kiekvieno gėrimo vidutinį įvertinimą, todėl lengva palyginti gėrimus tarpusavyje.
1.2.	Funkciniai reikalavimai
Taikomosios srities hierarchija:
Gamintojas -> Gėrimas -> Atsiliepimas

Rolės:
Rolė	Ką gali daryti
Svečias	Peržiūrėti gamintojų katalogą, kiekvieno gamintojo gėrimų katalogą, gėrimų atsiliepimus. Gali registruotis ir prisijungti.
Narys	Viskas, ką gali svečias. Gali kurti atsiliepimus apie gėrimą, redaguoti ir šalinti savo atsiliepimus, pakeisti įvertinimą.
Administratorius	Valdyti gamintojus ir gėrimus (kurti, redaguoti, šalinti), gali šalinti bet kurį atsiliepimą.

Funkciniai reikalavimai:
1.	Sistema leidžia registruotis ir prisijungti.
2.	Administratorius gali kurti, redaguoti, peržiūrėti, šalinti gamintojus.
3.	Administratorius gali kurti, redaguoti, šalinti gėrimus konkretaus gamintojo kategorijoje.
4.	Svečias gali peržiūrėti gamintojų, gėrimų ir atsiliepimų sąrašus bei pavienius įrašus.
5.	Narys gali sukurti atsiliepimą apie gėrimą (įvertinimas 1-5 ir komentaras).
6.	Narys gali redaguoti ir šalinti tik savo atsiliepimus.
7.	Vienas narys gali palikti tik vieną atsiliepimą apie tą patį gėrimą.
8.	Sistema automatiškai skaičiuoja ir rodo gėrimo vidutinį įvertinimą.
9.	Sistema tikrina duomenų teisingumą (įvertinimas 1-5, kofeino kiekis neneigiamas, privalomi laukai).
10.	Sistema leidžia gauti visus konkretaus gamintojo gėrimus ir visus konkretaus gėrimo atsiliepimus (hierarchiniai metodai).

2.	Pasirinktų technologijų aprašymas
	Technologija	Kodėl pasirinkta
Serverio dalis (backend)	ASP.NET Core Web API (.NET, C#)	Skirtas REST API kūrimui, turi įmontuotą maršrutizavimą, validaciją ir integraciją su JWT. Plačiai naudojamas darbo rinkoje.
Kliento dalis (frontend)	React + Vite	Komponentais grįsta biblioteka GUI kūrimui. Su REST API bendrauja per HTTP ir JSON, todėl gerai dera su .NET API. Vite užtikrina greitą kūrimą ir paprastą surinkimą.
Duomenų bazė	PostgreSQL	Nemokama reliacinė duomenų bazė.
Autentifikacija	JWT	Nereikalauja saugoti sesijos serveryje, todėl atitinka REST principus.
Testavimas	Postman	Leidžia patogiai testuoti kiekvieną API metodą, siųsti JWT žetoną ir išsaugoti užklausų rinkinius.
Debesų platforma	Microsoft Azure	Galima patalpinti visas dalis: React, API ir PostgreSQL. Gerai integruota su .NET.
Darbas su duomenų baze (ORM)	Entity Framework Core	Leidžia dirbti su duomenų baze per C# klases ir kurti schemą per migracijas.
