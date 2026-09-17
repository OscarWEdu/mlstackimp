namespace HKModels;

using Microsoft.ML.Data;

/// <summary>
/// Det modellen producerar som resultat för en rad.
///
/// ML.NET skriver alltid en regressions prediktion till en kolumn
/// som heter "Score". [ColumnName]-attributet kopplar vår egenskap till den
/// interna kolumnen så att vi kan läsa ut värdet med ett vettigt namn.
/// </summary>
public sealed class HKPrediktion
{
    /// <summary>Den predikterade BDI-poängen för personen.</summary>
    [ColumnName("Score")]
    public float PredikeradBdiTotal { get; set; }
}

/// <summary>
/// Nyckeltal från modellens utvärdering på testdatan (20 % av raderna som
/// modellen aldrig sett under träningen). Samma nyckeltal som i notebooken:
/// lägre RMSE/MAE = bättre, R² nära 1 = bättre.
/// </summary>
public sealed class HKMetrik
{
    /// <summary>Root Mean Squared Error – typiskt fel i BDI-poäng (viktas hårdare mot stora fel).</summary>
    public double Rmse { get; init; }

    /// <summary>Mean Absolute Error – genomsnittligt fel i BDI-poäng.</summary>
    public double Mae { get; init; }

    /// <summary>Andel av variationen i BDI som modellen förklarar (0–1). Här förväntas ~0,12–0,16.</summary>
    public double R2 { get; init; }

    /// <summary>Antal rader som modellen tränades på.</summary>
    public long AntalTräning { get; init; }

    /// <summary>Antal rader som utvärderingen gjordes på.</summary>
    public long AntalTest { get; init; }
}
