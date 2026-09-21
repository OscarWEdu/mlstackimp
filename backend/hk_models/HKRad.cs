namespace HKModels;

using Microsoft.ML.Data;

/// <summary>
/// Representerar EN rad i screen_time_mental_health.csv.
///
/// [LoadColumn]-attributen talar om för ML.NET vilken kolumn (0-baserat index)
/// i CSV-filen varje egenskap ska läsas från. Kolumnordningen i filen är:
///   0 subject_id, 1 sex, 2 screen_time_index, 3 est_leisure_screen_hours,
///   4 sleep_quality_index, 5 avg_sleep_hours, 6 midsleep_weekend_hours,
///   7 social_jetlag_hours, 8 bdi_total, 9 depressed
///
/// Kolumnen "depressed" (index 9) är medvetet utelämnad: den hänger ihop med
/// BDI-poängen och ska inte användas för att prediktera den.
/// </summary>
public sealed class HKRad
{
    /// <summary>Personens id i datasetet (används inte av modellen).</summary>
    [LoadColumn(0)]
    public float subject_id { get; set; }

    /// <summary>Kön: "Girl" eller "Boy" – avgör vilken av de två modellerna som gäller.</summary>
    [LoadColumn(1)]
    public string sex { get; set; } = "";

    // ----- De sex förklaringsvariablerna (samma som i "HK Eda"-notebooken) -----

    /// <summary>Sammanvägt index för skärmtid.</summary>
    [LoadColumn(2)]
    public float screen_time_index { get; set; }

    /// <summary>Uppskattad skärmtid på fritiden (timmar per dag).</summary>
    [LoadColumn(3)]
    public float est_leisure_screen_hours { get; set; }

    /// <summary>Index för sömnkvalitet (högre värde = sämre sömn).</summary>
    [LoadColumn(4)]
    public float sleep_quality_index { get; set; }

    /// <summary>Snitt sovtimmar per natt.</summary>
    [LoadColumn(5)]
    public float avg_sleep_hours { get; set; }

    /// <summary>Mittpunkt för sömn på helgen (senare = mer förskjuten rytm).</summary>
    [LoadColumn(6)]
    public float midsleep_weekend_hours { get; set; }

    /// <summary>Skillnad i sömn mellan vardag och helg ("social jetlag").</summary>
    [LoadColumn(7)]
    public float social_jetlag_hours { get; set; }

    // ----- Målvariabeln -----

    /// <summary>
    /// BDI-poängen (Beck Depression Inventory) från frågeformuläret.
    /// Detta är det modellen ska prediktera (label).
    /// OBS: det är ett självrapporterat formulärresultat, inte ett uträknat värde.
    /// </summary>
    [LoadColumn(8)]
    public float bdi_total { get; set; }
}
