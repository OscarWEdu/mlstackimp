//Add objects handling data from the frontend in this file
namespace mlstack;

public record SleepRequest(
    double SleepQualityIndex,
    int AverageSleepHours
);

public record SleepObservedPoint(
    double SleepQualityIndex,
    double MeanBdi,
    int Count
);

public record SleepCurvePoint(
    double SleepQualityIndex,
    double MeanBdi
);