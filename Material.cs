namespace RayTracing;

public class material { 

    public virtual bool scatter(
    Ray r_in, hit_record rec, ref Color attenuation, ref Ray scattered
        ){
        return false;
    }
};

class lambertian : material
{
    private Color albedo;

    public lambertian(Color albedo)
    {
        this.albedo = albedo;
    } 

    public override bool scatter(Ray r_in, hit_record rec, ref Color attenuation, ref Ray scattered)
     {
        var scatter_direction = rec.normal + Vec3.random_unit_vector();
        
        // Catch degenerate scatter direction
        if (scatter_direction.near_zero())
            scatter_direction = rec.normal;
        
        scattered = new Ray(rec.p, scatter_direction);
        attenuation = albedo;
        return true;
    }
};


class metal : material
{
    private Color albedo;
    private double fuzz;
    public metal(Color albedo, double fuzz)
    {
        this.albedo = albedo;
        this.fuzz = fuzz;
    }

    public override bool scatter(Ray r_in, hit_record rec, ref Color attenuation, ref Ray scattered){
        Vec3 reflected = Vec3.reflect(r_in.Direction, rec.normal);
        reflected = Vec3.UnitVector(reflected) + (fuzz * Vec3.random_unit_vector());
        scattered = new Ray(rec.p, reflected);
        attenuation = albedo;
        return (Vec3.Dot(scattered.Direction, rec.normal) > 0);
    }
};

class dielectric : material {
    private double refraction_index;

    public dielectric(double refraction_index)
    {
        this.refraction_index = refraction_index;
    } 
    
    public static double reflectance(double cosine, double refraction_index) {
        // Use Schlick's approximation for reflectance.
        var r0 = (1 - refraction_index) / (1 + refraction_index);
        r0 = r0*r0;
        return r0 + (1-r0)*double.Pow((1 - cosine),5);
    }

    public override bool scatter(Ray r_in, hit_record rec, ref Color attenuation, ref Ray scattered){
        attenuation = new Color(1.0, 1.0, 1.0);
        double ri = rec.front_face ? (1.0/refraction_index) : refraction_index;

        Vec3 unit_direction = Vec3.UnitVector(r_in.Direction);
        
        double cos_theta = double.Min(Vec3.Dot(-unit_direction, rec.normal), 1.0);
        double sin_theta = double.Sqrt(1.0 - cos_theta*cos_theta);

        bool cannot_refract = ri * sin_theta > 1.0;
        Vec3 direction;

        if (cannot_refract || reflectance(cos_theta, ri) > RandomUtilities.RandomDouble())
            direction = Vec3.reflect(unit_direction, rec.normal);
        else
            direction = Vec3.refract(unit_direction, rec.normal, ri);

        scattered = new Ray(rec.p, direction);
        
        return true;
    }
    // Refractive index in vacuum or air, or the ratio of the material's refractive index over
    // the refractive index of the enclosing media
   };