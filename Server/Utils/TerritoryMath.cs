namespace TerritoryServer.Utils;

public static class TerritoryMath
{
    public static int Wrap(int x, int min, int max)
    {
        return ((x - min) % (max - min) + (max - min)) % (max - min) + min;
    }
}