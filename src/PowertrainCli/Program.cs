using System.Diagnostics;
using PowertrainCli.Domain.Analyzers;
using PowertrainCli.Formatting;
using PowertrainCli.Infrastructure;

// Defaults
string csvPath = "/app/data/telemetry_sync.csv";
int year = 2026;
string gp = "Monza";
string session = "Q";
string driverA = "RUS";
string driverB = "ALO";
bool autoFetch = false;

// Parse CLI flags for one-shot mode
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--csv" && i + 1 < args.Length) csvPath = args[++i];
    if (args[i] == "--year" && i + 1 < args.Length) int.TryParse(args[++i], out year);
    if (args[i] == "--gp" && i + 1 < args.Length) gp = args[++i];
    if (args[i] == "--session" && i + 1 < args.Length) session = args[++i];
    if (args[i] == "--driver-a" && i + 1 < args.Length) driverA = args[++i];
    if (args[i] == "--driver-b" && i + 1 < args.Length) driverB = args[++i];
    if (args[i] == "--fetch") autoFetch = true;
}

bool isInteractive = args.Length == 0 || args.Contains("--interactive");

async Task<bool> RunIngestionWithSpinner(int yr, string grandPrix, string sess, string dA, string dB, string outputCsv)
{
    var outputDir = Path.GetDirectoryName(outputCsv);
    if (string.IsNullOrEmpty(outputDir)) outputDir = "/app/data";

    var psi = new ProcessStartInfo
    {
        FileName = "python",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        WorkingDirectory = "/app"
    };
    psi.ArgumentList.Add("-m");
    psi.ArgumentList.Add("ingestion.export_telemetry");
    psi.ArgumentList.Add("--year");
    psi.ArgumentList.Add(yr.ToString());
    psi.ArgumentList.Add("--gp");
    psi.ArgumentList.Add(grandPrix);
    psi.ArgumentList.Add("--session");
    psi.ArgumentList.Add(sess);
    psi.ArgumentList.Add("--driver-a");
    psi.ArgumentList.Add(dA);
    psi.ArgumentList.Add("--driver-b");
    psi.ArgumentList.Add(dB);
    psi.ArgumentList.Add("--output-dir");
    psi.ArgumentList.Add(outputDir);

    try
    {
        using var proc = Process.Start(psi);
        if (proc is null)
        {
            Console.WriteLine("[Error] Failed to start Python process.");
            return false;
        }

        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();
        using var spinnerCancellation = new CancellationTokenSource();
        string[] spinnerFrames = ["⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"];
        var spinnerTask = Task.Run(async () =>
        {
            var frame = 0;
            while (!spinnerCancellation.Token.IsCancellationRequested)
            {
                Console.Write($"\r[Ingestion] {spinnerFrames[frame++ % spinnerFrames.Length]} Fetching telemetry...");
                try
                {
                    await Task.Delay(100, spinnerCancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        });

        await proc.WaitForExitAsync();
        spinnerCancellation.Cancel();
        await spinnerTask;
        string errorOutput = await stderrTask;
        await stdoutTask;
        Console.Write("\r" + new string(' ', 70) + "\r");

        if (proc.ExitCode != 0)
        {
            Console.Error.WriteLine($"[Error] Ingestion failed with exit code {proc.ExitCode}: {errorOutput.Trim()}");
            return false;
        }

        Console.WriteLine($"[✔] Ingestion complete: {dA} vs {dB} ({grandPrix} {yr} [{sess}]).");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error] Could not invoke Python: {ex.Message}");
        return false;
    }
}

void ExecuteAnalysis(string path, string dA, string dB)
{
    if (!File.Exists(path))
    {
        Console.WriteLine($"[Error] Missing synchronized telemetry file: {path}");
        return;
    }

    try
    {
        var samples = TelemetryCsvReader.Read(path);
        if (samples.Count == 0)
        {
            Console.WriteLine("[Error] Telemetry file contains no data points.");
            return;
        }

        var envelopeAnalyzer = new GearEnvelopeAnalyzer();
        var wotAnalyzer = new WotDeltaAnalyzer();
        var clippingAnalyzer = new ClippingAnalyzer();

        var envA = envelopeAnalyzer.AnalyzeDriver(samples, isDriverA: true);
        var envB = envelopeAnalyzer.AnalyzeDriver(samples, isDriverA: false);
        var deltas = wotAnalyzer.Analyze(samples);

        var clippingEvents = clippingAnalyzer.DetectClipping(samples, dA, isDriverA: true)
            .Concat(clippingAnalyzer.DetectClipping(samples, dB, isDriverA: false))
            .ToList();

        AsciiTableFormatter.PrintReport(dA, dB, envA, envB, deltas, clippingEvents);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error] Analysis failed: {ex.Message}");
    }
}

// 1. One-off mode (flags provided)
if (!isInteractive)
{
    if (autoFetch)
    {
        bool ok = await RunIngestionWithSpinner(year, gp, session, driverA, driverB, csvPath);
        if (!ok) return 1;
    }
    ExecuteAnalysis(csvPath, driverA, driverB);
    return 0;
}

// 2. Interactive REPL Mode
Console.WriteLine(AsciiTableFormatter.Banner);
Console.WriteLine("Commands: compare (fetch & analyze), analyze (cached CSV only), exit");
Console.WriteLine("================================================================================");

while (true)
{
    Console.Write("\nf1-pu> ");
    string? input = Console.ReadLine()?.Trim().ToLowerInvariant();

    if (string.IsNullOrEmpty(input)) continue;
    if (input is "exit" or "quit")
    {
        Console.WriteLine("Exiting session. Goodbye.");
        break;
    }

    if (input == "compare")
    {
        Console.Write("Enter Championship Year [default: 2026]: ");
        string? yrInput = Console.ReadLine()?.Trim();
        int targetYear = int.TryParse(yrInput, out int y) ? y : 2026;

        Console.Write("Enter Grand Prix [e.g. Monza, Spa, Silverstone] [default: Monza]: ");
        string? gpInput = Console.ReadLine()?.Trim();
        string targetGp = string.IsNullOrEmpty(gpInput) ? "Monza" : gpInput;

        Console.Write("Enter Session [FP1, FP2, FP3, Q, SQ, R] [default: Q]: ");
        string? sessInput = Console.ReadLine()?.Trim();
        string targetSession = string.IsNullOrEmpty(sessInput) ? "Q" : sessInput.ToUpperInvariant();

        Console.Write("Enter Driver A code [e.g. RUS, VER, NOR]: ");
        string? daInput = Console.ReadLine()?.Trim();
        string targetDa = string.IsNullOrEmpty(daInput) ? "RUS" : daInput.ToUpperInvariant();

        Console.Write("Enter Driver B code [e.g. ALO, PIA, HAM]: ");
        string? dbInput = Console.ReadLine()?.Trim();
        string targetDb = string.IsNullOrEmpty(dbInput) ? "PIA" : dbInput.ToUpperInvariant();

        bool success = await RunIngestionWithSpinner(targetYear, targetGp, targetSession, targetDa, targetDb, csvPath);
        if (success)
        {
            ExecuteAnalysis(csvPath, targetDa, targetDb);
        }
    }
    else if (input == "analyze")
    {
        Console.Write($"Enter CSV path [default: {csvPath}]: ");
        string? p = Console.ReadLine()?.Trim();
        string activePath = string.IsNullOrEmpty(p) ? csvPath : p;

        Console.Write("Enter Driver A label [default: RUS]: ");
        string? daLabel = Console.ReadLine()?.Trim();
        string activeDa = string.IsNullOrEmpty(daLabel) ? "RUS" : daLabel.ToUpperInvariant();

        Console.Write("Enter Driver B label [default: PIA]: ");
        string? dbLabel = Console.ReadLine()?.Trim();
        string activeDb = string.IsNullOrEmpty(dbLabel) ? "PIA" : dbLabel.ToUpperInvariant();

        ExecuteAnalysis(activePath, activeDa, activeDb);
    }
    else
    {
        Console.WriteLine($"Unknown command: '{input}'. Available: 'compare', 'analyze', 'exit'");
    }
}

return 0;