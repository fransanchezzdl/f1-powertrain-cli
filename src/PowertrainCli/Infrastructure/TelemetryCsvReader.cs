using System.Globalization;
using PowertrainCli.Domain.Models;

namespace PowertrainCli.Infrastructure;

public static class TelemetryCsvReader
{
    public static List<TelemetrySample> Read(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Telemetry file not found: {filePath}");

        var samples = new List<TelemetrySample>();
        using var reader = new StreamReader(filePath);

        string? header = reader.ReadLine(); // Skip header
        if (header is null) return samples;

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var tokens = line.Split(',');
            if (tokens.Length < 11) continue;

            samples.Add(new TelemetrySample(
                Distance: double.Parse(tokens[0], CultureInfo.InvariantCulture),
                SpeedA: double.Parse(tokens[1], CultureInfo.InvariantCulture),
                SpeedB: double.Parse(tokens[2], CultureInfo.InvariantCulture),
                RpmA: (int)Math.Round(double.Parse(tokens[3], CultureInfo.InvariantCulture)),
                RpmB: (int)Math.Round(double.Parse(tokens[4], CultureInfo.InvariantCulture)),
                GearA: (int)Math.Round(double.Parse(tokens[5], CultureInfo.InvariantCulture)),
                GearB: (int)Math.Round(double.Parse(tokens[6], CultureInfo.InvariantCulture)),
                ThrottleA: double.Parse(tokens[7], CultureInfo.InvariantCulture),
                ThrottleB: double.Parse(tokens[8], CultureInfo.InvariantCulture),
                DrsA: (int)Math.Round(double.Parse(tokens[9], CultureInfo.InvariantCulture)),
                DrsB: (int)Math.Round(double.Parse(tokens[10], CultureInfo.InvariantCulture))
            ));
        }
        return samples;
    }
}