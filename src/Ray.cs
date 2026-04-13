namespace RayTracing
{
    public struct Ray
    {
        public Point3 Origin;
        public Vec3 Direction;
        public double tm;

        public Ray(Point3 origin, Vec3 direction, double time)
        {
            Origin = origin;
            Direction = direction;
            tm = time;
        }
        
        public Ray(Point3 origin, Vec3 direction)
        {
            Origin = origin;
            Direction = direction;
            tm = 0;
        }
        public Ray(Point3 origin, Point3 direction)
        {
            Origin = origin;
            Direction = new Vec3(direction);
            tm = 0;
        }

        public Point3 At(double t)
        {
            return new Point3(
                Origin.X + Direction.X * t,
                Origin.Y + Direction.Y * t, 
                Origin.Z + Direction.Z * t
            );
        }
    }
}