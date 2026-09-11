namespace TerritoryServer.Utils;

public static class TerritoryMath
{
    public static int Wrap(int x, int min, int max)
    {
        if (x > min && x < max)
            return x;
        
        return ((x - min) % (max - min) + (max - min)) % (max - min) + min;
    }
}