using mlstack;

LearningStacks.SleepTrainer();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api", () => "Test Call");

app.MapPost("/api/sleeppredict", (SleepRequest request) =>
{
    var bdi = Predictors.SleepBDIPrediction(
        request.SleepQualityIndex,
        request.AverageSleepHours
    );

    return Results.Ok(new { bdi });
});

app.Run();
