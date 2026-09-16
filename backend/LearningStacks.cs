//Add functions for ML training to this file
namespace mlstack;

using Microsoft.ML;

public static class LearningStacks
{

    // Define paths for saving models below here: 
    public static string ExampleStackModelPath => Path.Combine(AppContext.BaseDirectory, "model.zip");

    // Predicts BDI based on sleep_quality_index, and avg_sleep_hours
    // Serves as an example, but is currently also used in production together with SleepBDIPrediction
    public static double ExampleStack()
    {
        string dataPath = "screen_time_mental_health.csv";

        //Step 1. Create an ML Context
        var ctx = new MLContext();

        //Step 2. Read in the input data from a text file for model training
        IDataView trainingData = ctx.Data.LoadFromTextFile<SleepInput>(dataPath, hasHeader: true, separatorChar: ',');

        // Step 3. Split the data into training and test sets
        var split = ctx.Data.TrainTestSplit(trainingData,testFraction: 0.2);
        var trainData = split.TrainSet;
        var testData = split.TestSet;

        // Step 4. Build the data processing and training pipeline
        var pipeline = ctx.Transforms
            .Concatenate("Features",
                nameof(SleepInput.sleep_quality_index),
                nameof(SleepInput.avg_sleep_hours))
            .Append(ctx.Regression.Trainers.Sdca(
                labelColumnName: nameof(SleepInput.bdi_total)));

        // Step 5. Train the model
        ITransformer trainedModel = pipeline.Fit(trainData);

        // Step 6. Make predictions on the test data
        IDataView predictions = trainedModel.Transform(testData);

        // Step 7. Evaluate the model
        var metrics = ctx.Regression.Evaluate(predictions,labelColumnName: nameof(SleepInput.bdi_total));

        // Step 8. Save the trained model
        ctx.Model.Save(trainedModel, trainData.Schema, ExampleStackModelPath);

        // Step 9. Verify the model can load
        Console.WriteLine($"Reloaded R²: {ctx.Regression.Evaluate(ctx.Model.Load(ExampleStackModelPath, out _).Transform(testData), labelColumnName: nameof(SleepInput.bdi_total)).RSquared:0.######}");
        return metrics.RSquared;
    }
}