//Add Prediction functions in this class
namespace mlstack;

using Microsoft.ML;

public static class Predictors
{
    private class SinglePrediction { public float Score { get; set; }} //PredictionEngine maps output to objects, hence this. 

    //Predicts BDI based on sleep_quality_index, and avg_sleep_hours
    public static float SleepBDIPrediction(double sleepQualityIndex, double averageSleepHours)
    {
        var input = new SleepInput
        {
            sleep_quality_index = (float)sleepQualityIndex,
            avg_sleep_hours = (float)averageSleepHours
        };

        string modelPath = LearningStacks.SleepTrainerModelPath; //All saved model paths should be defined in LearningStacks
        var ctx = new MLContext();
        ITransformer model = ctx.Model.Load(modelPath, out _); //Gets model
        var predictionEngine = ctx.Model.CreatePredictionEngine<SleepInput, SinglePrediction>(model); //Model wrapper, which processes C# objects instead of dataframes
        SinglePrediction prediction = predictionEngine.Predict(input);

        return prediction.Score;
    }
}

