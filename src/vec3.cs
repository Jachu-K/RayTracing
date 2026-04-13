namespace RayTracing    
{
    public struct Vec3
    {
        public double X;
        public double Y;
        public double Z;

        public Vec3(Point3 point)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }
        public Vec3() : this(0, 0, 0) { }
        public Vec3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        

        // Operator przeciążenia
        public static Vec3 operator -(Vec3 v) => new Vec3(-v.X, -v.Y, -v.Z);
        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator *(Vec3 v, double t) => new Vec3(v.X * t, v.Y * t, v.Z * t);
        public static Vec3 operator *(double t, Vec3 v) => v * t;
        public static Vec3 operator *(Vec3 a, Vec3 b) => new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vec3 operator /(Vec3 v, double t) => v * (1 / t);

        // Właściwości
        public double Length => Math.Sqrt(LengthSquared);
        public double LengthSquared => X * X + Y * Y + Z * Z;
        
        public bool near_zero(){
            // Return true if the vector is close to zero in all dimensions.
            var s = 1e-8;
            return (double.Abs(X) < s) && (double.Abs(Y) < s) && (double.Abs(Z) < s);
        }

        // Metody statyczne
        
        public static Vec3 random() {
            return new Vec3(RandomUtilities.RandomDouble(), RandomUtilities.RandomDouble(), RandomUtilities.RandomDouble());
        }

        public static Vec3 random(double min, double max) {
            return new Vec3(RandomUtilities.RandomDouble(min, max), RandomUtilities.RandomDouble(min, max), RandomUtilities.RandomDouble(min, max));
        }
        public static double Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Vec3 Cross(Vec3 a, Vec3 b) => new Vec3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
        public static Vec3 UnitVector(Vec3 v) => v / v.Length;
        
        public static Vec3 random_in_unit_disk()
        {
            while (true)
            {
                var p = new Vec3(RandomUtilities.RandomDouble(-1, 1), RandomUtilities.RandomDouble(-1, 1), 0);
                if (p.LengthSquared < 1)
                {
                    return p;
                }
            }
        }
        
        public static Vec3 random_unit_vector() {
            while (true) {
                var p = Vec3.random(-1,1);
                var lensq = p.LengthSquared;
                if (1e-60 < lensq && lensq <= 1)
                    return p / double.Sqrt(lensq);
            }
        }
        
        public static Vec3 random_on_hemisphere(Vec3 normal) {
            Vec3 on_unit_sphere = random_unit_vector();
            if (Dot(on_unit_sphere, normal) > 0.0) // In the same hemisphere as the normal
                return on_unit_sphere;
            else
                return -on_unit_sphere;
        }
        public static Vec3 reflect(Vec3 v, Vec3 n) {
            return v - 2*Dot(v,n)*n;
        }
        
        public static Vec3 refract(Vec3 uv, Vec3 n, double etai_over_etat) {
            var cos_theta = double.Min(Vec3.Dot(-uv, n), 1.0);
            Vec3 r_out_perp =  etai_over_etat * (uv + cos_theta*n);
            Vec3 r_out_parallel = -double.Sqrt(double.Abs(1.0 - r_out_perp.LengthSquared)) * n;
            return r_out_perp + r_out_parallel;
        }
        
        public override string ToString() => $"{X} {Y} {Z}";
    }

    // Aliasy dla lepszej czytelności
    public struct Point3
    {
        public double X;
        public double Y;
        public double Z;

        public Point3()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
        public Point3(Vec3 v) : this(v.X, v.Y, v.Z) { }

        public Point3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public double Length => Math.Sqrt(LengthSquared);
        public double LengthSquared => X * X + Y * Y + Z * Z;

        public static Point3 operator -(Point3 a, Point3 b)
        {
            return new Point3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static Point3 operator +(Point3 a, Point3 b)
        {
            return new Point3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
    }

    public struct Color
    {
        public double X;
        public double Y;
        public double Z;

        public Color()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Color(double r, double g, double b)
        {
            X = r;
            Y = g;
            Z = b;
        }
        
        // Konwersja z Vec3 na Color
        public Color(Vec3 v) : this(v.X, v.Y, v.Z) { }

        // Operator mnożenia - akceptuje Vec3 i zwraca Color
        public static Color operator *(Color a, Vec3 b)
        {
            return new Color(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Color operator *(Vec3 a, Color b)
        {
            return new Color(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        
        public static Color operator *(Color a, Color b)
        {
            return new Color(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        // Operator mnożenia przez skalar (Color * double)
        public static Color operator *(Color c, double t)
        {
            return new Color(c.X * t, c.Y * t, c.Z * t);
        }

        // Operator mnożenia przez skalar (double * Color)
        public static Color operator *(double t, Color c)
        {
            return c * t;
        }
        
        public static Color operator +(Color a, Color b)
        {
            return new Color(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Color operator /(Color c, double t)
        {
            return c * (1.0 / t);
        }
        
        public static Color random(double min, double max) {
            return new Color(RandomUtilities.RandomDouble(min, max), RandomUtilities.RandomDouble(min, max), RandomUtilities.RandomDouble(min, max));
        }
        public static Color random() {
            return new Color(RandomUtilities.RandomDouble(), RandomUtilities.RandomDouble(), RandomUtilities.RandomDouble());
        }

    }
}