//Add objects handling data from CSV, formatted for training, in this file.
namespace mlstack;

using Microsoft.ML.Data;

public class SleepInput
{
    [LoadColumn(1)]
    public string sex { get; set; } = "";

    [LoadColumn(2)]
    public float screen_time_index { get; set; }

    [LoadColumn(3)]
    public float est_leisure_screen_hours { get; set; }

    [LoadColumn(4)]
    public float sleep_quality_index { get; set; }

    [LoadColumn(5)]
    public float avg_sleep_hours { get; set; }

    [LoadColumn(8)]
    public float bdi_total { get; set; }
}

