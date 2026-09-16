namespace mlstack;

using Microsoft.ML;

public static class Predictors
{
    private class SinglePrediction { public float Score { get; set; }} //PredictionEngine maps output to objects, hence this. 

    //TODO
    public static float SleepBDIPrediction(double sleepQualityIndex, double averageSleepHours)
    {
        var input = new SleepInput
        {
            sleep_quality_index = (float)sleepQualityIndex,
            avg_sleep_hours = (float)averageSleepHours
        };

        string modelPath = LearningStacks.ExampleStackModelPath;
        var ctx = new MLContext();
        ITransformer model = ctx.Model.Load(modelPath, out _);
        var predictionEngine = ctx.Model.CreatePredictionEngine<SleepInput, SinglePrediction>(model);
        SinglePrediction prediction = predictionEngine.Predict(input);

        return prediction.Score;
    }
}

