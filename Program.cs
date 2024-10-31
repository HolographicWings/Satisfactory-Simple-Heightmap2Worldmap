using System.Drawing;
using System.Drawing.Imaging;

public class SimpleHeightmapConverter
{
    public static void Main(string[] args)
    {
        string inputPath = "heightmap.png";
        string outputPath = "heightmap_transformed.png";

        Bitmap heightmap = new Bitmap(inputPath);
        Bitmap processedImage = ProcessHeightmap(heightmap);
        Bitmap postProcessedImage = PostProcessHeightmap(heightmap, processedImage);
        postProcessedImage.Save(outputPath, ImageFormat.Png);

        if(postProcessedImage != null)
            Console.WriteLine("Image saved !");
    }

    const byte waterlevel = 50;
    const byte surfaceStepSize = 5;
    public static Bitmap ProcessHeightmap(Bitmap heightmap)
    {
        int width = heightmap.Width;
        int height = heightmap.Height;

        Bitmap processedImage = new Bitmap(width, height);

        Color lowColor = Color.FromArgb(0x8A, 0x82, 0x70); // #8A8270
        Color highColor = Color.FromArgb(0xFF, 0xFF, 0xFF); // #FFFFFF

        Color waterColor1 = Color.FromArgb(0x4D, 0x9C, 0xAA); // #4D9CAA
        Color waterColor2 = Color.FromArgb(0x4D, 0x8D, 0x9A); // #4D8D9A
        Color waterColor3 = Color.FromArgb(0x4A, 0x7D, 0x88); // #4A7D88
        Color waterColor4 = Color.FromArgb(0x4A, 0x6E, 0x75); // #4A6E75

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte grayValue = heightmap.GetPixel(x, y).R;

                Color newColor;

                switch(grayValue)
                {
                    case >= waterlevel:
                        double gradientFactor = ((grayValue - waterlevel)/ surfaceStepSize * surfaceStepSize) / 205.0;

                        byte red = (byte)(lowColor.R + (highColor.R - lowColor.R) * gradientFactor);
                        byte green = (byte)(lowColor.G + (highColor.G - lowColor.G) * gradientFactor);
                        byte blue = (byte)(lowColor.B + (highColor.B - lowColor.B) * gradientFactor);

                        newColor = Color.FromArgb(red, green, blue);
                        break;
                    case < waterlevel:
                        switch (grayValue)
                        {
                            case < 20:
                                newColor = waterColor4;
                                break;
                            case < 30:
                                newColor = waterColor3;
                                break;
                            case < 40:
                                newColor = waterColor2;
                                break;
                            default:
                                newColor = waterColor1;
                                break;
                        }
                        break;
                }

                processedImage.SetPixel(x, y, newColor);
            }
        }

        return processedImage;
    }
    public static Bitmap PostProcessHeightmap(Bitmap heightmap, Bitmap processedImage)
    {
        int width = heightmap.Width;
        int height = heightmap.Height;

        Bitmap postProcessedImage = processedImage;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte grayValue = (byte)(heightmap.GetPixel(x, y).R / surfaceStepSize * surfaceStepSize);

                if (grayValue >= waterlevel)
                {
                    byte minValue = grayValue;
                    byte maxValue = grayValue;
                    byte difference = (byte)(maxValue - minValue);
                    byte neighbor = 255;
                    foreach ((int, int) v in GetNeighborPixels(x, y, width, height))
                    {
                        neighbor = (byte)(heightmap.GetPixel(v.Item1, v.Item2).R / surfaceStepSize * surfaceStepSize);
                        if (neighbor < waterlevel)
                            postProcessedImage.SetPixel(x, y, Color.FromArgb(32, 32, 32));
                        else if (neighbor < minValue)
                            minValue = neighbor;
                        else if (neighbor > maxValue)
                            maxValue = neighbor;
                        difference = (byte)(grayValue - minValue);
                    }
                    if (difference > 0)
                    {
                        Color currentColor = processedImage.GetPixel(x, y);
                        byte lineIntensity = (byte)(difference + 14);
                        byte red = (byte)(Math.Clamp(currentColor.R - lineIntensity, (byte)15, (byte)255));
                        byte green = (byte)(Math.Clamp(currentColor.G - lineIntensity, (byte)15, (byte)255));
                        byte blue = (byte)(Math.Clamp(currentColor.B - lineIntensity, (byte)15, (byte)255));

                        Color newColor = Color.FromArgb(red, green, blue);

                        postProcessedImage.SetPixel(x, y, newColor);
                    }
                }
            }
        }

        return postProcessedImage;
    }

    public static List<(int, int)> GetNeighborPixels(int X, int Y, int maxX, int maxY)
    {
        var neighbors = new List<(int, int)>(8);

        if (X > 0) neighbors.Add((X - 1, Y));
        if (X < maxX - 1) neighbors.Add((X + 1, Y));
        if (Y > 0) neighbors.Add((X, Y - 1));
        if (Y < maxY - 1) neighbors.Add((X, Y + 1));

        if (X > 0 && Y > 0) neighbors.Add((X - 1, Y - 1));
        if (X < maxX - 1 && Y > 0) neighbors.Add((X + 1, Y - 1));
        if (X > 0 && Y < maxY - 1) neighbors.Add((X - 1, Y + 1));
        if (X < maxX - 1 && Y < maxY - 1) neighbors.Add((X + 1, Y + 1));

        return neighbors;
    }
}
