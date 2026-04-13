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
        : this(new Color(red, green, blue)) // Użyj this() aby wywołać inny konstruktor
    {
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
        : this(scale, new solid_color(c1), new solid_color(c2))
    {
    }

    public override Color value(double u, double v, Point3 p){
        var xInteger = (int)(Math.Floor(inv_scale * p.X));
        var yInteger = (int)(Math.Floor(inv_scale * p.Y));
        var zInteger = (int)(Math.Floor(inv_scale * p.Z));

        bool isEven = (xInteger + yInteger + zInteger) % 2 == 0;

        return isEven ? even.value(u, v, p) : odd.value(u, v, p);
    }
};

public class image_texture : texture
{
    public image_texture(string filename) : base()
    {
        Console.WriteLine($"Loading texture: {filename}");
        image = new rtw_image(filename);
        Console.WriteLine($"Texture loaded: {image.width()} x {image.height()}");
    }

    public override Color value(double u, double v, Point3 p)
    {
        // If we have no texture data, then return solid cyan as a debugging aid.
        if (image.height() <= 0)
        {
            Console.WriteLine("No texture data - returning cyan");
            return new Color(0, 1, 1);
        }

        // Clamp input texture coordinates to [0,1] x [1,0]
        u = new Interval(0,1).Clamp(u);
        v = 1.0 - new Interval(0,1).Clamp(v);  // Flip V to image coordinates

        int i = (int)(u * image.width());
        int j = (int)(v * image.height());
        
        // Debug: print coordinates and pixel indices
        if (u < 0.1 && v < 0.1) // Print only for first few pixels to avoid spam
        {
            Console.WriteLine($"u={u:F3}, v={v:F3} -> i={i}, j={j}");
        }

        byte[] pixel = image.pixel_data(i, j);

        // Debug: print pixel values
        if (u < 0.1 && v < 0.1)
        {
            Console.WriteLine($"Pixel RGB: {pixel[0]}, {pixel[1]}, {pixel[2]}");
        }

        double color_scale = 1.0 / 255.0;
        Color result = new Color(color_scale * pixel[0], color_scale * pixel[1], color_scale * pixel[2]);
        
        if (u < 0.1 && v < 0.1)
        {
            Console.WriteLine($"Final color: {result.X:F3}, {result.Y:F3}, {result.Z:F3}");
        }

        return result;
    }

    private rtw_image image;
}