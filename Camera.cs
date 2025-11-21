using System.Drawing;

namespace RayTracing;
public struct camera {
    /* Public Camera Parameters Here */
    public camera()
    {
    }

    public double aspect_ratio = 1.0;  // Ratio of image width over height
    public int image_width  = 100;  // Rendered image width in pixel count
    public int    samples_per_pixel = 10;   // Count of random samples for each pixel
    public int    max_depth         = 10;   // Maximum number of ray bounces into scene
    public double vfov = 90;        // Vertical field of view
    public Point3 lookfrom = new Point3(0,0,0);   // Point camera is looking from
    public Point3 lookat   = new Point3(0,0,-1);  // Point camera is looking at
    public Vec3   vup      = new Vec3(0,1,0);     // Camera-relative "up" direction

    public double defocus_angle = 0; // Variation angle of ray passing through each pixel
    public double focus_dist = 10; // Distance from camera to the plane with perfect focus
    
    int    image_height;   // Rendered image height
    Point3 center;         // Camera center
    Point3 pixel00_loc;    // Location of pixel 0, 0
    Vec3   pixel_delta_u;  // Offset to pixel to the right
    Vec3   pixel_delta_v;  // Offset to pixel below
    double pixel_samples_scale;  // Color scale factor for a sum of pixel samples
    private Vec3 u, v, w;  // Camera frame basis vectors
    private Vec3 defocus_disk_u; // Defocus disk horizontal radius
    private Vec3 defocus_disk_v; // ----||---- vertical radius
    
    private void initialize() {
        // Camera
        image_height = (int)(image_width / aspect_ratio);
        image_height = (image_height < 1) ? 1 : image_height;
        
        pixel_samples_scale = 1.0 / samples_per_pixel;
        center = lookfrom;
        
        //var focal_length = (lookfrom-lookat).Length;
        var theta = Program.degrees_to_radians(vfov);
        var h = Math.Tan(theta / 2);
        var viewport_height = 2.0 * h * focus_dist;
        var viewport_width = viewport_height * ((double)(image_width)/image_height);
        

        
        
        // Calculate u,v,w unit basis vectors
        w = (Vec3.UnitVector(new Vec3(lookfrom - lookat)));
        u = Vec3.UnitVector(Vec3.Cross(vup, w));
        v = Vec3.Cross(w,u);
        
        // Calculate the vectors across the horizontal and down the vertical viewport edges.
        var viewport_u = viewport_width * u;
        var viewport_v =  viewport_height * -v;

        // Calculate the horizontal and vertical delta vectors from pixel to pixel.
         pixel_delta_u = viewport_u / image_width;
         pixel_delta_v = viewport_v / image_height;

        // Calculate the location of the upper left pixel.
        var viewport_upper_left = new Vec3( center )- (focus_dist * w) - viewport_u/2 - viewport_v/2;
        var Npixel00_loc = viewport_upper_left + 0.5 * (pixel_delta_u + pixel_delta_v);
        pixel00_loc = new Point3(Npixel00_loc.X, Npixel00_loc.Y, Npixel00_loc.Z);
        
        // Calculate the defocus disk basis vectors
        var defocus_radius = focus_dist * Math.Tan(Program.degrees_to_radians(defocus_angle / 2));
        defocus_disk_u = u * defocus_radius;
        defocus_disk_v = v * defocus_radius;
    }
    
    Color ray_color(Ray r, int depth, hittable world){
        // If we've exceeded the ray bounce limit, no more light is gathered.
        if (depth <= 0)
            return new Color(0,0,0);
        hit_record rec = new hit_record();
        
        if (world.hit(r,new Interval(0.001, Double.PositiveInfinity), ref rec)) {
            
            Ray scattered = new Ray(new Point3(0,0,0), new Vec3(0,0,0));
            Color attenuation = new Color(0,0,0);
            
            if (rec.mat.scatter(r, rec, ref attenuation, ref scattered))
                return attenuation * ray_color(scattered, depth - 1, world);
            
            //return new Color(0, 0, 0);
            
            Vec3 direction = rec.normal + Vec3.random_unit_vector();
            var temp = 0.5 * ray_color(new Ray(rec.p, direction), depth-1, world);
            return new Color(temp.X, temp.Y, temp.Z);
        }
        

        Vec3 unit_direction = Vec3.UnitVector(r.Direction);
        var a = 0.5*(unit_direction.Y + 1.0);
        var res =  (1.0-a)* new Color(1.0, 1.0, 1.0) + a* new Color(0.5, 0.7, 1.0);
        return res;
    }
    
    Ray get_ray(int i, int j){
        // Construct a camera ray originating from the defocus disk and directed at randomly sampled
        // point around the pixel location i, j.

        var offset = sample_square();
        Vec3 pixel100new = new Vec3(pixel00_loc);
        var pixel_sample = pixel100new
                            + ((i + offset.X) * pixel_delta_u)
                            + ((j + offset.Y) * pixel_delta_v);

        var ray_origin = (defocus_angle <= 0) ? center : defocus_disk_sample();
        Vec3 ray_ornew = new Vec3(ray_origin);
        var ray_direction = pixel_sample - ray_ornew;
        var ray_time = RandomUtilities.RandomDouble();

        return new Ray(ray_origin, ray_direction, ray_time);
    }

    Vec3 sample_square(){
        // Returns the vector to a random point in the [-.5,-.5]-[+.5,+.5] unit square.
        return new Vec3(RandomUtilities.RandomDouble() - 0.5, RandomUtilities.RandomDouble() - 0.5, 0);
    }

    Point3 defocus_disk_sample()
    {
        // Returns a random point in the camera defocus disk
        var p = Vec3.random_in_unit_disk();
        return new Point3( new Vec3(center) + (p.X * defocus_disk_u) + (p.Y * defocus_disk_v));
    }
    public void render(hittable world) {
        initialize();

        using (StreamWriter writer = new StreamWriter("image.ppm"))
        {
            writer.WriteLine("P3");
            writer.WriteLine($"{image_width} {image_height}");
            writer.WriteLine("255");

            for (int j = 0; j < image_height; j++)
            {
                Console.WriteLine($"\rScanlines remaining: {image_height - j} ");
                for (int i = 0; i < image_width; i++)
                {
                    Color pixel_color = new Color(0, 0, 0);
                    for (int sample = 0; sample < samples_per_pixel; sample++) 
                    {
                        Ray r = get_ray(i, j);
                        pixel_color += ray_color(r, max_depth, world);
                    }

                    pixel_color *= pixel_samples_scale;
                    ColorUtilities.WriteColor(writer, pixel_color);
                }
            }
        }

        Console.WriteLine("\rDone.                 ");
    }
    
    public void render(hittable world, string filename) {
        initialize();

        using (StreamWriter writer = new StreamWriter(filename))
        {
            writer.WriteLine("P3");
            writer.WriteLine($"{image_width} {image_height}");
            writer.WriteLine("255");

            for (int j = 0; j < image_height; j++)
            {
                Console.WriteLine($"\rScanlines remaining: {image_height - j} ");
                for (int i = 0; i < image_width; i++)
                {
                    Color pixel_color = new Color(0, 0, 0);
                    for (int sample = 0; sample < samples_per_pixel; sample++) 
                    {
                        Ray r = get_ray(i, j);
                        pixel_color += ray_color(r, max_depth, world);
                    }

                    pixel_color *= pixel_samples_scale;
                    ColorUtilities.WriteColor(writer, pixel_color);
                }
            }
        }

        Console.WriteLine("\rDone.                 ");
    }
}