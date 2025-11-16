using RayTracing;

namespace RayTracing
{
    public class hit_record {
        public Point3 p;
        public Vec3 normal;
        public material mat;
        public double t;
        public bool front_face;

        public void set_face_normal(Ray r, Vec3 outward_normal) {
            // Sets the hit record normal vector.
            // NOTE: the parameter `outward_normal` is assumed to have unit length.

            front_face = Vec3.Dot(r.Direction, outward_normal) < 0;
            normal = front_face ? outward_normal : -outward_normal;
        }
    };

    public class hittable
    {
        public virtual bool hit(Ray r, Interval ray_t, ref hit_record rec)
        {
            return false;
        }
    };
    class sphere : hittable {
        private Point3 center { get; }
        private double radius { get; }

        public material mat;
        
        public sphere(Point3 center1, double radius1, material mat1){
            center = center1;
            radius = radius1;
            mat = mat1;
        }

        public override bool hit(Ray r, Interval ray_t, ref hit_record rec) {
            Vec3 oc = center - r.Origin;
            var a = r.Direction.LengthSquared;
            var h = Vec3.Dot(r.Direction, oc);
            var c = oc.LengthSquared - radius*radius;

            var discriminant = h*h - a*c;
            if (discriminant < 0)
                return false;

            var sqrtd = double.Sqrt(discriminant);

            // Find the nearest root that lies in the acceptable range.
            var root = (h - sqrtd) / a;
            if (!ray_t.Surrounds(root)) {
                root = (h + sqrtd) / a;
                if (!ray_t.Surrounds(root))
                    return false;
            }

            rec.t = root;
            rec.p = r.At(rec.t);
            rec.normal = (rec.p - center) / radius;
            
            Vec3 outward_normal = (rec.p - center) / radius;
            rec.set_face_normal(r, outward_normal);
            rec.mat = mat;

            return true;
        }

        
    };
}