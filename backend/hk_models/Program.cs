// ============================================================================
// hk_models – startpunkt för HK:s ML-projekt.
//
// Kör det här projektet med:  dotnet run   (från den här mappen)
//
// Programmet gör tre saker:
//   1. Tränar en BDI-modell för flickor och en för pojkar (HKTränare).
//   2. Sparar båda som .zip i Modeller-mappen – det är de filerna frontend/
//      backend-tjänsten sedan laddar in, se HKPrediktor och README.md.
//   3. Gör ett exempel-anrop mot varje SPARAD modell för att visa att
//      spara→ladda→prediktera fungerar hela vägen.
// ============================================================================

namespace HKModels;

using System.Text;

public static class Program
{
    public static void Main(string[] args)
    {
        // Ser till att ä, ö och å skrivs ut korrekt i konsolen oavsett Windows-inställningar.
        Console.OutputEncoding = Encoding.UTF8;

        // Datasetet ligger i backend-roten, alltså en nivå ovanför den här mappen.
        // Kan skrivas över via kommandoraden: dotnet run -- <sökväg-till-csv>
        var csvSökväg = args.Length > 0 ? args[0] : "../screen_time_mental_health.csv";

        if (!File.Exists(csvSökväg))
        {
            Console.Error.WriteLine($"Hittar inte datasetet: {csvSökväg}");
            Environment.Exit(1);
        }

        var tränare = new HKTränare(csvSökväg);

        // ---- 1 + 2: Träna och spara en modell per kön och målkolumn ----
        // BDI-modellerna (som hk_modell_flickor/pojkar) predikterar depressionspoängen,
        // sömnmodellerna (hk_modell_somn_*) predikterar sömnkvalitetsindexet och används
        // av frontend via backend-endpointen /api/somnpredict.
        tränare.TränaOchSpara(kön: "Girl", filnamn: "hk_modell_flickor.zip",
                              målkolumn: nameof(HKRad.bdi_total));
        Console.WriteLine();
        tränare.TränaOchSpara(kön: "Boy", filnamn: "hk_modell_pojkar.zip",
                              målkolumn: nameof(HKRad.bdi_total));
        Console.WriteLine();
        tränare.TränaOchSpara(kön: "Girl", filnamn: "hk_modell_somn_flickor.zip",
                              målkolumn: nameof(HKRad.sleep_quality_index));
        Console.WriteLine();
        tränare.TränaOchSpara(kön: "Boy", filnamn: "hk_modell_somn_pojkar.zip",
                              målkolumn: nameof(HKRad.sleep_quality_index));

        // ---- 3: Demonstrera att de sparade modellerna kan användas ----
        // Vi laddar in zip-filerna igen och predikterar för en påhittad elev med
        // "genomsnittliga" värden från datasetet. Detta simulerar precis det
        // frontend-anropet kommer göra via backend-tjänsten.
        Console.WriteLine();
        Console.WriteLine("Exempel: prediktion från SPARADE modellfiler");

        var exempelElev = new HKRad
        {
            screen_time_index = 3.2f,
            est_leisure_screen_hours = 4.0f,
            sleep_quality_index = 1.9f,
            avg_sleep_hours = 7.7f,
            midsleep_weekend_hours = 5.6f,
            social_jetlag_hours = 2.4f,
        };

        var flickPrediktor = HKPrediktor.Ladda(Path.Combine("Modeller", "hk_modell_flickor.zip"));
        Console.WriteLine($"  Flickor: predikterad BDI = {flickPrediktor.Predikera(exempelElev):0.0}");

        var pojkarPrediktor = HKPrediktor.Ladda(Path.Combine("Modeller", "hk_modell_pojkar.zip"));
        Console.WriteLine($"  Pojkar:  predikterad BDI = {pojkarPrediktor.Predikera(exempelElev):0.0}");

        Console.WriteLine();
        Console.WriteLine("Klart! Modellerna ligger i Modeller-mappen, redo att laddas av frontend/backend.");
    }
}
