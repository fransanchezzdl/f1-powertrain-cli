namespace PowertrainCli.Domain.Models;

public record TelemetrySample(
    double Distance,
    double SpeedA,
    double SpeedB,
    int RpmA,
    int RpmB,
    int GearA,
    int GearB,
    double ThrottleA,
    double ThrottleB,
    int DrsA,
    int DrsB
);