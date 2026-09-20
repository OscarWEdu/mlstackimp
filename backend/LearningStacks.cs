//Add functions for ML training to this file
namespace mlstack;

using Microsoft.ML;

public static class LearningStacks
{
    public static string dataPath = "screen_time_mental_health.csv";

    // Define paths for saving models below here: 
    public static string SleepTrainerModelPath => Path.Combine(AppContext.BaseDirectory, "sleepmodel.zip");
    public static string SleepTrainerFemaleModelPath => Path.Combine(AppContext.BaseDirectory, "sleepmodelF.zip");
    public static string SleepTrainerMaleModelPath => Path.Combine(AppContext.BaseDirectory, "sleepmodelM.zip");

    public static string LifestyleModelPath => Path.Combine(AppContext.BaseDirectory, "lifestylemodel.zip");
    public static string LifestyleFemaleModelPath => Path.Combine(AppContext.BaseDirectory, "lifestylemodelF.zip");
    public static string LifestyleMaleModelPath => Path.Combine(AppContext.BaseDirectory, "lifestylemodelM.zip");

    // Example method, includes methods for validation, as well as both saving and loading.
    // For simple training, only steps 1, 2, 4, 5, and 8, are needed.
    public static double ExampleTrainer()
    {
        //Step 1. Create an ML Context
        var ctx = new MLContext();

        //Step 2. Read in the input data from a text file for model training
        IDataView trainingData = ctx.Data.LoadFromTextFile<SleepInput>(dataPath, hasHeader: true, separatorChar: ',');

        // Step 3. Split the data into training and test sets
        var split = ctx.Data.TrainTestSplit(trainingData, testFraction: 0.2);
        var trainData = split.TrainSet;
        var testData = split.TestSet;

        // Step 4. Build the data processing and training pipeline
        var pipeline = ctx.Transforms
            .Concatenate("Features",
                nameof(SleepInput.sleep_quality_index),
                nameof(SleepInput.avg_sleep_hours))
            .Append(ctx.Regression.Trainers.Sdca( // <- Model type specified here
                labelColumnName: nameof(SleepInput.bdi_total)));

        // Step 5. Train the model
        ITransformer trainedModel = pipeline.Fit(trainData);

        // Step 6. Make predictions on the test data
        IDataView predictions = trainedModel.Transform(testData);

        // Step 7. Evaluate the model
        var metrics = ctx.Regression.Evaluate(predictions, labelColumnName: nameof(SleepInput.bdi_total));

        // Step 8. Save the trained model
        ctx.Model.Save(trainedModel, trainData.Schema, SleepTrainerModelPath);

        // Step 9. Verify the model can load
        Console.WriteLine($"Reloaded R²: {ctx.Regression.Evaluate(ctx.Model.Load(SleepTrainerModelPath, out _).Transform(testData), labelColumnName: nameof(SleepInput.bdi_total)).RSquared:0.######}");
        return metrics.RSquared;
    }

    // ADD ML TRAINING FUNCTIONS HERE

    // Predicts BDI based on sleep_quality_index, and avg_sleep_hours, prediction performed by SleepBDIPrediction
    public static void SleepTrainer()
    {
        var ctx = new MLContext();

        IDataView dataset = ctx.Data.LoadFromTextFile<SleepInput>(dataPath, hasHeader: true, separatorChar: ',');
        var trainingData = ctx.Data.CreateEnumerable<SleepInput>(dataset, reuseRowObject: false).ToList();

        var allData = ctx.Data.LoadFromEnumerable(trainingData);
        var boysData = ctx.Data.LoadFromEnumerable(trainingData.Where(x => x.sex == "Boy"));
        var girlsData = ctx.Data.LoadFromEnumerable(trainingData.Where(x => x.sex == "Girl"));

        TrainSleepModel(ctx, allData, SleepTrainerModelPath);
        TrainSleepModel(ctx, boysData, SleepTrainerMaleModelPath);
        TrainSleepModel(ctx, girlsData, SleepTrainerFemaleModelPath);
    }

    // Function carrying out the actual training of the sleep models.
    private static void TrainSleepModel(MLContext ctx, IDataView trainingData, string modelPath)
    {
        var pipeline = ctx.Transforms //Build the data processing and training pipeline
            .Concatenate("Features",
                nameof(SleepInput.sleep_quality_index),
                nameof(SleepInput.avg_sleep_hours))
            .Append(ctx.Regression.Trainers.Sdca(
                labelColumnName: nameof(SleepInput.bdi_total)));

        var trainedModel = pipeline.Fit(trainingData);
        ctx.Model.Save(trainedModel, trainingData.Schema, modelPath);
    }

    // Predicts BDI based on screen_time_index, sleep_quality_index, and avg_sleep_hours.
    // Prediction performed by LifestyleBDIPrediction.
    public static void LifestyleTrainer()
    {
        var ctx = new MLContext(seed: 42);

        IDataView dataset = ctx.Data.LoadFromTextFile<SleepInput>(dataPath, hasHeader: true, separatorChar: ',');
        var trainingData = ctx.Data.CreateEnumerable<SleepInput>(dataset, reuseRowObject: false).ToList();

        var allData = ctx.Data.LoadFromEnumerable(trainingData);
        var boysData = ctx.Data.LoadFromEnumerable(trainingData.Where(x => x.sex == "Boy"));
        var girlsData = ctx.Data.LoadFromEnumerable(trainingData.Where(x => x.sex == "Girl"));

        TrainLifestyleModel(ctx, allData, LifestyleModelPath);
        TrainLifestyleModel(ctx, boysData, LifestyleMaleModelPath);
        TrainLifestyleModel(ctx, girlsData, LifestyleFemaleModelPath);
    }

    // Function carrying out the actual training of the lifestyle models.
    private static void TrainLifestyleModel(MLContext ctx, IDataView trainingData, string modelPath)
    {
        var pipeline = ctx.Transforms
            .Concatenate("Features",
                nameof(SleepInput.screen_time_index),
                nameof(SleepInput.sleep_quality_index),
                nameof(SleepInput.avg_sleep_hours))
            .Append(ctx.Regression.Trainers.Sdca(
                labelColumnName: nameof(SleepInput.bdi_total)));

        var trainedModel = pipeline.Fit(trainingData);
        ctx.Model.Save(trainedModel, trainingData.Schema, modelPath);
    }
}