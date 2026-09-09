using PowertrainCli.Domain.Models;

namespace PowertrainCli.Formatting;

public static class AsciiTableFormatter
{
    public const string Banner = """
███████╗ ██╗
██╔════╝███║
█████╗  ╚██║
██╔══╝   ██║
██║      ██║
╚═╝      ╚═╝ POWERTRAIN PERFORMANCE ANALYZER
""";

    public static void PrintReport(
        string driverA,
        string driverB,
        IReadOnlyList<GearEnvelopeMetric> envA,
        IReadOnlyList<GearEnvelopeMetric> envB,
        PowertrainDeltaSummary deltas,
        IReadOnlyList<ClippingEvent> clippingEvents)
    {
        Console.WriteLine(Banner);
        Console.WriteLine("================================================================================");
        Console.WriteLine($"POWERTRAIN OPERATING ENVELOPE (WOT >= 98%): {driverA} vs {driverB}");
        Console.WriteLine("================================================================================");
        Console.WriteLine(string.Format("{0,-5} | {1,-14} | {2,-11} | {3,-14} | {4,-11}", "Gear", $"{driverA} Max V", $"{driverA} Avg RPM", $"{driverB} Max V", $"{driverB} Avg RPM"));
        Console.WriteLine(new string('-', 80));

        var gears = envA.Select(x => x.Gear).Union(envB.Select(x => x.Gear)).OrderBy(g => g);
        foreach (var gear in gears)
        {
            var a = envA.FirstOrDefault(x => x.Gear == gear);
            var b = envB.FirstOrDefault(x => x.Gear == gear);

            string vA = a is not null ? $"{a.MaxSpeedKmh:F1} km/h" : "N/A";
            string rpmA = a is not null ? $"{a.AvgRpm:F0} RPM" : "N/A";
            string vB = b is not null ? $"{b.MaxSpeedKmh:F1} km/h" : "N/A";
            string rpmB = b is not null ? $"{b.AvgRpm:F0} RPM" : "N/A";

            Console.WriteLine(string.Format("{0,-5} | {1,-14} | {2,-11} | {3,-14} | {4,-11}", gear, vA, rpmA, vB, rpmB));
        }

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("FULL THROTTLE ACCELERATION SUMMARY (DRS CLOSED)");
        Console.WriteLine("================================================================================");
        Console.WriteLine($"Mean Speed Delta ({driverA} - {driverB}): {deltas.MeanSpeedDeltaKmh:+0.00;-0.00;0.00} km/h");
        Console.WriteLine($"Max Speed Delta ({driverA} - {driverB}):  {deltas.MaxSpeedDeltaKmh:+0.00;-0.00;0.00} km/h");
        Console.WriteLine($"Total Analyzed Samples:            {deltas.HighThrottleSampleCount}");

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("ENERGY DEPLOYMENT / CLIPPING DETECTION");
        Console.WriteLine("================================================================================");
        if (clippingEvents.Count == 0)
        {
            Console.WriteLine("No significant high-speed energy deployment decay (> 3.5 km/h) detected.");
        }
        else
        {
            Console.WriteLine(string.Format("{0,-8} | {1,-19} | {2,-13} | {3}", "Driver", "Track Interval", "Speed Decay", "Diagnostic"));
            Console.WriteLine(new string('-', 70));
            foreach (var evt in clippingEvents.OrderByDescending(x => x.SpeedDropKmh).Take(5))
            {
                string interval = $"{evt.StartDistanceMeters:F0}m -> {evt.EndDistanceMeters:F0}m";
                Console.WriteLine(string.Format("{0,-8} | {1,-19} | {2,-13} | {3}", evt.Driver, interval, $"-{evt.SpeedDropKmh:F1} km/h", "MGU-K Depletion"));
            }
        }
        Console.WriteLine("================================================================================\n");
    }
}