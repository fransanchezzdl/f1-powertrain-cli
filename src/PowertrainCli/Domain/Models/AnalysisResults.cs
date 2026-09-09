namespace PowertrainCli.Domain.Models;

public record GearEnvelopeMetric(
    int Gear,
    int MinRpm,
    double AvgRpm,
    int MaxRpm,
    double MaxSpeedKmh,
    int SampleCount
);

public record PowertrainDeltaSummary(
    double MeanSpeedDeltaKmh,
    double MaxSpeedDeltaKmh,
    int HighThrottleSampleCount
);

public record ClippingEvent(
    string Driver,
    double StartDistanceMeters,
    double EndDistanceMeters,
    double SpeedDropKmh
);