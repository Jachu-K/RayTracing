namespace RayTracing
{
    public struct ColorUtilities
    {
        public static double linear_to_gamma(double linear_component)
        {
            if (linear_component > 0)
                return double.Sqrt(linear_component);

            return 0;
        }
        public static void WriteColor(StreamWriter writer, Color pixelColor)
        {
            double r = pixelColor.X;
            double g = pixelColor.Y;
            double b = pixelColor.Z;

            // Apply gamma correction
            r = linear_to_gamma(r);
            g = linear_to_gamma(g);
            b = linear_to_gamma(b);

            // Translate to [0,255]
            var intensity = new Interval(0.000, 0.999);
            int rByte = (int)(256 * intensity.Clamp(r));
            int gByte = (int)(256 * intensity.Clamp(g));
            int bByte = (int)(256 * intensity.Clamp(b));

            writer.WriteLine($"{rByte} {gByte} {bByte}");
        }

        /*private static double LinearToGamma(double linearComponent)
        {
            return linearComponent > 0 ? Math.Sqrt(linearComponent) : 0;
        }*/
    }
}