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
    // Alla modeller skapas/laddas från träningsprojektets Modeller-mapp.
    private static readonly string ModellMapp = BestämModellmapp();

    private static string BestämModellmapp()
    {
        // Primärt: programmappen -> tre steg upp (bin/Debug/net10.0 -> backend) -> hk_models/Modeller
        var frånProgram = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "hk_models", "Modeller"));
        if (Directory.Exists(frånProgram)) return frånProgram;

        // Alternativ: relativt aktuell katalog
        var relativt = Path.GetFullPath(Path.Combine("hk_models", "Modeller"));
        if (Directory.Exists(relativt)) return relativt;

        // Saknas allt (ny klon): skapa mappen på standardplatsen vid första träningen.
        return frånProgram;
    }

    private static string Modellfil(string filnamn) => Path.Combine(ModellMapp, filnamn);

    /// <summary>
    /// Tränar en modell OM zip-filen saknas (t.ex. efter en färsk klon av repot,
    /// eftersom *.zip är git-ignorerat). Träningen har fast frö (42) i HKTränare,
    /// så den automatiskt tränade modellen blir identisk med en manuellt tränad.
    /// </summary>
    private static void SäkerställTränad(string filnamn, string kön)
    {
        if (File.Exists(Modellfil(filnamn))) return;

        Console.WriteLine($"[HKSomnPrediktor] {filnamn} saknas – tränar automatiskt " +
                          "(tar någon minut första gången)...");
        var csvSökväg = Path.GetFullPath(
            Path.Combine(ModellMapp, "..", "..", "screen_time_mental_health.csv"));
        new HKTränare(csvSökväg, ModellMapp)
            .TränaOchSpara(kön, filnamn, målkolumn: nameof(HKRad.sleep_quality_index));
    }

    // Laddar de två sparade sömnkvalitetsmodellerna (en flickmodell, en pojkmodell).
    // Initieras i den statiska konstruktorn så att ev. automatisk träning hinner
    // ske FÖRE inladdningen.
    private static readonly HKPrediktor Flickor;
    private static readonly HKPrediktor Pojkar;

    static HKSomnPrediktor()
    {
        SäkerställTränad("hk_modell_somn_flickor.zip", "Girl");
        SäkerställTränad("hk_modell_somn_pojkar.zip", "Boy");
        Flickor = HKPrediktor.Ladda(Modellfil("hk_modell_somn_flickor.zip"));
        Pojkar = HKPrediktor.Ladda(Modellfil("hk_modell_somn_pojkar.zip"));
    }

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
            //
            // screen_time_index är i datasetet ett helhetsbetyg (1–6) över totala
            // skärmvanor, inte rena timmar. Formuläret frågar bara efter fritids-
            // timmarna (som användaren kan svara på), så indexet härleds härifrån:
            // sambandet i datasetet är index ≈ 0,402 * fritidstimmar + 1,628
            // (korrelation 0,94), klamrat till betygsskalan 1–6.
            var screenTimeIndex = Math.Clamp(
                0.402f * (float)request.LeisureScreenHours + 1.628f, 1f, 6f);

            var prediktion = prediktor.Predikera(new HKRad
            {
                screen_time_index = screenTimeIndex,
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
//   LeisureScreenHours  -> est_leisure_screen_hours (fritidsskärmtid, timmar/dag)
//   SleepHours          -> avg_sleep_hours          (sovtimmar per natt)
//   WeekendMidsleep     -> midsleep_weekend_hours   (insovningstid helg, timmar efter midnatt)
//   SocialJetlag        -> social_jetlag_hours      (skillnad sömnrutin vardag/helg, timmar)
//   Kon                 -> vilken modell som ska användas: "flicka" eller "pojke"
//
// OBS: screen_time_index frågas inte ut – det är ett sammanvägt betyg (1–6) i
// datasetet som användaren inte kan svara på. Det härleds i stället från
// fritidstimmarna (se kommentaren i endpointen).
public record SomnRequest(
    double LeisureScreenHours,
    double SleepHours,
    double WeekendMidsleep,
    double SocialJetlag,
    string Kon
);
