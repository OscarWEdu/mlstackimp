namespace HKModels;

using Microsoft.ML;

/// <summary>
/// Tränar och sparar HK:s BDI-modeller – en per kön (flickor/pojkar),
/// exakt som i "HK Eda"-notebooken där varje segment analyserats separat.
///
/// Flödet i varje träning:
///   1. Läs in CSV-filen.
///   2. Filtrera fram rätt kön (segment).
///   3. Dela upp i träningsdata (80 %) och testdata (20 %).
///   4. Bygg pipeline: slå ihop de sex förklaringsvariablerna → linjär regression (SDCA).
///   5. Träna på träningsdatan.
///   6. Utvärdera på testdatan (RMSE, MAE, R²).
///   7. Spara modellen som .zip som frontend/backend-tjänster kan ladda in.
/// </summary>
public sealed class HKTränare
{
    // Fast slumpfrö => träningen blir likadan varje gång den körs (lättare att förklara och jämföra).
    private readonly MLContext _ml = new(seed: 42);

    // Alla kolumner som KAN användas som förklaringsvariabler (samma som i "HK Eda"-notebooken).
    private static readonly string[] AllaFeatures =
    [
        nameof(HKRad.screen_time_index),
        nameof(HKRad.est_leisure_screen_hours),
        nameof(HKRad.sleep_quality_index),
        nameof(HKRad.avg_sleep_hours),
        nameof(HKRad.midsleep_weekend_hours),
        nameof(HKRad.social_jetlag_hours),
    ];

    /// <summary>Sökväg till datasetet (CSV-filen).</summary>
    private readonly string _csvSökväg;

    /// <summary>Mapp där de tränade modellerna (.zip) sparas.</summary>
    private readonly string _modellMapp;

    public HKTränare(string csvSökväg, string modellMapp = "Modeller")
    {
        _csvSökväg = csvSökväg;
        _modellMapp = modellMapp;
    }

    /// <summary>
    /// Tränar en modell för ett kön och sparar den som .zip.
    /// </summary>
    /// <param name="kön">Vilket segment som ska tränas: "Girl" eller "Boy".</param>
    /// <param name="filnamn">Namn på zip-filen som skapas i modellmappen.</param>
    /// <param name="målkolumn">Vad modellen ska prediktera: bdi_total (standard)
    /// eller sleep_quality_index för sömnkvalitetsmodellerna.</param>
    public HKMetrik TränaOchSpara(string kön, string filnamn, string målkolumn = nameof(HKRad.bdi_total))
    {
        // ---- Steg 1: Läs in hela datasetet från CSV-filen ----
        // LoadFromTextFile använder [LoadColumn]-attributen i HKRad för att veta
        // vilken kolumn varje egenskap kommer ifrån.
        var allData = _ml.Data.LoadFromTextFile<HKRad>(
            path: _csvSökväg,
            hasHeader: true,
            separatorChar: ',');

        // ---- Steg 2: Filtrera fram segmentet (flickor ELLER pojkar) ----
        // Vi plockar ut raderna som C#-objekt, filtrerar med vanlig LINQ
        // och läser in dem igen som träningsdata. Enkelt och lätt att förklara.
        var segment = _ml.Data.CreateEnumerable<HKRad>(allData, reuseRowObject: false)
            .Where(rad => rad.sex == kön)
            .ToList();
        var segmentData = _ml.Data.LoadFromEnumerable(segment);

        // ---- Steg 3: Dela upp i träning (80 %) och test (20 %) ----
        // Testdatan hålls utanför träningen så utvärderingen visar hur modellen
        // klarar data den ALDRIG sett – precis som train_test_split i notebooken.
        var uppdelning = _ml.Data.TrainTestSplit(segmentData, testFraction: 0.2);

        // ---- Steg 4: Bygg pipelinen ----
        // a) Features = alla förklaringsvariabler UTOM målkolumnen. Att ta bort
        //    målet ur features är kritiskt – annars "läcker" svaret in i modellen
        //    och den ser perfekt ut men är värdelös på riktigt data (dataläckage).
        // b) Concatenate: packar features i EN "Features"-vektor, vilket är
        //    formatet alla ML.NET-tränare förväntar sig.
        // c) Sdca: linjär regression (samma modelltyp som vann i notebooken).
        var features = AllaFeatures.Where(f => f != målkolumn).ToArray();
        var pipeline = _ml.Transforms
            .Concatenate("Features", features)
            .Append(_ml.Regression.Trainers.Sdca(
                labelColumnName: målkolumn,
                featureColumnName: "Features"));

        Console.WriteLine($"Tränar modell för {(kön == "Girl" ? "FLICKOR" : "POJKAR")} (mål: {målkolumn}) " +
                          $"({segment.Count} rader, varav {(long)(segment.Count * 0.8)} till träning)...");

        // ---- Steg 5: Träna modellen ----
        var modell = pipeline.Fit(uppdelning.TrainSet);

        // ---- Steg 6: Utvärdera på testdatan ----
        // Transform kör testdatan genom modellen, och Regression.Evaluate
        // jämför prediktionerna med de faktiska bdi_total-värdena.
        var prediktioner = modell.Transform(uppdelning.TestSet);
        var metrikFrånML = _ml.Regression.Evaluate(
            prediktioner, labelColumnName: målkolumn);

        var metrik = new HKMetrik
        {
            Rmse = metrikFrånML.RootMeanSquaredError,
            Mae = metrikFrånML.MeanAbsoluteError,
            R2 = metrikFrånML.RSquared,
            AntalTräning = uppdelning.TrainSet.GetRowCount() ?? 0,
            AntalTest = uppdelning.TestSet.GetRowCount() ?? 0,
        };

        Console.WriteLine($"  RMSE: {metrik.Rmse:0.00}  MAE: {metrik.Mae:0.00}  R²: {metrik.R2:0.000}");

        // ---- Steg 7: Spara modellen som .zip ----
        // Zip-filen innehåller hela pipelinen + modellvikterna.
        // Frontend/backend-tjänsten laddar in den med HKPrediktor – ingen omträning krävs.
        Directory.CreateDirectory(_modellMapp);
        var zipSökväg = Path.Combine(_modellMapp, filnamn);
        _ml.Model.Save(modell, uppdelning.TrainSet.Schema, zipSökväg);
        Console.WriteLine($"  Sparad till: {zipSökväg}");

        return metrik;
    }
}
