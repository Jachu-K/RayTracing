namespace RayTracing;
public static class RandomUtilities
{
    private static Random random = new Random();
    
    public static double RandomDouble()
    {
        return random.NextDouble();
    }

    public static double RandomDouble(double min, double max)
    {
        return min + (max - min) * random.NextDouble();
    }
}