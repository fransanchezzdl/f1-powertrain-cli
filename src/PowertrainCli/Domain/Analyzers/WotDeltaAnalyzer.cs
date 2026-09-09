using PowertrainCli.Domain.Models;

namespace PowertrainCli.Domain.Analyzers;

public class WotDeltaAnalyzer : IPowertrainAnalyzer<PowertrainDeltaSummary>
{
    public PowertrainDeltaSummary Analyze(IReadOnlyList<TelemetrySample> samples)
    {
        var wotSamples = samples
            .Where(s => s.ThrottleA >= 98.0 && s.ThrottleB >= 98.0 && s.DrsA == 0 && s.DrsB == 0)
            .ToList();

        if (wotSamples.Count == 0)
            return new PowertrainDeltaSummary(0, 0, 0);

        var deltas = wotSamples.Select(s => s.SpeedA - s.SpeedB).ToList();
        return new PowertrainDeltaSummary(
            MeanSpeedDeltaKmh: Math.Round(deltas.Average(), 2),
            MaxSpeedDeltaKmh: Math.Round(deltas.Max(), 2),
            HighThrottleSampleCount: wotSamples.Count
        );
    }
}