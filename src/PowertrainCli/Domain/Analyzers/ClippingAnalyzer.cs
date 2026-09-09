using PowertrainCli.Domain.Models;

namespace PowertrainCli.Domain.Analyzers;

public class ClippingAnalyzer
{
    public IReadOnlyList<ClippingEvent> DetectClipping(IReadOnlyList<TelemetrySample> samples, string driverCode, bool isDriverA, double speedDropThreshold = 2.5)
    {
        var events = new List<ClippingEvent>();
        int i = 0;

        while (i < samples.Count)
        {
            double throttle = isDriverA ? samples[i].ThrottleA : samples[i].ThrottleB;
            if (throttle >= 98.0)
            {
                int startIdx = i;
                double maxSpeed = isDriverA ? samples[i].SpeedA : samples[i].SpeedB;

                while (i < samples.Count && (isDriverA ? samples[i].ThrottleA : samples[i].ThrottleB) >= 98.0)
                {
                    double currentSpeed = isDriverA ? samples[i].SpeedA : samples[i].SpeedB;
                    if (currentSpeed > maxSpeed)
                        maxSpeed = currentSpeed;
                    
                    double drop = maxSpeed - currentSpeed;
                    if (drop >= speedDropThreshold)
                    {
                        events.Add(new ClippingEvent(
                            Driver: driverCode,
                            StartDistanceMeters: Math.Round(samples[startIdx].Distance, 1),
                            EndDistanceMeters: Math.Round(samples[i].Distance, 1),
                            SpeedDropKmh: Math.Round(drop, 2)
                        ));
                        break;
                    }
                    i++;
                }
            }
            i++;
        }
        return events;
    }
}