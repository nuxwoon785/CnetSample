using System;

namespace DefectPrototypeConsole;

class Program
{
    static void Main()
    {
        var rng = new Random(0);

        // Step 1: support image -> feature map -> prototype
        double[,,] supportFeatureMap = RandomFeatureMap(rng, channels: 64, height: 8, width: 8);
        bool[,] defectMask = new bool[8, 8];
        for (int y = 2; y < 5; y++)
        for (int x = 3; x < 6; x++)
            defectMask[y, x] = true;

        double[] prototype = BuildPrototype(supportFeatureMap, defectMask);

        // Step 2: query image -> feature map -> similarity map
        double[,,] queryFeatureMap = RandomFeatureMap(rng, channels: 64, height: 8, width: 8);
        double[,] similarityMap = SimilarityMap(queryFeatureMap, prototype);

        Console.WriteLine($"Prototype length: {prototype.Length}");
        var (min, max, mean) = Stats(similarityMap);
        Console.WriteLine($"Similarity map stats -> min: {min:F3} max: {max:F3} mean: {mean:F3}");
    }

    static double[,,] RandomFeatureMap(Random rng, int channels, int height, int width)
    {
        var map = new double[channels, height, width];
        for (int c = 0; c < channels; c++)
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            map[c, y, x] = NextGaussian(rng);
        return map;
    }

    static double[] BuildPrototype(double[,,] featureMap, bool[,] mask)
    {
        int channels = featureMap.GetLength(0);
        int height = featureMap.GetLength(1);
        int width = featureMap.GetLength(2);

        if (mask.GetLength(0) != height || mask.GetLength(1) != width)
            throw new ArgumentException("Mask dimensions must match feature map spatial shape");

        var prototype = new double[channels];
        int count = 0;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (!mask[y, x]) continue;
            count++;
            for (int c = 0; c < channels; c++)
                prototype[c] += featureMap[c, y, x];
        }

        if (count == 0)
            throw new ArgumentException("Mask must contain at least one positive pixel");

        for (int c = 0; c < channels; c++)
            prototype[c] /= count;

        return prototype;
    }

    static double[,] SimilarityMap(double[,,] queryFeatureMap, double[] prototype)
    {
        int channels = queryFeatureMap.GetLength(0);
        int height = queryFeatureMap.GetLength(1);
        int width = queryFeatureMap.GetLength(2);

        if (prototype.Length != channels)
            throw new ArgumentException("Prototype channel size must match feature map channels");

        double protoNorm = Math.Sqrt(Dot(prototype, prototype)) + 1e-8;
        double[,] similarity = new double[height, width];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            double[] pixelVector = new double[channels];
            for (int c = 0; c < channels; c++)
                pixelVector[c] = queryFeatureMap[c, y, x];

            double pixelNorm = Math.Sqrt(Dot(pixelVector, pixelVector)) + 1e-8;
            similarity[y, x] = Dot(pixelVector, prototype) / (protoNorm * pixelNorm);
        }

        return similarity;
    }

    static (double min, double max, double mean) Stats(double[,] map)
    {
        double min = double.MaxValue, max = double.MinValue, sum = 0;
        int h = map.GetLength(0), w = map.GetLength(1);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            double v = map[y, x];
            min = Math.Min(min, v);
            max = Math.Max(max, v);
            sum += v;
        }
        double mean = sum / (h * w);
        return (min, max, mean);
    }

    static double Dot(double[] a, double[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same length");
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    static double NextGaussian(Random rng)
    {
        // Box-Muller transform
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}
