using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace TerritoryServer.Utils;

[Injectable(InjectionType.Singleton)]
public class TerritoryMath(MathUtil mathUtil)
{
    public static int Wrap(int x, int min, int max)
    {
        return ((x - min) % (max - min) + (max - min)) % (max - min) + min;
    }

    public double MapToRangeInverted(double x, double minIn, double maxIn, double minOut, double maxOut)
    {
        return maxOut - mathUtil.MapToRange(x, minIn, maxIn, minOut, maxOut);
    }
}