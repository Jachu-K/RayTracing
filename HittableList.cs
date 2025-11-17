using System.Collections.Generic;

namespace RayTracing
{
    public class hittable_list : hittable
    {
        public aabb bbox;
        public List<hittable> objects = new List<hittable>();

        public hittable_list() {}
        public hittable_list(hittable obj) { add(obj); }

        public void clear() { objects.Clear(); }

        public void add(hittable obj) {
            objects.Add(obj);
            if (bbox == null)
            {
                bbox = obj.bounding_box();
            }
            else
            {
                bbox = new aabb(bbox, obj.bounding_box());
            }
        }

        public override bool hit(Ray r, Interval ray_t, ref hit_record rec) {
            hit_record temp_rec = new hit_record();
            bool hit_anything = false;
            var closest_so_far = ray_t.Max;

            foreach (var obj in objects) {
                if (obj.hit(r, new Interval(ray_t.Min,closest_so_far), ref temp_rec)) {
                    hit_anything = true;
                    closest_so_far = temp_rec.t;
                    rec = temp_rec;
                }
            }

            return hit_anything;
        }
    }
}