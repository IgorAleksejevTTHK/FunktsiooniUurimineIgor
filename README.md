# Funktsioonide uurimise veebirakendus

Valmis ASP.NET Core MVC projekt praktilise ülesande jaoks.

## Tehnoloogiad

- C# / .NET 8
- ASP.NET Core MVC
- Entity Framework Core 8
- SQLite
- Chart.js
- Razor Views

## Funktsionaalsus

Rakendus võimaldab:

1. Sisestada matemaatilise funktsiooni.
2. Leida nullkohad.
3. Arvutada tuletise sümboolselt.
4. Leida tuletise nullkohad ehk kriitilised punktid.
5. Määrata lihtsate funktsioonide määramispiirkonna.
6. Klassifitseerida kriitilisi punkte maksimumiks, miinimumiks või statsionaarseks punktiks.
7. Salvestada analüüsi SQLite andmebaasi EF Core kaudu.
8. Teha Create / Read / Update / Delete toiminguid.
9. Kuvada funktsiooni ja tuletise interaktiivse Chart.js graafikuna.
10. Muuta graafiku x-vahemikku ja sammu brauseris.

## Toetatud valemid

`+`, `-`, `*`, `/`, `^`, sulud, `x`, `sin()`, `cos()`, `tan()`, `sqrt()`, `ln()`, `log()`, `exp()`, `abs()`, `pi`, `e`.

Näited:

- `x^3 - 3*x`
- `x^2 - 4*x + 3`
- `sin(x)`
- `x^2 + 2*x - 8`

## Käivitamine Visual Studios

1. Ava `FunktsiooniUurimine.sln`.
2. Taasta NuGet paketid.
3. Käivita projekt HTTPS profiiliga.
4. Rakendus rakendab olemasolevad EF Core migratsioonid automaatselt.
5. Esimesel käivitamisel lisatakse kaks näidisfunktsiooni.

## EF Core käsud

Visual Studio Package Manager Console:

```powershell
Add-Migration InitialCreate
Update-Database
```

Projektis on algne migratsioon juba kaasas, seega tavaliselt piisab rakenduse käivitamisest.

## Näidisfunktsioonide oodatavad tulemused

### `x^3 - 3*x`

- Määramispiirkond: kõik reaalarvud
- Tuletis: `f'(x) = 3*x^2 - 3`
- Kriitilised punktid: `x = -1` ja `x = 1`
- Ekstreemumid: maksimum `(-1; 2)` ja miinimum `(1; -2)`

### `x^2 - 4*x + 3`

- Määramispiirkond: kõik reaalarvud
- Tuletis: `f'(x) = 2*x - 4`
- Kriitiline punkt: `x = 2`
- Ekstreemum: miinimum `(2; -1)`
- Nullkohad: `x = 1` ja `x = 3`

## Matemaatiline klaviatuur
Create ja Edit lehel on interaktiivne matemaatiline klaviatuur. Nupud sisestavad parseri poolt toetatud sümboleid ja funktsioone, näiteks `x`, `pi`, `sin()`, `sqrt()`, `^`, `^2`, sulud ning tehted. Klaviatuur säilitab kursori ja valiku asukoha, toetab Backspace/Clear tegevusi ning valitud avaldist saab funktsiooni sisse mähkida.
