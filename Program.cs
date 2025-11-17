using System;
using RayTracing;



class Program
{
    public static double degrees_to_radians(double degrees) {
        return degrees * Math.PI / 180.0;
    }

    static void Main()
    {
        switch (2) {
            case 1: spheres();  break;
            case 2: checkered_spheres(); break;
        }
    }
    static void spheres()
    {

        hittable_list world = new hittable_list();
        
        //var ground_material = new lambertian(new Color(0.5, 0.5, 0.5));
       //world.add(new sphere(new Point3(0,-1000,0), 1000, ground_material));

       var checker = new checker_texture(0.32, new Color(.2, .3, .1), new Color(.9, .9, .9));
       world.add(new sphere(new Point3(0,-1000,0), 1000, new lambertian(checker)));
       
        for (int a = -11; a < 11; a++) {
            for (int b = -11; b < 11; b++) {
                var choose_mat = RandomUtilities.RandomDouble();
                Point3 center = new Point3(a + 0.9*RandomUtilities.RandomDouble(), 0.2, b + 0.9*RandomUtilities.RandomDouble());

                if ((center - new Point3(4, 0.2, 0)).Length > 0.9) {
                    material sphere_material;

                    if (choose_mat < 0.8) {
                        // diffuse
                        var albedo = Color.random() * Color.random();
                        sphere_material = new lambertian(new Color(albedo));
                        var temp = center + new Vec3(0, RandomUtilities.RandomDouble(0,.5), 0);
                        Point3 center2 = new Point3(temp.X, temp.Y, temp.Z);
                        //world.add(new sphere(center,center2, 0.2, sphere_material));
                        world.add(new sphere(center, 0.2, sphere_material));
                    } else if (choose_mat < 0.95) {
                        // metal
                        var albedo = Color.random(0.5,1);
                        var fuzz = RandomUtilities.RandomDouble(0, 0.5);
                        sphere_material = new metal(new Color(albedo), fuzz);
                        world.add(new sphere(center, 0.2, sphere_material));
                    } else {
                        // glass
                        sphere_material = new dielectric(1.5);
                        world.add(new sphere(center, 0.2, sphere_material));
                    }
                }
            }
        }

        var material1 = new dielectric(1.5);
        world.add(new sphere(new Point3(0, 1, 0), 1.0, material1));

        var material2 = new lambertian(new Color(0.4, 0.2, 0.1));
        world.add(new sphere(new Point3(-4, 1, 0), 1.0, material2));

        var material3 = new metal(new Color(0.7, 0.6, 0.5), 0.0);
        world.add(new sphere(new Point3(4, 1, 0), 1.0, material3));
        
        world = new hittable_list(new bvh_node(world));

        camera cam = new camera();

        cam.aspect_ratio      = 16.0 / 9.0;
        cam.image_width       = 1200;
        cam.samples_per_pixel = 150;
        cam.max_depth         = 50;

        //cam.image_width = 200;
        //cam.samples_per_pixel = 1; 
        //cam.max_depth = 2;
        
        
        cam.vfov     = 20;
        cam.lookfrom = new Point3(13,2,3);
        cam.lookat   = new Point3(0,0,0);
        cam.vup      = new Vec3(0,1,0);

        cam.defocus_angle = 0.6;
        cam.focus_dist    = 10.0;
        
        cam.render(world);

    }
    static void checkered_spheres() {
        hittable_list world = new hittable_list();

        var checker = new checker_texture(0.32, new Color(.2, .3, .1), new Color(.9, .9, .9));

        world.add(new sphere(new Point3(0,-10, 0), 10, new lambertian(checker)));
        world.add(new sphere(new Point3(0, 10, 0), 10, new lambertian(checker)));

        camera cam = new camera();

        cam.aspect_ratio      = 16.0 / 9.0;
        cam.image_width       = 400;
        cam.samples_per_pixel = 100;
        cam.max_depth         = 50;

        cam.vfov     = 20;
        cam.lookfrom = new Point3(13,2,3);
        cam.lookat   = new Point3(0,0,0);
        cam.vup      = new Vec3(0,1,0);

        cam.defocus_angle = 0;

        cam.render(world);
    }
}