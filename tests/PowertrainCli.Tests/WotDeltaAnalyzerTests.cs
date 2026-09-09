using PowertrainCli.Domain.Analyzers;
using PowertrainCli.Tests.Mocks;
using Xunit;

namespace PowertrainCli.Tests;

public class WotDeltaAnalyzerTests
{
    [Fact]
    public void Analyze_CalculatesAccurateSpeedDeltas_UnderFullThrottle()
    {
        var analyzer = new WotDeltaAnalyzer();
        var samples = SyntheticTelemetryFactory.CreateStraightLineSample();

        var result = analyzer.Analyze(samples);

        Assert.Equal(3, result.HighThrottleSampleCount);
        Assert.Equal(10.0, result.MeanSpeedDeltaKmh); // (5 + 10 + 15) / 3 = 10.0
        Assert.Equal(15.0, result.MaxSpeedDeltaKmh);
    }
}