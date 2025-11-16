namespace RayTracing
{
    public class Ray
    {
        public Point3 Origin { get; }
        public Vec3 Direction { get; }

        public Ray(Point3 origin, Vec3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Point3 At(double t)
        {
            Vec3 result = Origin + Direction * t;
            return new Point3(result.X, result.Y, result.Z);
        }
    }
}