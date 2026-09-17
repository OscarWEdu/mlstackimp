// ============================================================================
// TränaModeller – ser till att alla HK-modeller finns sparade som .zip.
//
// Ingen egen startpunkt: metoden SäkerställAlla anropas av HKSomnPrediktor.cs
// när backend startar, och tränar bara de modeller vars zip-fil saknas
// (t.ex. efter en färsk klon av repot, eftersom *.zip är git-ignorerat).
//
// Träningen har fast frö (42) i HKTränare, så automatiskt tränade modeller
// blir identiska mellan körningar och mellan utvecklare.
// ============================================================================

namespace HKModels;

public static class TränaModeller
{
    // Alla modeller som ska finnas: (zip-filnamn, kön, målkolumn).
    private static readonly (string Filnamn, string Kön, string Målkolumn)[] AllaModeller =
    [
        ("hk_modell_flickor.zip", "Girl", nameof(HKRad.bdi_total)),
        ("hk_modell_pojkar.zip", "Boy", nameof(HKRad.bdi_total)),
        ("hk_modell_somn_flickor.zip", "Girl", nameof(HKRad.sleep_quality_index)),
        ("hk_modell_somn_pojkar.zip", "Boy", nameof(HKRad.sleep_quality_index)),
    ];

    /// <summary>
    /// Tränar de modeller vars zip-fil saknas i modellmappen.
    /// Sökvägar sätts av anroparen (HKSomnPrediktor.cs vid backend-start).
    /// </summary>
    /// <param name="csvSökväg">Sökväg till datasetet (screen_time_mental_health.csv).</param>
    /// <param name="modellMapp">Mapp där zip-filerna sparas/läses.</param>
    public static void SäkerställAlla(string csvSökväg, string modellMapp)
    {
        var tränare = new HKTränare(csvSökväg, modellMapp);

        foreach (var (filnamn, kön, målkolumn) in AllaModeller)
        {
            if (File.Exists(Path.Combine(modellMapp, filnamn))) continue;

            Console.WriteLine($"[TränaModeller] {filnamn} saknas – tränar automatiskt...");
            tränare.TränaOchSpara(kön, filnamn, målkolumn);
        }
    }
}
