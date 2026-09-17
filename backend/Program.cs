using mlstack;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api", () => "Test Call");

app.MapPost("/api/sleeppredict", (SleepRequest request) =>
{
    //Trains associated model if a dump of it doesn't already exist
    if (!File.Exists(LearningStacks.SleepTrainerModelPath)) { LearningStacks.SleepTrainer(); }

    var bdi = Predictors.SleepBDIPrediction(
        request.SleepQualityIndex,
        request.AverageSleepHours,
        request.Sex
    );

    return Results.Ok(new { bdi });
});

app.Run();
