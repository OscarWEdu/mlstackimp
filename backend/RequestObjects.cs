//Add objects handling data from the frontend in this file
namespace mlstack;

public record SleepRequest(
    double SleepQualityIndex,
    int AverageSleepHours,
    string Sex
);
public record LifestyleRequest(
    double ScreenTimeIndex,
    double SleepQualityIndex,
    double AverageSleepHours,
    string Sex
);