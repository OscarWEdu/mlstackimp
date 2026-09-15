//Add objects handling data from the frontend in this file
namespace mlstack;

public record SleepRequest(
    double SleepQualityIndex,
    int AverageSleepHours
);