# hk_models – HK:s ML-modeller (ML.NET)

Isolerat C#-projekt som tränar, utvärderar och **sparar** BDI-modellerna från
["HK Eda"-notebooken](../../eda/HK%20Eda.ipynb). Sparade modellfiler (.zip)
kan sedan laddas av backend-tjänsten och användas från frontend – utan omträning.

Projektet är **medvetet separerat** från webbprojektet i backend-roten
(`backend.csproj`, namespace `mlstack`): eget csproj, eget namespace
(`HKModels`), alla klasser prefixade med `HK`. Inga filer i roten
behövs eller ändras av det här projektet.

## Kom igång

```bash
cd backend/hk_models
dotnet run
```

Programmet tränar en modell per kön och målkolumn på `../screen_time_mental_health.csv`
(4 810 rader), skriver ut RMSE/MAE/R² per modell och sparar:

| Fil | Innehåll |
|---|---|
| `Modeller/hk_modell_flickor.zip` | BDI-modell tränad på flickorna (~2 364 rader) |
| `Modeller/hk_modell_pojkar.zip` | BDI-modell tränad på pojkarna (~2 446 rader) |
| `Modeller/hk_modell_somn_flickor.zip` | Sömnkvalitetsmodell (flickor) – används av `/api/somnpredict` |
| `Modeller/hk_modell_somn_pojkar.zip` | Sömnkvalitetsmodell (pojkar) – används av `/api/somnpredict` |

Förväntade värden (samma modelltyp och uppdelning som i notebooken):
RMSE ≈ 7,8–8,2 (flickor) respektive ≈ 4,9–5,4 (pojkar), R² ≈ 0,12–0,16.
Modellen förklarar alltså bara ca 12–16 % av variationen i BDI – se
notebookens "Tolkning av utvärderingen" för varför.

## Filstruktur

| Fil | Vad den gör |
|---|---|
| `Program.cs` | Startpunkt: tränar båda modellerna, sparar zip-filer, gör en exempelprediktion mot varje sparad fil |
| `HKRad.cs` | En rad i CSV-filen; `[LoadColumn]`-attribut kopplar egenskaper till kolumner |
| `HKTränare.cs` | Pipeline: filtrera kön → 80/20-uppdelning → linjär regression (SDCA) → utvärdera → spara |
| `HKPrediktor.cs` | Laddar en sparad .zip och predikterar BDI för en person. **Detta är ingången för frontend-användning** |
| `HKPrediktion.cs` | Resultattyper: predikterad poäng + utvärderingsnyckeltal |

## Hur frontend använder modellen

**Sömnkvalitetsmodellerna är redan kopplade:** backend-filen `HKSomnPrediktor.cs`
(backend-roten) exponerar `POST /api/somnpredict` som tar emot
`{ leisureScreenHours, sleepHours, weekendMidsleep, socialJetlag, kon }`
(där `kon` är `"flicka"` eller `"pojke"`) och svarar med
`{ kon, sleepQuality }` (lägre värde = bättre sömn). Frontend-sidan är
`src/pages/SomnPredictor.tsx` (route `/somnpredictor`).

Obs: `screen_time_index` frågas inte ut i formuläret – det är ett sammanvägt
betyg (1–6) i datasetet som användaren inte kan svara på. Endpointen härleder
det från fritidstimmarna via sambandet i datan (index ≈ 0,402 × timmar + 1,628,
korrelation 0,94).

För BDI-modellerna: webbprojektet refererar det här projektet via `ProjectReference`
i `backend.csproj`, så `HKModels.HKPrediktor` kan användas direkt i webbkod. Exempel
på en endpoint i webbprojektet (gör det i EN egen fil, inte i andras):

```csharp
// Ladda vid start (sökvägen pekar mot hk_models/Modeller)
var flickor = HKModels.HKPrediktor.Ladda("hk_models/Modeller/hk_modell_flickor.zip");

// I en endpoint: ta emot elevens värden som JSON, returnera predikterad BDI
app.MapPost("/api/hk/bdi/flickor", (mlstack.SleepInput e) =>
    Results.Ok(new { predikteradBdi = flickor.Predikera(new HKModels.HKRad
    {
        screen_time_index = e.screen_time_index,
        // ...övriga värden från request-bodyn
    }) }));
```

`HKPrediktor`/`PredictionEngine` är inte trådsäker – i en webbtjänst,
skapa en prediktion per anrop eller använd ML.NET:s `PredictionEnginePool`.

## Bra att veta

- **BDI** (`bdi_total`) är ett självrapporterat frågeformulärsvärde
  (Beck Depression Inventory), inte uträknat från övriga kolumner.
- Kolumnen `depressed` används inte – den hänger ihop med BDI och skulle
  läcka målet in i modellen.
- Träningen har fast slumpfrö (42) och blir därmed reproducerbar.
- När målkolumnen är `sleep_quality_index` utesluts den automatiskt ur
  feature-listan i tränaren – annars skulle modellen "fuska" (dataläckage).
- Vill du träna på annan data: `dotnet run -- <sökväg-till-csv>`.
