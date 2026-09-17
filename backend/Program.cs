using mlstack;

double teststackscore = LearningStacks.ExampleStack();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api", () => $"R²:   {teststackscore:0.###}");

// HK:s sömnkvalitets-prediktor (tränad i hk_models) – allt ligger i HKSomnPrediktor.cs.
app.MapHKSomnPrediktor();

app.MapPost("/api/sleeppredict", (SleepRequest request) =>
{
    var bdi = Predictors.SleepBDIPrediction(
        request.SleepQualityIndex,
        request.AverageSleepHours
    );

    return Results.Ok(new { bdi });
});

app.Run();
