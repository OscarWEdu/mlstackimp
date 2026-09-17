//Add Prediction functions in this class
namespace mlstack;

using Microsoft.ML;

public static class Predictors
{
    private class SinglePrediction { public float Score { get; set; }} //PredictionEngine maps output to objects, hence this. 

    //Predicts BDI based on sleep_quality_index, and avg_sleep_hours
    public static float SleepBDIPrediction(double sleepQualityIndex, double averageSleepHours, string sex)
    {
        var input = new SleepInput
        {
            sleep_quality_index = (float)sleepQualityIndex,
            avg_sleep_hours = (float)averageSleepHours
        };

        //All saved model paths should be defined in LearningStacks
        string modelPath = (sex ?? string.Empty).ToLowerInvariant() switch
        {
            "male" => LearningStacks.SleepTrainerMaleModelPath,
            "female" => LearningStacks.SleepTrainerFemaleModelPath,
            _ => LearningStacks.SleepTrainerModelPath
        };

        var ctx = new MLContext();
        ITransformer model = ctx.Model.Load(modelPath, out _); //Gets model
        var predictionEngine = ctx.Model.CreatePredictionEngine<SleepInput, SinglePrediction>(model); //Model wrapper, which processes C# objects instead of dataframes
        SinglePrediction prediction = predictionEngine.Predict(input);

        return prediction.Score;
    }

    public static List<SleepCurvePoint> SleepBDICurve(double averageSleepHours)
    {
        string modelPath = LearningStacks.SleepTrainerModelPath; //All saved model paths should be defined in LearningStacks
        var ctx = new MLContext();
        ITransformer model = ctx.Model.Load(modelPath, out _); //Gets model
        var predictionEngine = ctx.Model.CreatePredictionEngine<SleepInput, SinglePrediction>(model);

        var points = new List<SleepCurvePoint>();
        for (int step = 0; step <= 20; step++)
        {
            double sleepQualityIndex = 1.0 + step * 0.25; //Integer steps, so the x values don't drift from floating point addition
            var input = new SleepInput
            {
                sleep_quality_index = (float)sleepQualityIndex,
                avg_sleep_hours = (float)averageSleepHours
            };
            points.Add(new SleepCurvePoint(sleepQualityIndex, predictionEngine.Predict(input).Score));
        }
        return points;
    }
}

