using PowertrainCli.Domain.Models;

namespace PowertrainCli.Domain.Analyzers;

public interface IPowertrainAnalyzer<TResult>
{
    TResult Analyze(IReadOnlyList<TelemetrySample> samples);
}