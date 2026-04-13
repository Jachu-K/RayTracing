namespace RayTracing;

public class aabb {
    public Interval x, y, z;

    public aabb() {} // The default AABB is empty, since intervals are empty by default.

    public aabb(Interval x, Interval y, Interval z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public aabb(Point3 a, Point3 b) {
        // Treat the two points a and b as extrema for the bounding box, so we don't require a
        // particular minimum/maximum coordinate order.

        x = (a.X <= b.X) ? new Interval(a.X, b.X) : new Interval(b.X, a.X);
        y = (a.Y <= b.Y) ? new Interval(a.Y, b.Y) : new Interval(b.Y, a.Y);
        z = (a.Z <= b.Z) ? new Interval(a.Z, b.Z) : new Interval(b.Z, a.Z);
    }
    
    public aabb(aabb box0, aabb box1) {
        x = new Interval(box0.x, box1.x);
        y = new Interval(box0.y, box1.y);
        z = new Interval(box0.z, box1.z);
    }

     public Interval axis_interval(int n){
        if (n == 1) return y;
        if (n == 2) return z;
        return x;
    }

    public bool hit(Ray r, Interval ray_t)
    {
        double t_min = ray_t.Min;
        double t_max = ray_t.Max;
        Point3 ray_orig = r.Origin;
        Vec3 ray_dir = r.Direction;

        // Check X axis
        double invD_x = 1.0 / ray_dir.X;
        double t0_x = (x.Min - ray_orig.X) * invD_x;
        double t1_x = (x.Max - ray_orig.X) * invD_x;
        if (invD_x < 0.0) { double temp = t0_x; t0_x = t1_x; t1_x = temp; }
        t_min = Math.Max(t0_x, t_min);
        t_max = Math.Min(t1_x, t_max);
        if (t_max <= t_min) return false;

        // Check Y axis
        double invD_y = 1.0 / ray_dir.Y;
        double t0_y = (y.Min - ray_orig.Y) * invD_y;
        double t1_y = (y.Max - ray_orig.Y) * invD_y;
        if (invD_y < 0.0) { double temp = t0_y; t0_y = t1_y; t1_y = temp; }
        t_min = Math.Max(t0_y, t_min);
        t_max = Math.Min(t1_y, t_max);
        if (t_max <= t_min) return false;

        // Check Z axis
        double invD_z = 1.0 / ray_dir.Z;
        double t0_z = (z.Min - ray_orig.Z) * invD_z;
        double t1_z = (z.Max - ray_orig.Z) * invD_z;
        if (invD_z < 0.0) { double temp = t0_z; t0_z = t1_z; t1_z = temp; }
        t_min = Math.Max(t0_z, t_min);
        t_max = Math.Min(t1_z, t_max);
        if (t_max <= t_min) return false;

        return true;
    }
};