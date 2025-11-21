namespace RayTracing;
public class triangle : hittable
{
    private Point3 v0, v1, v2;
    private material mat;
    private aabb bbox;
    private Vec3 normal; // Przechowujemy normalną

    public triangle(Point3 v0, Point3 v1, Point3 v2, material mat, string name = "")
    {
        this.v0 = v0;
        this.v1 = v1;
        this.v2 = v2;
        this.mat = mat;
    
        // Oblicz normalną
        Vec3 edge1 = new Vec3(v1 - v0);
        Vec3 edge2 = new Vec3(v2 - v0);
        this.normal = Vec3.UnitVector(Vec3.Cross(edge1, edge2));
    
        // Debug: wypisz informacje o trójkącie
        Console.WriteLine($"Triangle '{name}':");
        Console.WriteLine($"  Vertices: {v0} -> {v1} -> {v2}");
        Console.WriteLine($"  Normal: {normal}");
    
        // Oblicz AABB
        Point3 min = new Point3(
            Math.Min(Math.Min(v0.X, v1.X), v2.X),
            Math.Min(Math.Min(v0.Y, v1.Y), v2.Y),
            Math.Min(Math.Min(v0.Z, v1.Z), v2.Z)
        );
        Point3 max = new Point3(
            Math.Max(Math.Max(v0.X, v1.X), v2.X),
            Math.Max(Math.Max(v0.Y, v1.Y), v2.Y),
            Math.Max(Math.Max(v0.Z, v1.Z), v2.Z)
        );
        bbox = new aabb(min, max);
    }

    public override bool hit(Ray r, Interval ray_t, ref hit_record rec)
    {
        // Algorytm Möller–Trumbore
        Vec3 edge1 = new Vec3(v1 - v0);
        Vec3 edge2 = new Vec3(v2 - v0);
        Vec3 h = Vec3.Cross(r.Direction, edge2);
        double a = Vec3.Dot(edge1, h);
        
        if (a > -1e-8 && a < 1e-8)
            return false; // Promień równoległy do trójkąta
            
        double f = 1.0 / a;
        Vec3 s = new Vec3(r.Origin - v0);
        double u = f * Vec3.Dot(s, h);
        
        if (u < 0.0 || u > 1.0)
            return false;
            
        Vec3 q = Vec3.Cross(s, edge1);
        double v = f * Vec3.Dot(r.Direction, q);
        
        if (v < 0.0 || u + v > 1.0)
            return false;
            
        double t = f * Vec3.Dot(edge2, q);
        
        if (!ray_t.Contains(t))
            return false;
            
        rec.t = t;
        rec.p = r.At(t);
        rec.normal = normal; // Używamy pre-obliczonej normalnej
        rec.mat = mat;
        rec.set_face_normal(r, rec.normal);
        
        return true;
    }

    public override aabb bounding_box() => bbox;
}

public class two_sided_triangle : hittable
{
    private triangle front_face;
    private triangle back_face;
    private aabb bbox;

    public two_sided_triangle(Point3 v0, Point3 v1, Point3 v2, material mat, string name = "")
    {
        // Przód - oryginalna kolejność
        this.front_face = new triangle(v0, v1, v2, mat);
        
        // Tył - odwrócona kolejność wierzchołków
        this.back_face = new triangle(v0, v2, v1, mat);
        
        this.bbox = front_face.bounding_box();
        
        Console.WriteLine($"Two-sided triangle '{name}' created");
    }

    public override bool hit(Ray r, Interval ray_t, ref hit_record rec)
    {
        hit_record temp_rec = new hit_record();
        bool hit_anything = false;
        double closest_so_far = ray_t.Max;

        // Sprawdź przód
        if (front_face.hit(r, new Interval(ray_t.Min, closest_so_far), ref temp_rec))
        {
            hit_anything = true;
            closest_so_far = temp_rec.t;
            rec = temp_rec;
        }

        // Sprawdź tył
        if (back_face.hit(r, new Interval(ray_t.Min, closest_so_far), ref temp_rec))
        {
            hit_anything = true;
            closest_so_far = temp_rec.t;
            rec = temp_rec;
        }

        return hit_anything;
    }

    public override aabb bounding_box() => bbox;
}

public class textured_triangle : hittable
{
    private Point3 v0, v1, v2;
    private Vec3 uv0, uv1, uv2;
    private material mat;
    private aabb bbox;
    private Vec3 normal_front;  // Normalna dla przodu
    private Vec3 normal_back;   // Normalna dla tyłu

