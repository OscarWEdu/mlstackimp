# hk_models – HK:s ML-modeller (ML.NET)

Kod som tränar, utvärderar och **sparar** BDI- och sömnkvalitetsmodellerna från
["HK Eda"-notebooken](../../eda/HK%20Eda.ipynb) och
["SömnkvalitetsEda"-notebooken](../../eda/S%C3%B6mnkvalitetsEda.ipynb).

Mappen är en vanlig kodbas-mapp **inom webbprojektet** (ingen egen csproj –
beslutat i teamet): alla .cs-filer kompileras in i backend och har eget
namespace (`HKModels`) med klasser prefixade `HK`, så de krockar inte med
`mlstack`-filerna i backend-roten.

## Hur det fungerar

Sökvägarna sätts i `HKSomnPrediktor.cs` vid backend-start:

- **Modellmapp:** `backend/hk_models/Modeller/` (hittas via programmappen, oavsett
  varifrån backend startas)
- **Dataset:** `backend/screen_time_mental_health.csv` (4 810 rader)

Vid backend-start går `TränaModeller.SäkerställAlla` igenom alla fyra modeller
och tränar bara de vars zip-fil saknas — så ett färskt repo-klon räcker med att
köra `npm run dev`. Träningen har fast frö (42) och blir därmed **identisk**
oavsett vem som kör den. Tvinga fram omträning genom att radera zip-fil(er) och
starta om backend.

| Fil | Innehåll |
|---|---|
| `Modeller/hk_modell_flickor.zip` | BDI-modell tränad på flickorna (~2 364 rader) |
| `Modeller/hk_modell_pojkar.zip` | BDI-modell tränad på pojkarna (~2 446 rader) |
| `Modeller/hk_modell_somn_flickor.zip` | Sömnkvalitetsmodell (flickor) – används av `/api/somnpredict` |
| `Modeller/hk_modell_somn_pojkar.zip` | Sömnkvalitetsmodell (pojkar) – används av `/api/somnpredict` |

Förväntade värden: BDI RMSE ≈ 7,8 (flickor) / 4,9 (pojkar), sömn RMSE ≈ 0,87 / 0,67,
R² ca 0,12–0,16. Modellen förklarar alltså bara en liten del av variationen – se
notebookens "Tolkning av utvärderingen" för varför.

## Filstruktur

| Fil | Vad den gör |
|---|---|
| `HKRad.cs` | En rad i CSV-filen; `[LoadColumn]`-attribut kopplar egenskaper till kolumner |
| `HKTränare.cs` | Pipeline: filtrera kön → 80/20-uppdelning → linjär regression (SDCA) → utvärdera → spara. Målkolumnen utesluts automatiskt ur features (dataläckage-skydd) |
| `TränaModeller.cs` | Tränar de modeller vars zip saknas – anropas av backend vid start |
| `HKPrediktor.cs` | Laddar en sparad .zip och predikterar för en person |
| `HKPrediktion.cs` | Resultattyper: predikterad poäng + utvärderingsnyckeltal |

## Hur frontend använder modellen

**Sömnkvalitetsmodellerna är redan kopplade:** backend-filen `HKSomnPrediktor.cs`
(backend-roten) exponerar `POST /api/somnpredict` som tar emot
`{ leisureScreenHours, sleepHours, weekendMidsleep, socialJetlag, kon }`
(där `kon` är `"flicka"` eller `"pojke"`) och svarar med
`{ kon, sleepQuality }` (lägre värde = bättre sömn). Frontend-sidan är
`src/pages/SomnPredictor.tsx` (route `/somnpredictor`).

Obs: `screen_time_index` frågas inte ut – det är ett sammanvägt betyg (1–6) i
datasetet som användaren inte kan svara på. Endpointen härleder det från
fritidstimmarna via sambandet i datan (index ≈ 0,402 × timmar + 1,628,
korrelation 0,94).

BDI-modellerna kan användas var som helst i webbprojektet via
`HKModels.HKPrediktor.Ladda(...)` – se `HKPrediktor.cs` för exempel.

## Bra att veta

- **BDI** (`bdi_total`) är ett självrapporterat frågeformulärsvärde
  (Beck Depression Inventory), inte uträknat från övriga kolumner.
- Kolumnen `depressed` används inte – den hänger ihop med BDI och skulle
  läcka målet in i modellen.
- `PredictionEngine` är inte trådsäker – i webbmiljö, skapa en prediktion
  per anrop eller använd ML.NET:s `PredictionEnginePool`.
