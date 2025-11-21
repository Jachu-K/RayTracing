namespace RayTracing;
using System.Globalization;

public class OBJLoader
{
    public class OBJMesh
    {
        public List<Point3> vertices = new List<Point3>();
        public List<Vec3> texcoords = new List<Vec3>();
        public List<Vec3> normals = new List<Vec3>();
        public List<List<(int v, int vt, int vn)>> faces = new List<List<(int, int, int)>>();
        public string materialName;
    }


    private static string FindActualObjFile(string requestedPath)
    {
        // Sprawdź czy plik istnieje pod podaną ścieżką
        if (File.Exists(requestedPath))
        {
            Console.WriteLine($"Found OBJ at exact path: {requestedPath}");
            return requestedPath;
        }

        // Jeśli nie, szukaj w całym projekcie
        string currentDir = Directory.GetCurrentDirectory();
        Console.WriteLine($"Searching for OBJ files from: {currentDir}");

        try
        {
            // Szukaj plików .obj w całym katalogu projektu
            var objFiles = Directory.GetFiles(currentDir, "*.obj", SearchOption.AllDirectories);
            
            Console.WriteLine($"Found {objFiles.Length} OBJ files:");
            foreach (var file in objFiles)
            {
                Console.WriteLine($"  - {file}");
            }

            if (objFiles.Length == 0)
            {
                Console.WriteLine("No OBJ files found!");
                return null;
            }

            // Spróbuj znaleźć plik po nazwie
            string requestedName = Path.GetFileName(requestedPath);
            if (!string.IsNullOrEmpty(requestedName))
            {
                var matchingFiles = objFiles.Where(f => Path.GetFileName(f).Equals(requestedName, StringComparison.OrdinalIgnoreCase)).ToArray();
                
                if (matchingFiles.Length > 0)
                {
                    Console.WriteLine($"Found matching OBJ: {matchingFiles[0]}");
                    return matchingFiles[0];
                }
                else
                {
                    Console.WriteLine($"No OBJ file named '{requestedName}' found.");
                }
            }

            // Jeśli nie znaleziono dokładnej nazwy, użyj pierwszego znalezionego pliku OBJ
            Console.WriteLine($"Using first found OBJ: {objFiles[0]}");
            return objFiles[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching for OBJ files: {ex.Message}");
            return null;
        }
    }

    private static string FindAnyObjFile()
    {
        try
        {
            string currentDir = Directory.GetCurrentDirectory();
            var objFiles = Directory.GetFiles(currentDir, "*.obj", SearchOption.AllDirectories);
            return objFiles.Length > 0 ? objFiles[0] : null;
        }
        catch
        {
            return null;
        }
    }

    private static string FindSceneRoot(string startDirectory)
    {
        if (string.IsNullOrEmpty(startDirectory))
        {
            Console.WriteLine("WARNING: startDirectory is null or empty, using current directory");
            return Directory.GetCurrentDirectory();
        }

        try
        {
            DirectoryInfo dir = new DirectoryInfo(startDirectory);
            
            while (dir != null && dir.Exists)
            {
                var objFiles = dir.GetFiles("*.obj", SearchOption.AllDirectories);
                var jpgFiles = dir.GetFiles("*.jpg", SearchOption.AllDirectories);
                
                if (objFiles.Length > 0 || jpgFiles.Length > 0)
                {
                    Console.WriteLine($"Found scene root: {dir.FullName} ({objFiles.Length} OBJ files, {jpgFiles.Length} JPG files)");
                    return dir.FullName;
                }
                
                dir = dir.Parent;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding scene root: {ex.Message}");
        }

        Console.WriteLine($"Using startDirectory as scene root: {startDirectory}");
        return startDirectory;
    }

    private static string FindFileInScene(string sceneRoot, string filename)
    {
        if (string.IsNullOrEmpty(sceneRoot))
        {
            Console.WriteLine($"WARNING: sceneRoot is empty, cannot search for {filename}");
            return filename;
        }

        try
        {
            // Najpierw spróbuj dokładnej ścieżki
            var files = Directory.GetFiles(sceneRoot, filename, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                Console.WriteLine($"Found file: {files[0]}");
                return files[0];
            }

            // Jeśli nie, szukaj po nazwie pliku
            string justName = Path.GetFileName(filename);
            if (!string.IsNullOrEmpty(justName))
            {
                files = Directory.GetFiles(sceneRoot, justName, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    Console.WriteLine($"Found file by name: {files[0]}");
                    return files[0];
                }
            }

            Console.WriteLine($"File not found: {filename} in {sceneRoot}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching for {filename}: {ex.Message}");
        }

        return filename;
    }

    private static string FindTextureFile(string sceneRoot, string texturePath)
    {
        if (string.IsNullOrEmpty(sceneRoot))
        {
            Console.WriteLine($"WARNING: sceneRoot is empty, cannot search for texture: {texturePath}");
            return texturePath;
        }

        string textureName = Path.GetFileName(texturePath);
        
        // Szukaj różnych wariantów nazw
        string[] possibleNames = {
            textureName,
            textureName.ToLower(),
            textureName.ToUpper(),
            textureName.Replace(" ", "_"),
            Path.GetFileNameWithoutExtension(textureName) + ".jpg",
            Path.GetFileNameWithoutExtension(textureName) + ".jpeg",
            
            // Typowe nazwy tekstur
            Path.GetFileNameWithoutExtension(textureName) + "_albedo.jpg",
            Path.GetFileNameWithoutExtension(textureName) + "_diffuse.jpg", 
            Path.GetFileNameWithoutExtension(textureName) + "_basecolor.jpg",
            "albedo.jpg",
            "diffuse.jpg",
            "texture.jpg"
        };

        foreach (string name in possibleNames)
        {
            try
            {
                var files = Directory.GetFiles(sceneRoot, name, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    Console.WriteLine($"    Found texture: {files[0]}");
                    return files[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Error searching for {name}: {ex.Message}");
            }
        }

        Console.WriteLine($"    Texture not found: {texturePath}");
        return texturePath;
    }

    private static Dictionary<string, material> LoadMTL(string mtlFilepath, string sceneRoot)
    {
        var materials = new Dictionary<string, material>();
        string currentMaterial = "";

        Console.WriteLine($"Loading MTL file: {mtlFilepath}");

        try
        {
            foreach (string line in File.ReadLines(mtlFilepath))
            {
                string[] parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "newmtl": // new material
                        currentMaterial = parts[1];
                        materials[currentMaterial] = new lambertian(new Color(0.8, 0.8, 0.8)); // domyślny
                        Console.WriteLine($"  Defined material: {currentMaterial}");
                        break;

                    case "map_Kd": // diffuse texture
                        if (!string.IsNullOrEmpty(currentMaterial))
                        {
                            string textureFile = FindTextureFile(sceneRoot, parts[1]);
                            if (File.Exists(textureFile))
                            {
                                Console.WriteLine($"    Loading texture: {textureFile}");
                                try
                                {
                                    var texture = new image_texture(textureFile);
                                    materials[currentMaterial] = new lambertian(texture);
                                    Console.WriteLine($"    Successfully applied texture to {currentMaterial}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"    Error loading texture: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"    Texture not found: {parts[1]}");
                                Console.WriteLine($"    Searched for: {textureFile}");
                            }
                        }
                        break;

                    case "Kd": // diffuse color
                        if (!string.IsNullOrEmpty(currentMaterial) && parts.Length >= 4)
                        {
                            double r = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double g = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double b = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            materials[currentMaterial] = new lambertian(new Color(r, g, b));
                            Console.WriteLine($"    Material {currentMaterial} color: {r}, {g}, {b}");
                        }
                        break;

                    case "Ka": // ambient color
                    case "Ks": // specular color
                    case "Ns": // specular exponent
                        // Możesz dodać obsługę tych później
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading MTL: {ex.Message}");
        }

        return materials;
    }

    public static (List<OBJMesh>, Dictionary<string, material>) LoadOBJWithMaterials(string objFilepath)
    {
        var meshes = new List<OBJMesh>();
        var materials = new Dictionary<string, material>();
        
        string actualObjPath = FindActualObjFile(objFilepath);
        if (actualObjPath == null)
        {
            Console.WriteLine($"ERROR: Could not find any OBJ file matching: {objFilepath}");
            string anyObj = FindAnyObjFile();
            if (anyObj != null)
            {
                Console.WriteLine($"Using found OBJ file: {anyObj}");
                actualObjPath = anyObj;
            }
            else
            {
                Console.WriteLine("No OBJ files found at all!");
                return (meshes, materials);
            }
        }

        string objDirectory = Path.GetDirectoryName(actualObjPath);
        string sceneRoot = FindSceneRoot(objDirectory);

        Console.WriteLine($"Loading OBJ: {actualObjPath}");
        Console.WriteLine($"Scene root: {sceneRoot}");

        OBJMesh currentMesh = new OBJMesh();
        string currentMaterial = "default";
        
        // Domyślny materiał
        materials["default"] = new lambertian(new Color(0.8, 0.8, 0.8));

        try
        {
            foreach (string line in File.ReadLines(actualObjPath))
            {
                string[] parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v": // vertex
                        if (parts.Length >= 4)
                        {
                            double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            currentMesh.vertices.Add(new Point3(x, y, z));
                        }
                        break;

                    case "vt": // texture coordinate
                        if (parts.Length >= 3)
                        {
                            double u = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double v = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            currentMesh.texcoords.Add(new Vec3(u, v, 0));
                        }
                        break;

                    case "vn": // vertex normal
                        if (parts.Length >= 4)
                        {
                            double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            currentMesh.normals.Add(new Vec3(x, y, z));
                        }
                        break;

                    case "f": // face
                        var face = new List<(int v, int vt, int vn)>();
                        for (int i = 1; i < parts.Length; i++)
                        {
                            string[] indices = parts[i].Split('/');
                            
                            int v = int.Parse(indices[0]) - 1;
                            int vt = indices.Length > 1 && !string.IsNullOrEmpty(indices[1]) ? int.Parse(indices[1]) - 1 : -1;
                            int vn = indices.Length > 2 && !string.IsNullOrEmpty(indices[2]) ? int.Parse(indices[2]) - 1 : -1;
                            
                            face.Add((v, vt, vn));
                        }
                        currentMesh.faces.Add(face);
                        currentMesh.materialName = currentMaterial;
                        break;

                    case "usemtl": // use material
                        currentMaterial = parts[1];
                        Console.WriteLine($"Using material: {currentMaterial}");
                        
                        // Upewnij się, że materiał istnieje
                        if (!materials.ContainsKey(currentMaterial))
                        {
                            Console.WriteLine($"WARNING: Material {currentMaterial} not found, creating default");
                            materials[currentMaterial] = new lambertian(new Color(0.8, 0.8, 0.8));
                        }
                        break;

                    case "mtllib": // material library
                        string mtlFilename = parts[1];
                        Console.WriteLine($"Found MTL reference: {mtlFilename}");
                        string mtlFilepath = FindFileInScene(sceneRoot, mtlFilename);
                        if (File.Exists(mtlFilepath))
                        {
                            Console.WriteLine($"Loading MTL: {mtlFilepath}");
                            var loadedMaterials = LoadMTL(mtlFilepath, sceneRoot);
                            foreach (var mat in loadedMaterials)
                            {
                                materials[mat.Key] = mat.Value;
                            }
                            Console.WriteLine($"Loaded {loadedMaterials.Count} materials from MTL");
                        }
                        else
                        {
                            Console.WriteLine($"MTL file not found: {mtlFilename}");
                            Console.WriteLine($"Searched at: {mtlFilepath}");
                        }
                        break;

                    // DODAJ OBSŁUGĘ TEKSTUR BEZPOŚREDNIO W PLIKU OBJ
                    case "map_Kd": // diffuse texture in OBJ file
                        if (!string.IsNullOrEmpty(currentMaterial))
                        {
                            string textureFile = FindTextureFile(sceneRoot, parts[1]);
                            if (File.Exists(textureFile))
                            {
                                Console.WriteLine($"    Loading texture from OBJ: {textureFile}");
                                try
                                {
                                    var texture = new image_texture(textureFile);
                                    materials[currentMaterial] = new lambertian(texture);
                                    Console.WriteLine($"    Successfully applied texture to {currentMaterial}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"    Error loading texture: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"    Texture not found: {parts[1]}");
                                Console.WriteLine($"    Searched for: {textureFile}");
                            }
                        }
                        break;

                    case "Kd": // diffuse color in OBJ file
                        if (!string.IsNullOrEmpty(currentMaterial) && parts.Length >= 4)
                        {
                            double r = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double g = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double b = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            materials[currentMaterial] = new lambertian(new Color(r, g, b));
                            Console.WriteLine($"    Material {currentMaterial} color from OBJ: {r}, {g}, {b}");
                        }
                        break;
                }
            }

            meshes.Add(currentMesh);
            Console.WriteLine($"Loaded: {currentMesh.vertices.Count} vertices, {currentMesh.texcoords.Count} texcoords, {currentMesh.faces.Count} faces");
            Console.WriteLine($"Final materials count: {materials.Count}");
            
            // Debug: wypisz wszystkie załadowane materiały
            foreach (var mat in materials)
            {
                Console.WriteLine($"  - {mat.Key}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading OBJ: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        return (meshes, materials);
    }
}