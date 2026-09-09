using PowertrainCli.Domain.Analyzers;
using PowertrainCli.Domain.Models;
using Xunit;

namespace PowertrainCli.Tests;

public class ClippingAnalyzerTests
{
    [Fact]
    public void DetectClipping_IdentifiesEnergyDrop_BeforeBrakingZone()
    {
        var samples = new List<TelemetrySample>
        {
            new(Distance: 800, SpeedA: 320.0, SpeedB: 310.0, RpmA: 12000, RpmB: 11800, GearA: 8, GearB: 8, ThrottleA: 100, ThrottleB: 100, DrsA: 0, DrsB: 0),
            new(Distance: 850, SpeedA: 316.0, SpeedB: 312.0, RpmA: 11850, RpmB: 11900, GearA: 8, GearB: 8, ThrottleA: 100, ThrottleB: 100, DrsA: 0, DrsB: 0), // Speed drop of 4 km/h
        };

        var analyzer = new ClippingAnalyzer();
        var events = analyzer.DetectClipping(samples, "RUS", isDriverA: true, speedDropThreshold: 2.0);

        Assert.Single(events);
        Assert.Equal("RUS", events[0].Driver);
        Assert.Equal(4.0, events[0].SpeedDropKmh);
    }
}