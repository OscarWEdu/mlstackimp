// ============================================================================
// HK:s sömnkvalitets-prediktor – API-lager för frontend.
//
// Den här filen hör ihop med hk_models-projektet (där modellerna tränas och
// sparas) och är medvetet ISOLERAD i en egen fil: endast EN rad har lagts till
// i Program.cs (app.MapHKSomnPrediktor();) och inga andra befintliga filer
// behöver ändras.
//
// Flödet: frontend POSTar JSON till /api/somnpredict -> vi väljer rätt
// sparad modell (.zip) utifrån kön -> modellen predikterar ett
// sömnkvalitetsindex (lägre = bättre sömn) -> svaret skickas som JSON.
// ============================================================================

namespace mlstack;

using HKModels;

public static class HKSomnPrediktor
{
    // Hjälpare som hittar modellfilerna oavsett varifrån backend startas
    // (från repo-roten via "dotnet run --project backend" eller från backend/).
    private static string HittaModellsökväg(string filnamn)
    {
        // Alternativ 1: relativt aktuell katalog (backend/)
        var relativt = Path.Combine("hk_models", "Modeller", filnamn);
        if (File.Exists(relativt)) return relativt;

        // Alternativ 2: relativt programmappen (bin/Debug/net10.0/ -> tre steg upp)
        var frånProgram = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "hk_models", "Modeller", filnamn));
        if (File.Exists(frånProgram)) return frånProgram;

        throw new FileNotFoundException(
            $"Hittar inte modellfilen '{filnamn}'. Kör 'dotnet run' i backend/hk_models för att träna och spara modellerna först.");
    }

    // Laddar de två sparade sömnkvalitetsmodellerna (en flickmodell, en pojkmodell).
    private static readonly HKPrediktor Flickor =
        HKPrediktor.Ladda(HittaModellsökväg("hk_modell_somn_flickor.zip"));

    private static readonly HKPrediktor Pojkar =
        HKPrediktor.Ladda(HittaModellsökväg("hk_modell_somn_pojkar.zip"));

    /// <summary>
    /// Registrerar endpointen. Anropas från Program.cs med: app.MapHKSomnPrediktor();
    /// </summary>
    public static void MapHKSomnPrediktor(this WebApplication app)
    {
        app.MapPost("/api/somnpredict", (SomnRequest request) =>
        {
            // Välj modell efter kön – samma uppdelning som i träningen ("Girl"/"Boy").
            HKPrediktor prediktor;
            switch (request.Kon?.Trim().ToLowerInvariant())
            {
                case "flicka":
                case "girl":
                    prediktor = Flickor;
                    break;
                case "pojke":
                case "boy":
                    prediktor = Pojkar;
                    break;
                default:
                    return Results.BadRequest(new { error = "kon måste vara 'flicka' eller 'pojke'" });
            }

            // Gör prediktionen: index där LÄGRE värde = bättre sömn.
            var prediktion = prediktor.Predikera(new HKRad
            {
                screen_time_index = (float)request.ScreenTimeIndex,
                est_leisure_screen_hours = (float)request.LeisureScreenHours,
                avg_sleep_hours = (float)request.SleepHours,
                midsleep_weekend_hours = (float)request.WeekendMidsleep,
                social_jetlag_hours = (float)request.SocialJetlag,
            });

            return Results.Ok(new { kon = request.Kon, sleepQuality = Math.Round(prediktion, 2) });
        });
    }
}

// Data som frontend skickar in. Variabelnamnen är förenklade jämfört med
// CSV-kolumnerna och mappas enligt nedan:
//   ScreenTimeIndex     -> screen_time_index       (sammanvägt skärmtidsindex)
//   LeisureScreenHours  -> est_leisure_screen_hours (fritidsskärmtid, timmar/dag)
//   SleepHours          -> avg_sleep_hours          (sovtimmar per natt)
//   WeekendMidsleep     -> midsleep_weekend_hours   (insovningstid helg, timmar efter midnatt)
//   SocialJetlag        -> social_jetlag_hours      (skillnad sömnrutin vardag/helg, timmar)
//   Kon                 -> vilken modell som ska användas: "flicka" eller "pojke"
public record SomnRequest(
    double ScreenTimeIndex,
    double LeisureScreenHours,
    double SleepHours,
    double WeekendMidsleep,
    double SocialJetlag,
    string Kon
);
