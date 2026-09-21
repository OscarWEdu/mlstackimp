using mlstack;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api", () => "Test Call");

// HK:s sömnkvalitets-prediktor (tränad i hk_models) – allt ligger i HKSomnPrediktor.cs.
app.MapHKSomnPrediktor();

app.MapPost("/api/sleeppredict", (SleepRequest request) =>
{
    //Trains associated model if a dump of it doesn't already exist
    if (!File.Exists(LearningStacks.SleepTrainerModelPath) || !File.Exists(LearningStacks.SleepTrainerMaleModelPath) || !File.Exists(LearningStacks.SleepTrainerFemaleModelPath)) { LearningStacks.SleepTrainer(); }

    var bdi = Predictors.SleepBDIPrediction(
        request.SleepQualityIndex,
        request.AverageSleepHours,
        request.Sex
    );

    return Results.Ok(new { bdi });
});

app.MapGet("/api/sleepcurve", () =>
{
    //Trains associated model if a dump of it doesn't already exist
    if (!File.Exists(LearningStacks.SleepTrainerModelPath) || !File.Exists(LearningStacks.SleepTrainerMaleModelPath) || !File.Exists(LearningStacks.SleepTrainerFemaleModelPath)) { LearningStacks.SleepTrainer(); }

    //The model line is drawn at the median sleep duration, so it sits in the middle of the data
    var (observed, medianSleepHours) = LearningStacks.SleepObservedMeans(minCount: 20);
    var predicted = Predictors.SleepBDICurve(medianSleepHours);

    return Results.Ok(new { sleepHours = medianSleepHours, predicted, observed });
});

app.MapPost("/api/lifestylepredict", (LifestyleRequest request) =>
{
    //Trains associated models if a dump of them doesn't already exist
    if (!File.Exists(LearningStacks.LifestyleModelPath) || !File.Exists(LearningStacks.LifestyleMaleModelPath) || !File.Exists(LearningStacks.LifestyleFemaleModelPath)) { LearningStacks.LifestyleTrainer(); }

    var bdi = Predictors.LifestyleBDIPrediction(
        request.LeisureScreenHours,
        request.SleepQualityIndex,
        request.AverageSleepHours,
        request.Sex
    );

    return Results.Ok(new { bdi });
});

app.Run();
