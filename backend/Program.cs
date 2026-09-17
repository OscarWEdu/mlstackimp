using mlstack;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api", () => "Test Call");

// HK:s sömnkvalitets-prediktor (tränad i hk_models) – allt ligger i HKSomnPrediktor.cs.
app.MapHKSomnPrediktor();

app.MapPost("/api/sleeppredict", (SleepRequest request) =>
{
    //Trains associated model if a dump of it doesn't already exist
    if (!File.Exists(LearningStacks.SleepTrainerModelPath)) { LearningStacks.SleepTrainer(); }

    var bdi = Predictors.SleepBDIPrediction(
        request.SleepQualityIndex,
        request.AverageSleepHours
    );

    return Results.Ok(new { bdi });
});

app.Run();
