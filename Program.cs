using System;
using RayTracing;



class Program
{
    public static double degrees_to_radians(double degrees) {
        return degrees * Math.PI / 180.0;
    }

    static void Main()
    {
        switch (6) {
            case 1: spheres();  break;                  // Test z ksiazki (z duzymi limitami)
            case 2: checkered_spheres(); break;         // Dalsza czesc ksiazki - teskstura na kuli
            case 3: earth(); break;                     // Tekstura z pliku
            case 4: cat(); break;                       // Tekstura z pliku (ale tym razem z kotem)
            case 5: pyramid_test(); break;              // Test trójkątów
            case 6: cube_test(); break;                 // Prosty plik .obj
            case 7: rabbit_test_corrected(); break;     // Plik .obj z internetu
            /*case 8: test_bvh_performance(); break;
            case 9: analyze_model(); break;
            case 11: debug_mtl_content(); break;*/
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
                        Color albedo = Color.random() * Color.random();
                        sphere_material = new lambertian(albedo);
                        var temp = center + new Point3(0, RandomUtilities.RandomDouble(0,.5), 0);
                        Point3 center2 = new Point3(temp.X, temp.Y, temp.Z);
                        //world.add(new sphere(center,center2, 0.2, sphere_material));
                        world.add(new sphere(center, 0.2, sphere_material));
                    } else if (choose_mat < 0.95) {
                        // metal
                        var albedo = Color.random(0.5,1);
                        var fuzz = RandomUtilities.RandomDouble(0, 0.5);
                        sphere_material = new metal(albedo, fuzz);
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
        
        cam.render(world, "spheres.ppm");

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

        cam.render(world, "checkered_spheres.ppm");
    }
    static void earth() {
        var earth_texture = new image_texture("earthmap.ppm");
        var earth_surface = new lambertian(earth_texture);
        var globe = new sphere(new Point3(0,0,0), 2, earth_surface);

        camera cam = new camera();

        cam.aspect_ratio      = 16.0 / 9.0;
        cam.image_width       = 400;
        cam.samples_per_pixel = 100;
        cam.max_depth         = 50;

        cam.vfov     = 20;
        cam.lookfrom = new Point3(0,0,12);
        cam.lookat   = new Point3(0,0,0);
        cam.vup      = new Vec3(0,1,0);

        cam.defocus_angle = 0;

        cam.render(new hittable_list(globe), "earth.ppm");
    }
    static void cat() {
        // Tekstura kota
        var cat_texture = new image_texture("cat.ppm");
        var cat_surface = new lambertian(cat_texture);
        var cat_sphere = new sphere(new Point3(-1.5, 0, 0), 1, cat_surface);

        // Lustrzana kula
        var mirror_sphere = new sphere(new Point3(-0.5, 0, -2), 1, new metal(new Color(0.8, 0.8, 0.8), 0.0));

        // Podłoże w kratkę
        var checker_texture = new checker_texture(0.5, new Color(0.2, 0.3, 0.1), new Color(0.9, 0.9, 0.9));
        var ground_material = new lambertian(checker_texture);
        var ground = new sphere(new Point3(0, -100.5, 0), 100, ground_material);

        // Lista obiektów
        var world = new hittable_list();
        world.add(cat_sphere);
        world.add(mirror_sphere);
        world.add(ground);

        camera cam = new camera();

        cam.aspect_ratio      = 16.0 / 9.0;
        cam.image_width       = 400;
        cam.samples_per_pixel = 100;
        cam.max_depth         = 50;

        cam.vfov     = 20;
        cam.lookfrom = new Point3(4, 0, 4);  // Kamera z boku (prawo-przód)
        cam.lookat   = new Point3(0, 0, 0);
        cam.vup      = new Vec3(0, 1, 0);

        cam.defocus_angle = 0;

        cam.render(world, "cat_sphere.ppm");
    }

    static void pyramid_test() {
        var world = new hittable_list();
    
        var red_mat = new lambertian(new Color(0.8, 0.2, 0.2));
        var green_mat = new lambertian(new Color(0.2, 0.8, 0.2));
        var blue_mat = new lambertian(new Color(0.2, 0.2, 0.8));
        var yellow_mat = new lambertian(new Color(0.8, 0.8, 0.2));
    
        Point3 top = new Point3(0, 1, 0);
        Point3 front_left = new Point3(-1, -1, -1);
        Point3 front_right = new Point3(1, -1, -1);
        Point3 back_right = new Point3(1, -1, 1);
        Point3 back_left = new Point3(-1, -1, 1);
    
        // Użyj dwustronnych trójkątów
        world.add(new two_sided_triangle(top, front_left, front_right, red_mat, "Front"));
        world.add(new two_sided_triangle(top, front_right, back_right, green_mat, "Right"));
        world.add(new two_sided_triangle(top, back_right, back_left, blue_mat, "Back"));
        world.add(new two_sided_triangle(top, back_left, front_left, yellow_mat, "Left"));
    
        // Podstawa też dwustronna
        //world.add(new two_sided_triangle(front_left, back_left, back_right, new lambertian(new Color(0.5, 0.5, 0.5)), "Base1"));
        //world.add(new two_sided_triangle(front_left, back_right, front_right, new lambertian(new Color(0.5, 0.5, 0.5)), "Base2"));
        
        // Podłoże
        var ground_mat = new lambertian(new Color(0.8, 0.8, 0.8));
        var ground = new sphere(new Point3(0, -100.5, 0), 100, ground_mat);
        //world.add(ground);

        camera cam = new camera();

        cam.aspect_ratio      = 16.0 / 9.0;
        cam.image_width       = 400;
        cam.samples_per_pixel = 100;
        cam.max_depth         = 50;

        cam.vfov     = 30;
        cam.lookfrom = new Point3(0, -2, -5);
        cam.lookat   = new Point3(0, 0, 0);
        cam.vup      = new Vec3(0, 1, 0);

        cam.defocus_angle = 0;

        cam.render(world, "pyramid_test.ppm");
    }

    static void cube_test()
    {
        var world = new hittable_list();
        
        var (meshes, materials) = OBJLoader.LoadOBJWithMaterials("cube.obj"); // "pyramid.obj"
        
        if (meshes.Count == 0)
        {
            Console.WriteLine("ERROR: No meshes loaded from cube.obj");
            return;
        }

        Console.WriteLine($"Loaded {meshes.Count} meshes, {materials.Count} materials");

        var allObjects = new hittable_list();

        foreach (var mesh in meshes)
        {
            material mat = materials.ContainsKey(mesh.materialName) 
                ? materials[mesh.materialName] 
                : new lambertian(new Color(0.8, 0.3, 0.3));

            Console.WriteLine($"Creating triangles for material: {mesh.materialName}");

            int triangleCount = 0;
            foreach (var face in mesh.faces)
            {
                if (face.Count >= 3)
                {
                    for (int i = 1; i < face.Count - 1; i++)
                    {
                        var v0 = mesh.vertices[face[0].v];
                        var v1 = mesh.vertices[face[i].v];
                        var v2 = mesh.vertices[face[i + 1].v];

                        Vec3 uv0 = face[0].vt >= 0 ? mesh.texcoords[face[0].vt] : new Vec3(0, 0, 0);
                        Vec3 uv1 = face[i].vt >= 0 ? mesh.texcoords[face[i].vt] : new Vec3(1, 0, 0);
                        Vec3 uv2 = face[i + 1].vt >= 0 ? mesh.texcoords[face[i + 1].vt] : new Vec3(0, 1, 0);

                        allObjects.add(new textured_triangle(v0, v1, v2, uv0, uv1, uv2, mat));
                        triangleCount++;
                    }
                }
            }
            Console.WriteLine($"Created {triangleCount} triangles");
        }
        foreach (var triangle in allObjects.objects) {
            world.add(triangle);
        }

        var ground = new sphere(new Point3(0, -100.5, 0), 100, new lambertian(new Color(0.5, 0.5, 0.5)));
        world.add(ground);
        
        camera cam = new camera();

        cam.aspect_ratio      = 16.0 / 9.0;
        cam.image_width       = 400;
        cam.samples_per_pixel = 100;
        cam.max_depth         = 50;

        cam.vfov     = 30;
        cam.lookfrom = new Point3(3, 2, 4);  // Lepsza pozycja do oglądania sześcianu
        cam.lookat   = new Point3(0, 0, 0);
        cam.vup      = new Vec3(0, 1, 0);

        cam.defocus_angle = 0;
        
        cam.render(world, "cube.ppm");
    }

    static void rabbit_test() {
        var world = new hittable_list();
        
        // Po prostu podaj nazwę pliku - loader sam go znajdzie
        string objFilename = "fg_SpkRabbit.obj"; // tylko nazwa pliku
        
        Console.WriteLine("Searching for OBJ file automatically...");
        var (meshes, materials) = OBJLoader.LoadOBJWithMaterials(objFilename);

        if (meshes.Count == 0)
        {
            Console.WriteLine("No meshes loaded - exiting");
            return;
        }

        // Stwórz obiekty
        foreach (var mesh in meshes)
        {
            material mat = materials.ContainsKey(mesh.materialName) 
                ? materials[mesh.materialName] 
                : new lambertian(new Color(0.8, 0.3, 0.3));

            Console.WriteLine($"Creating triangles for material: {mesh.materialName}");

            int triangleCount = 0;
            foreach (var face in mesh.faces)
            {
                if (face.Count >= 3)
                {
                    for (int i = 1; i < face.Count - 1; i++)
                    {
                        var v0 = mesh.vertices[face[0].v];
                        var v1 = mesh.vertices[face[i].v];
                        var v2 = mesh.vertices[face[i + 1].v];

                        Vec3 uv0 = face[0].vt >= 0 ? mesh.texcoords[face[0].vt] : new Vec3(0, 0, 0);
                        Vec3 uv1 = face[i].vt >= 0 ? mesh.texcoords[face[i].vt] : new Vec3(1, 0, 0);
                        Vec3 uv2 = face[i + 1].vt >= 0 ? mesh.texcoords[face[i + 1].vt] : new Vec3(0, 1, 0);

                        world.add(new textured_triangle(v0, v1, v2, uv0, uv1, uv2, mat));
                        triangleCount++;
                    }
                }
            }
            Console.WriteLine($"Created {triangleCount} triangles");
        }

        // Podłoże
        var ground = new sphere(new Point3(0, -100.5, 0), 100, new lambertian(new Color(0.8, 0.8, 0.8)));
        world.add(ground);

        // Kamera - dostosowana do królika
        camera cam = new camera();
        cam.aspect_ratio = 16.0 / 9.0;
        cam.image_width = 800;
        cam.samples_per_pixel = 50; // Mniej próbek dla szybkości
        cam.max_depth = 50;
        cam.vfov = 40;
        cam.lookfrom = new Point3(0, 1, 3);
        cam.lookat = new Point3(0, 0.5, 0);
        cam.vup = new Vec3(0, 1, 0);

        Console.WriteLine("Rendering steampunk rabbit...");
        var world2 = new bvh_node(world);
        cam.render(world2, "steampunk_rabbit.ppm");
    }
    static void test_bvh_performance() {
        Console.WriteLine("=== BVH PERFORMANCE TEST ===");
    
        // Stwórz prostą scenę testową
        var testWorld = new hittable_list();
    
        // Dodaj kilka prostych sfer (szybkie do renderowania)
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                testWorld.add(new sphere(
                    new Point3(i - 5, 0.2, j - 5), 
                    0.2, 
                    new lambertian(new Color(0.8, 0.3, 0.3))
                ));
            }
        }
    
