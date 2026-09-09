using PowertrainCli.Domain.Models;

namespace PowertrainCli.Tests.Mocks;

public static class SyntheticTelemetryFactory
{
    public static List<TelemetrySample> CreateStraightLineSample() => new()
    {
        new(Distance: 100, SpeedA: 280.0, SpeedB: 275.0, RpmA: 11500, RpmB: 11200, GearA: 7, GearB: 7, ThrottleA: 100.0, ThrottleB: 100.0, DrsA: 0, DrsB: 0),
        new(Distance: 200, SpeedA: 300.0, SpeedB: 290.0, RpmA: 11800, RpmB: 11400, GearA: 7, GearB: 7, ThrottleA: 100.0, ThrottleB: 100.0, DrsA: 0, DrsB: 0),
        new(Distance: 300, SpeedA: 320.0, SpeedB: 305.0, RpmA: 11200, RpmB: 11600, GearA: 8, GearB: 7, ThrottleA: 100.0, ThrottleB: 100.0, DrsA: 0, DrsB: 0),
    };
}