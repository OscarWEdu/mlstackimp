namespace mlstack;

using Microsoft.ML.Data;

public class SleepInput
{
    [LoadColumn(4)]
    public float sleep_quality_index { get; set; }

    [LoadColumn(5)]
    public float avg_sleep_hours { get; set; }

    [LoadColumn(8)]
    public float bdi_total { get; set; }
}

