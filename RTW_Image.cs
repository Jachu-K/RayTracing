namespace RayTracing;
using System;
using System.IO;

public class rtw_image : IDisposable
{
    public rtw_image() { }

    public rtw_image(string image_filename)
    {
        // Loads image data from the specified file. If the RTW_IMAGES environment variable is
        // defined, looks only in that directory for the image file. If the image was not found,
        // searches for the specified image file first from the current directory, then in the
        // images/ subdirectory, then the _parent's_ images/ subdirectory, and then _that_
        // parent, on so on, for six levels up. If the image was not loaded successfully,
        // width() and height() will return 0.

        var filename = image_filename;
        var imagedir = Environment.GetEnvironmentVariable("RTW_IMAGES");

        Console.WriteLine($"Searching for image: {image_filename}");
        Console.WriteLine($"RTW_IMAGES environment variable: {(string.IsNullOrEmpty(imagedir) ? "not set" : imagedir)}");

        // Hunt for the image file in some likely locations.
        if (!string.IsNullOrEmpty(imagedir)) 
        {
            string path = Path.Combine(imagedir, image_filename);
            Console.WriteLine($"Trying: {path}");
            if (load(path)) 
            {
                Console.WriteLine($"SUCCESS: Loaded image from RTW_IMAGES directory: {path}");
                return;
            }
        }

        string[] searchPaths = {
            filename,
            Path.Combine("images", filename),
            Path.Combine("..", "images", filename),
            Path.Combine("..", "..", "images", filename),
            Path.Combine("..", "..", "..", "images", filename),
            Path.Combine("..", "..", "..", "..", "images", filename),
            Path.Combine("..", "..", "..", "..", "..", "images", filename),
            Path.Combine("..", "..", "..", "..", "..", "..", "images", filename)
        };

        foreach (string path in searchPaths)
        {
            Console.WriteLine($"Trying: {Path.GetFullPath(path)}");
            if (load(path)) 
            {
                Console.WriteLine($"SUCCESS: Loaded image from: {Path.GetFullPath(path)}");
                Console.WriteLine($"Image dimensions: {image_width}x{image_height}");
                return;
            }
        }

        Console.Error.WriteLine($"ERROR: Could not load image file '{image_filename}'.");
        Console.Error.WriteLine("Searched in the following locations:");
        if (!string.IsNullOrEmpty(imagedir))
        {
            Console.Error.WriteLine($"  - {Path.Combine(imagedir, image_filename)}");
        }
        foreach (string path in searchPaths)
        {
            string fullPath = Path.GetFullPath(path);
            Console.Error.WriteLine($"  - {fullPath} {(File.Exists(fullPath) ? "EXISTS BUT FAILED TO LOAD" : "NOT FOUND")}");
        }
    }

    private bool load(string filename)
    {
        // Loads the linear (gamma=1) image data from the given file name. Returns true if the
        // load succeeded.

        if (!File.Exists(filename))
        {
            Console.WriteLine($"  File not found: {filename}");
            return false;
        }

        try
        {
            Console.WriteLine($"  Attempting to load: {filename}");
            
            // Prosta implementacja dla plików RAW/PPM (możesz rozszerzyć o inne formaty)
            if (filename.EndsWith(".ppm") || filename.EndsWith(".PPM"))
            {
                bool result = load_ppm(filename);
                Console.WriteLine($"  PPM load result: {result}");
                return result;
            }
            else
            {
                // Dla innych formatów potrzebowałbyś biblioteki zewnętrznej
                Console.WriteLine($"  Unsupported image format: {filename}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error loading image: {ex.Message}");
            return false;
        }
    }

    private bool load_ppm(string filename)
    {
        // Prosty loader PPM (Portable PixMap)
        try
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                // Odczyt nagłówka PPM
                var magic = reader.ReadChars(2);
                if (new string(magic) != "P6") 
                {
                    Console.WriteLine($"  Invalid PPM magic number: {new string(magic)}");
                    return false;
                }

                reader.ReadChar(); // whitespace
                
                // Odczyt wymiarów
                string widthStr = "";
                char c;
                while ((c = reader.ReadChar()) != ' ') widthStr += c;
                image_width = int.Parse(widthStr);

                string heightStr = "";
                while ((c = reader.ReadChar()) != '\n') heightStr += c;
                image_height = int.Parse(heightStr);

                string maxValStr = "";
                while ((c = reader.ReadChar()) != '\n') maxValStr += c;
                int maxVal = int.Parse(maxValStr);

                Console.WriteLine($"  PPM dimensions: {image_width}x{image_height}, max value: {maxVal}");

                bytes_per_scanline = image_width * bytes_per_pixel;
                
                // Odczyt danych RGB
                int total_bytes = image_width * image_height * bytes_per_pixel;
                byte[] raw_data = reader.ReadBytes(total_bytes);

                Console.WriteLine($"  Read {raw_data.Length} bytes of image data");

                // Konwersja do float [0.0, 1.0]
                fdata = new float[total_bytes];
                for (int i = 0; i < total_bytes; i++)
                {
                    fdata[i] = raw_data[i] / 255.0f;
                }

                convert_to_bytes();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error in PPM loader: {ex.Message}");
            return false;
        }
    }

    // Reszta metod pozostaje bez zmian...
    public int width()  { return (fdata == null) ? 0 : image_width; }
    public int height() { return (fdata == null) ? 0 : image_height; }

    public byte[] pixel_data(int x, int y)
    {
        byte[] magenta = { 255, 0, 255 };
        if (bdata == null) return magenta;

        x = clamp(x, 0, image_width);
        y = clamp(y, 0, image_height);

        int index = y * bytes_per_scanline + x * bytes_per_pixel;
        return new byte[] { bdata[index], bdata[index + 1], bdata[index + 2] };
    }

    private readonly int bytes_per_pixel = 3;
    private float[] fdata = null;
    private byte[] bdata = null;
    private int image_width = 0;
    private int image_height = 0;
    private int bytes_per_scanline = 0;

    private static int clamp(int x, int low, int high)
    {
        if (x < low) return low;
        if (x < high) return x;
        return high - 1;
    }

    private static byte float_to_byte(float value)
    {
        if (value <= 0.0) return 0;
        if (1.0 <= value) return 255;
        return (byte)(256.0 * value);
    }

    private void convert_to_bytes()
    {
        int total_bytes = image_width * image_height * bytes_per_pixel;
        bdata = new byte[total_bytes];

        for (int i = 0; i < total_bytes; i++)
        {
            bdata[i] = float_to_byte(fdata[i]);
        }
    }

    public void Dispose()
    {
        fdata = null;
        bdata = null;
    }
}