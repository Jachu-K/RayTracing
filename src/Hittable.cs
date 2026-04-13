using RayTracing;

namespace RayTracing
{
    public class hit_record {
        public Point3 p;
        public Vec3 normal;
        public material mat;
        public double t;
        public bool front_face;
        public double u;
        public double v;

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

        public virtual aabb bounding_box()
        {
            return null;
        }
    };
    class sphere : hittable
    {
        public Ray center;
        public double radius;

        public material mat;
        public aabb bbox;

        public override aabb bounding_box()
        {
            return bbox;   
        }

        // Stationary Sphere
        public sphere(Point3 static_center, double radius1, material mat1){
            center = new Ray(static_center, new Vec3(0,0,0));
            radius = Math.Max(0,radius1);
            mat = mat1;

            Point3 rvec = new Point3(radius1, radius1, radius1);
            bbox = new aabb(static_center - rvec, static_center + rvec);
        }
        
        // Moving Sphere
        public sphere(Point3 center1, Point3 center2, double radius1, material mat1)
        {
            center = new Ray(center1, center2 - center1);
            radius = double.Max(0,radius1);
            mat = mat1;

            var rvec = new Point3(radius1, radius1, radius1);
            var box1 = new aabb((center.At(0) - rvec), (center.At(0) + rvec));
            var box2 = new aabb((center.At(1) - rvec), (center.At(1) + rvec));
            bbox = new aabb(box1, box2);
        }

        public override bool hit(Ray r, Interval ray_t, ref hit_record rec) {
            /*Vec3 oc = center - r.Origin;
            var a = r.Direction.LengthSquared;
            var h = Vec3.Dot(r.Direction, oc);
            var c = oc.LengthSquared - radius*radius;*/

            Point3 current_center = center.At(r.tm);
            
            // wąskie gardło --> lekka optymalizacja
            double ocX = current_center.X - r.Origin.X;
            double ocY = current_center.Y - r.Origin.Y; 
            double ocZ = current_center.Z - r.Origin.Z;
    
            double dirX = r.Direction.X;
            double dirY = r.Direction.Y;
            double dirZ = r.Direction.Z;
    
            double a = dirX * dirX + dirY * dirY + dirZ * dirZ;
            double h = ocX * dirX + ocY * dirY + ocZ * dirZ;
            double c = ocX * ocX + ocY * ocY + ocZ * ocZ - radius * radius;
            

            double discriminant = h*h - a*c;
            if (discriminant < 0)
                return false;

            double sqrtd = double.Sqrt(discriminant);

            // Find the nearest root that lies in the acceptable range.
            var root = (h - sqrtd) / a;
            if (!ray_t.Surrounds(root)) {
                root = (h + sqrtd) / a;
                if (!ray_t.Surrounds(root))
                    return false;
            }

            rec.t = root;
            rec.p = r.At(rec.t);
            //rec.normal = (rec.p - center) / radius;
            double normalX = (rec.p.X - current_center.X) / radius;
            double normalY = (rec.p.Y - current_center.Y) / radius; 
            double normalZ = (rec.p.Z - current_center.Z) / radius;
            rec.normal = new Vec3(normalX, normalY, normalZ);
            
            
            Vec3 outward_normal = new Vec3(normalX, normalY, normalZ);
            rec.set_face_normal(r, outward_normal);
            get_sphere_uv(new Point3(outward_normal), out rec.u, out rec.v);
            rec.mat = mat;

            return true;
        }

        public static void get_sphere_uv(Point3 p, out double u, out double v) {
            // p: a given point on the sphere of radius one, centered at the origin.
            // u: returned value [0,1] of angle around the Y axis from X=-1.
            // v: returned value [0,1] of angle from Y=-1 to Y=+1.
            //     <1 0 0> yields <0.50 0.50>       <-1  0  0> yields <0.00 0.50>
            //     <0 1 0> yields <0.50 1.00>       < 0 -1  0> yields <0.50 0.00>
            //     <0 0 1> yields <0.25 0.50>       < 0  0 -1> yields <0.75 0.50>
            var pi = Math.PI;
            var theta = Math.Acos(-p.Y);
            var phi = Math.Atan2(-p.Z, p.X) + pi;

            u = phi / (2*pi);
            v = theta / pi;
        }
    };
}