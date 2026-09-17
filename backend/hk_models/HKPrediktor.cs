namespace HKModels;

using Microsoft.ML;

/// <summary>
/// Laddar en SPARAD modell (.zip) och gör prediktioner med den.
///
/// Det här är klassen frontend-integrationen använder: den kräver ingen
/// träningsdata och ingen omträning – bara zip-filen från Modeller-mappen.
///
/// Exempel (t.ex. i webbprojektets API):
/// <code>
///   var prediktor = HKPrediktor.Ladda("Modeller/hk_modell_flickor.zip");
///   var bdi = prediktor.Predikera(new HKRad { sleep_quality_index = 3f, ... });
/// </code>
/// </summary>
public sealed class HKPrediktor
{
    private readonly PredictionEngine<HKRad, HKPrediktion> _engine;

    private HKPrediktor(PredictionEngine<HKRad, HKPrediktion> engine)
    {
        _engine = engine;
    }

    /// <summary>
    /// Laddar en sparad modell från en .zip-fil och gör den redo för prediktioner.
    /// </summary>
    /// <param name="zipSökväg">Sökväg till modellen, t.ex. "Modeller/hk_modell_flickor.zip".</param>
    public static HKPrediktor Ladda(string zipSökväg)
    {
        // Eget MLContext behövs bara för att skapa prediktionsmotorn.
        var ml = new MLContext();

        // Läs in hela den tränade pipelinen från zip-filen.
        ITransformer modell = ml.Model.Load(zipSökväg, out _);

        // En PredictionEngine är "en rad in, en prediktion ut" – lämpad för
        // enkla API-anrop. OBS: inte trådsäker, skapa en per anrop i webbmiljö
        // eller använd PredictionEnginePool.
        var engine = ml.Model.CreatePredictionEngine<HKRad, HKPrediktion>(modell);

        return new HKPrediktor(engine);
    }

    /// <summary>
    /// Predikterar BDI-poängen för en person utifrån de sex förklaringsvariablerna.
    /// </summary>
    /// <param name="rad">Personens värden (fältet sex/bdi_total behöver inte vara satta).</param>
    /// <returns>Predikterad BDI-poäng.</returns>
    public float Predikera(HKRad rad)
    {
        return _engine.Predict(rad).PredikeradBdiTotal;
    }
}
