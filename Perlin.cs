/*namespace RayTracing;

public struct perlin {
    public const int point_count = 256;
    public double randfloat[point_count];
    public int perm_x[point_count];
    public int perm_y[point_count];
    public int perm_z[point_count];
    perlin() {
        for (int i = 0; i < point_count; i++) {
            randfloat[i] = RandomUtilities.RandomDouble();
        }

        perlin_generate_perm(perm_x);
        perlin_generate_perm(perm_y);
        perlin_generate_perm(perm_z);
    }

    double noise(const point3& p) const {
        auto i = int(4*p.x()) & 255;
        auto j = int(4*p.y()) & 255;
        auto k = int(4*p.z()) & 255;

        return randfloat[perm_x[i] ^ perm_y[j] ^ perm_z[k]];
    }

    private:
    

    static void perlin_generate_perm(int* p) {
        for (int i = 0; i < point_count; i++)
            p[i] = i;

        permute(p, point_count);
    }

    static void permute(int* p, int n) {
        for (int i = n-1; i > 0; i--) {
            int target = random_int(0, i);
            int tmp = p[i];
            p[i] = p[target];
            p[target] = tmp;
        }
    }
};*/