using System.Drawing;
using System.Numerics;

namespace RayTracing;

class bvh_node : hittable {
    
    public hittable left;
    public hittable right;
    public aabb bbox;
    public bvh_node(hittable_list list) 
        : this(new List<hittable>(list.objects), 0, list.objects.Count)
    {
        // Kopiowanie listy gwarantuje, że oryginalna lista nie zostanie zmodyfikowana
        // Kopia istnieje tylko w czasie życia tego konstruktora, co jest OK
    }
    
    public bvh_node(List<hittable> objects, int start, int end)
    {
        // Główna logika budowania BVH
        int axis = (int)RandomUtilities.random_int(0, 2);

        Comparison<hittable> comparator = axis switch
        {
            0 => box_x_compare,
            1 => box_y_compare,
            _ => box_z_compare
        };

        int object_span = end - start;

        if (object_span == 1)
        {
            left = right = objects[start];
        }else if (object_span == 2)
        {
            left = objects[start];
            right = objects[start + 1];
        }
        else
        {
            objects.Sort(start, object_span, Comparer<hittable>.Create(comparator));
            var mid = start + object_span / 2;
            left = new bvh_node(objects, start, mid);
            right = new bvh_node(objects, mid, end);
        }

        bbox = new aabb(left.bounding_box(), right.bounding_box());
    }
    
    public static bool box_compare(
    hittable a, hittable b, int axis_index
    ) {
        var a_axis_interval = a.bounding_box().axis_interval(axis_index);
        var b_axis_interval = b.bounding_box().axis_interval(axis_index);
        return a_axis_interval.Min < b_axis_interval.Min;
    }

    static int box_x_compare(hittable a, hittable b) {
        return box_compare(a, b, 0) ? -1 : 1;
    }

    static int box_y_compare(hittable a, hittable b) {  
        return box_compare(a, b, 1) ? -1 : 1;
    }

    static int box_z_compare(hittable a, hittable b) {
        return box_compare(a, b, 2) ? -1 : 1;
    }
    public override bool hit(Ray r, Interval ray_t, ref hit_record rec){
        if (!bbox.hit(r, ray_t))
            return false;

        bool hit_left = left.hit(r, ray_t, ref rec);
        bool hit_right = right.hit(r, new Interval(ray_t.Min, hit_left ? rec.t : ray_t.Max), ref rec);

        return hit_left || hit_right;
    }

    public override aabb bounding_box(){ return bbox; }
    
};
