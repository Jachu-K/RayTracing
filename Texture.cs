namespace RayTracing;


public class texture
{

    public virtual Color value(double u, double v, Point3 p)
    {
        return new Color();
    }
}

public class solid_color : texture {
    public solid_color(Color albedo1)
    {
        albedo = albedo1;
    }

    public solid_color(double red, double green, double blue)
    {
        new solid_color(new Color(red, green, blue));
    }

    public override Color value(double u, double v, Point3 p){
        return albedo;
    }
    public Color albedo;
};

public class checker_texture : texture {
    public double inv_scale;
    public texture even;
    public texture odd;
    public checker_texture(double scale, texture even1, texture odd1)
    {
        inv_scale = (1.0 / scale);
        even = even1;
        odd = odd1;
    }

    public checker_texture(double scale, Color c1, Color c2)
    {
        new checker_texture(scale, new solid_color(c1), new solid_color(c2));
    }

    public override Color value(double u, double v, Point3 p){
        var xInteger = (int)(Math.Floor(inv_scale * p.X));
        var yInteger = (int)(Math.Floor(inv_scale * p.Y));
        var zInteger = (int)(Math.Floor(inv_scale * p.Z));

        bool isEven = (xInteger + yInteger + zInteger) % 2 == 0;

        return isEven ? even.value(u, v, p) : odd.value(u, v, p);
    }
};