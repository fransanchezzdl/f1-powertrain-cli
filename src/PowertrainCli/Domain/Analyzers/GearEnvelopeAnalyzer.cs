using PowertrainCli.Domain.Models;

namespace PowertrainCli.Domain.Analyzers;

public class GearEnvelopeAnalyzer
{
    public IReadOnlyList<GearEnvelopeMetric> AnalyzeDriver(IReadOnlyList<TelemetrySample> samples, bool isDriverA)
    {
        var filtered = samples
            .Select(s => new {
                Gear = isDriverA ? s.GearA : s.GearB,
                Rpm = isDriverA ? s.RpmA : s.RpmB,
                Speed = isDriverA ? s.SpeedA : s.SpeedB,
                Throttle = isDriverA ? s.ThrottleA : s.ThrottleB
            })
            .Where(x => x.Throttle >= 98.0 && x.Gear > 0)
            .GroupBy(x => x.Gear)
            .OrderBy(g => g.Key);

        return filtered.Select(g => new GearEnvelopeMetric(
            Gear: g.Key,
            MinRpm: g.Min(x => x.Rpm),
            AvgRpm: Math.Round(g.Average(x => x.Rpm), 1),
            MaxRpm: g.Max(x => x.Rpm),
            MaxSpeedKmh: Math.Round(g.Max(x => x.Speed), 2),
            SampleCount: g.Count()
        )).ToList();
    }
}