    public textured_triangle(Point3 v0, Point3 v1, Point3 v2, Vec3 uv0, Vec3 uv1, Vec3 uv2, material mat)
    {
        this.v0 = v0;
        this.v1 = v1;
        this.v2 = v2;
        this.uv0 = uv0;
        this.uv1 = uv1;
        this.uv2 = uv2;
        this.mat = mat;
        
        // Oblicz normalną z geometrii (dla przodu)
        Vec3 edge1 = new Vec3(v1 - v0);
        Vec3 edge2 = new Vec3(v2 - v0);
        this.normal_front = Vec3.UnitVector(Vec3.Cross(edge1, edge2));
        this.normal_back = -normal_front;  // Normalna dla tyłu jest przeciwna
        
        // Oblicz AABB
        Point3 min = new Point3(
            Math.Min(Math.Min(v0.X, v1.X), v2.X),
            Math.Min(Math.Min(v0.Y, v1.Y), v2.Y),
            Math.Min(Math.Min(v0.Z, v1.Z), v2.Z)
        );
        Point3 max = new Point3(
            Math.Max(Math.Max(v0.X, v1.X), v2.X),
            Math.Max(Math.Max(v0.Y, v1.Y), v2.Y),
            Math.Max(Math.Max(v0.Z, v1.Z), v2.Z)
        );
        bbox = new aabb(min, max);

        //Console.WriteLine($"Created textured triangle:");
        //Console.WriteLine($"  Normal front: {normal_front}");
        //Console.WriteLine($"  Normal back: {normal_back}");
        //Console.WriteLine($"  Points: ({v0.X}, {v0.Y}, {v0.Z}), ({v1.X}, {v1.Y}, {v1.Z}), ({v2.X}, {v2.Y}, {v2.Z})");
    }

    public override bool hit(Ray r, Interval ray_t, ref hit_record rec)
    {
        // Algorytm Möller–Trumbore
        Vec3 edge1 = new Vec3(v1 - v0);
        Vec3 edge2 = new Vec3(v2 - v0);
        Vec3 h = Vec3.Cross(r.Direction, edge2);
        double a = Vec3.Dot(edge1, h);
        
        // Jeśli a jest bliskie 0, promień jest równoległy do trójkąta
        if (a > -1e-8 && a < 1e-8)
            return false;
            
        double f = 1.0 / a;
        Vec3 s = new Vec3(r.Origin - v0);
        double u = f * Vec3.Dot(s, h);
        
        if (u < 0.0 || u > 1.0)
            return false;
            
        Vec3 q = Vec3.Cross(s, edge1);
        double v = f * Vec3.Dot(r.Direction, q);
        
        if (v < 0.0 || u + v > 1.0)
            return false;
            
        double t = f * Vec3.Dot(edge2, q);
        
        if (!ray_t.Contains(t))
            return false;

        // Interpolacja współrzędnych tekstury
        double w = 1.0 - u - v;
        double texU = w * uv0.X + u * uv1.X + v * uv2.X;
        double texV = w * uv0.Y + u * uv1.Y + v * uv2.Y;
            
        // Uzupełnij rekord trafienia
        rec.t = t;
        rec.p = r.At(t);
        rec.mat = mat;
        rec.u = texU;
        rec.v = texV;
        
        // DETERMINACJA STRONY TRÓJKĄTA
        // Sprawdź z której strony promień trafił trójkąt
        double dot = Vec3.Dot(normal_front, r.Direction);
        
        if (dot < 0) 
        {
            // Promień trafił w PRZÓD trójkąta (normalna i promień w przeciwnych kierunkach)
            rec.normal = normal_front;
        }
        else 
        {
            // Promień trafił w TYŁ trójkąta (normalna i promień w tym samym kierunku)
            rec.normal = normal_back;
        }
        
        rec.set_face_normal(r, rec.normal);
        
        return true;
    }

    public override aabb bounding_box() => bbox;

    public static textured_triangle FromVerticesOnly(Point3 v0, Point3 v1, Point3 v2, material mat)
    {
        Vec3 uv0 = new Vec3(0, 0, 0);
        Vec3 uv1 = new Vec3(1, 0, 0);
        Vec3 uv2 = new Vec3(0, 1, 0);
        
        return new textured_triangle(v0, v1, v2, uv0, uv1, uv2, mat);
    }
}