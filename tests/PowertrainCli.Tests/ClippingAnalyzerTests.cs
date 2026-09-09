using PowertrainCli.Domain.Analyzers;
using PowertrainCli.Domain.Models;
using Xunit;

namespace PowertrainCli.Tests;

public class ClippingAnalyzerTests
{
    [Fact]
    public void DetectClipping_IdentifiesSingleHighSpeedEnergyDrop()
    {
        var samples = new List<TelemetrySample>
        {
            new(Distance: 800, SpeedA: 320.0, SpeedB: 310.0, RpmA: 12000, RpmB: 11800, GearA: 8, GearB: 8, ThrottleA: 100, ThrottleB: 100, DrsA: 0, DrsB: 0),
            new(Distance: 850, SpeedA: 316.0, SpeedB: 312.0, RpmA: 11850, RpmB: 11900, GearA: 8, GearB: 8, ThrottleA: 100, ThrottleB: 100, DrsA: 0, DrsB: 0),
            new(Distance: 900, SpeedA: 314.0, SpeedB: 311.0, RpmA: 11750, RpmB: 11850, GearA: 8, GearB: 8, ThrottleA: 100, ThrottleB: 100, DrsA: 0, DrsB: 0),
        };

        var analyzer = new ClippingAnalyzer();
        var events = analyzer.DetectClipping(samples, "RUS", isDriverA: true);

        Assert.Single(events);
        Assert.Equal("RUS", events[0].Driver);
        Assert.Equal(4.0, events[0].SpeedDropKmh);
        Assert.Equal(800.0, events[0].StartDistanceMeters);
        Assert.Equal(850.0, events[0].EndDistanceMeters);
    }

    [Fact]
    public void DetectClipping_IgnoresLowSpeedThrottleDrops()
    {
        var samples = new List<TelemetrySample>
        {
            new(Distance: 1200, SpeedA: 280.0, SpeedB: 280.0, RpmA: 11000, RpmB: 11000, GearA: 7, GearB: 7, ThrottleA: 100, ThrottleB: 100, DrsA: 0, DrsB: 0),
            new(Distance: 1250, SpeedA: 274.0, SpeedB: 274.0, RpmA: 10800, RpmB: 10800, GearA: 7, GearB: 7, ThrottleA: 100, ThrottleB: 100, DrsA: 0, DrsB: 0),
        };

        var analyzer = new ClippingAnalyzer();

        Assert.Empty(analyzer.DetectClipping(samples, "RUS", isDriverA: true));
    }
}