        Console.WriteLine($"Created test scene with {testWorld.objects.Count} objects");
    
        // Test 1: Bez BVH
        var world1 = testWorld;
    
        // Test 2: Z BVH
        var world2 = new bvh_node(testWorld);
    
        camera cam = new camera();
        cam.aspect_ratio = 16.0 / 9.0;
        cam.image_width = 200;
        cam.samples_per_pixel = 10;
        cam.max_depth = 5;
        cam.vfov = 90;
        cam.lookfrom = new Point3(0, 5, 10);
        cam.lookat = new Point3(0, 0, 0);
        cam.vup = new Vec3(0, 1, 0);

        Console.WriteLine("Rendering WITHOUT BVH...");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        cam.render(world1, "test_no_bvh.ppm");
        var time1 = stopwatch.ElapsedMilliseconds;
    
        Console.WriteLine("Rendering WITH BVH...");
        stopwatch.Restart();
        cam.render(world2, "test_with_bvh.ppm");
        var time2 = stopwatch.ElapsedMilliseconds;
    
        Console.WriteLine($"Results: No BVH = {time1}ms, With BVH = {time2}ms");
        Console.WriteLine($"BVH is {time1/(double)time2:F2}x faster");
    }
    static void analyze_model() {
        Console.WriteLine("=== MODEL ANALYSIS ===");
    
        var (meshes, materials) = OBJLoader.LoadOBJWithMaterials("fg_SpkRabbit.obj");
    
        int totalVertices = 0;
        int totalTriangles = 0;
        int totalFaces = 0;

        foreach (var mesh in meshes)
        {
            totalVertices += mesh.vertices.Count;
            totalFaces += mesh.faces.Count;
        
            foreach (var face in mesh.faces)
            {
                if (face.Count >= 3)
                {
                    totalTriangles += face.Count - 2; // Triangulacja
                }
            }
        }

        Console.WriteLine($"Model statistics:");
        Console.WriteLine($"- Meshes: {meshes.Count}");
        Console.WriteLine($"- Vertices: {totalVertices}");
        Console.WriteLine($"- Faces: {totalFaces}");
        Console.WriteLine($"- Triangles: {totalTriangles}");
        Console.WriteLine($"- Materials: {materials.Count}");

        if (totalTriangles > 10000)
        {
            Console.WriteLine($"WARNING: Model has {totalTriangles} triangles - this will be slow!");
            Console.WriteLine("Consider using a simplified version.");
        }
    }
    static void rabbit_test_corrected() {
        Console.WriteLine("=== RABBIT TEST CORRECTED ===");
        
        var allObjects = new hittable_list();
        var (meshes, materials) = OBJLoader.LoadOBJWithMaterials("fg_SpkRabbit.obj");
        
        int triangleCount = 0;
        foreach (var mesh in meshes)
        {
            material mat = materials.ContainsKey(mesh.materialName) 
                ? materials[mesh.materialName] 
                : new lambertian(new Color(0.8, 0.3, 0.3));

            foreach (var face in mesh.faces)
            {
                if (face.Count >= 3)
                {
                    for (int i = 1; i < face.Count - 1; i++)
                    {
                        var v0 = mesh.vertices[face[0].v];
                        var v1 = mesh.vertices[face[i].v];
                        var v2 = mesh.vertices[face[i + 1].v];

                        Vec3 uv0 = face[0].vt >= 0 ? mesh.texcoords[face[0].vt] : new Vec3(0, 0, 0);
                        Vec3 uv1 = face[i].vt >= 0 ? mesh.texcoords[face[i].vt] : new Vec3(1, 0, 0);
                        Vec3 uv2 = face[i + 1].vt >= 0 ? mesh.texcoords[face[i + 1].vt] : new Vec3(0, 1, 0);

                        allObjects.add(new textured_triangle(v0, v1, v2, uv0, uv1, uv2, mat));
                        triangleCount++;
                    }
                }
            }
        }

        allObjects.add(new sphere(new Point3(0, -100.5, 0), 100, new lambertian(new Color(0.8, 0.8, 0.8))));

        Console.WriteLine($"Created {triangleCount} triangles + ground");

        var world = new bvh_node(allObjects);

        // Kamera
        camera cam = new camera();
        cam.aspect_ratio = 16.0 / 9.0;
        cam.image_width = 1200;
        cam.samples_per_pixel = 50;
        cam.max_depth = 10;
        cam.vfov = 35;
        cam.lookfrom = new Point3(0, 0.08, 0.3);
        cam.lookat = new Point3(0, 0.08, 0);

        cam.render(world, "rabbit_correct.ppm");
    }
    static void debug_mtl_content() {
        Console.WriteLine("=== MTL CONTENT DEBUG ===");
    
        string mtlPath = "/home/jan/Dokumenty/csharp_p3/RayTracing/bin/Release/net9.0/steampunk-rabbit-figurine-free/source/fg_spkRabbit/fg_spkRabbit_cc/fg_spkRabbit.mtl";
    
        if (File.Exists(mtlPath))
        {
            Console.WriteLine($"Reading MTL file: {mtlPath}");
            var lines = File.ReadAllLines(mtlPath);
        
            Console.WriteLine($"MTL has {lines.Length} lines:");
            foreach (var line in lines)
            {
                Console.WriteLine($"  {line}");
            }
        
            // Analiza zawartości
            int materialCount = 0;
            bool hasTexture = false;
        
            foreach (var line in lines)
            {
                if (line.StartsWith("newmtl"))
                    materialCount++;
                if (line.StartsWith("map_Kd"))
                    hasTexture = true;
            }
        
            Console.WriteLine($"\nAnalysis: {materialCount} materials, has texture: {hasTexture}");
        }
        else
        {
            Console.WriteLine("MTL file not found!");
        }
    }
}