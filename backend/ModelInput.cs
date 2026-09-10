namespace mlstack;

using Microsoft.ML.Data;

public class ModelInput
{
    [LoadColumn(0)]
    public float population { get; set; }

    [LoadColumn(1)]
    public float housing_median_age { get; set; }

    [LoadColumn(2)]
    public float median_house_value { get; set; }
}
