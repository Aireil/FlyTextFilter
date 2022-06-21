using System;

namespace FlyTextFilter.Model;

[Flags]
public enum FlyTextTargets
{
    None = 0,
    You = 1 << 0,
    Party = 1 << 1,
    Others = 1 << 2,
    All = ~(-1 << 3),
}
