using System;
using System.Collections.Generic;
using System.Linq;
using static ImageReadCS.Lab1;

namespace ImageReadCS
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public static class Lab2
    {
        public static double MSE(ColorFloatImage i1, ColorFloatImage i2)
        {
            if (i1.Height != i2.Height || i1.Width != i2.Width)
                return 0;
            
            double mse = 0;

            for (int y = 0; y < i1.Height; y++)
                for (int x = 0; x < i1.Width; x++)
                {
                    mse += Math.Pow(RGB2GrayPix(i1[x, y]) - RGB2GrayPix(i2[x, y]), 2);
                }

            return mse / (i1.Width * i1.Height * 3);
        }

        public static double PSNR(ColorFloatImage i1, ColorFloatImage i2)
        {
            return 20 * Math.Log10(255.0 / Math.Sqrt(MSE(i1, i2)));
        }

        public static double SSIM(ColorFloatImage i1, ColorFloatImage i2)
        {
            double K1 = 0.01, K2 = 0.03;
            int L = 255;

            var C1 = Math.Pow(K1 * L, 2);
            var C2 = Math.Pow(K2 * L, 2);
            var mu1 = 0;//signal.fftconvolve(window, img1, mode = 'valid');
            var mu2 = 0;//signal.fftconvolve(window, img2, mode = 'valid');
            var mu1_sq = mu1 * mu1;
            var mu2_sq = mu2 * mu2;
            var mu1_mu2 = mu1 * mu2;
            var sigma1_sq = 0;//signal.fftconvolve(window, img1 * img1, mode = 'valid') - mu1_sq;
            var sigma2_sq = 0;//signal.fftconvolve(window, img2 * img2, mode = 'valid') - mu2_sq;
            var sigma12 = 0;//signal.fftconvolve(window, img1 * img2, mode = 'valid') - mu1_mu2;

            return ((2 * mu1_mu2 + C1) * (2 * sigma12 + C2)) / ((mu1_sq + mu2_sq + C1) *
                                                                (sigma1_sq + sigma2_sq + C2));
            
        }
        public static double MSSIM(ColorFloatImage i1, ColorFloatImage i2)
        {
            return 0;
        }

        public static GrayscaleFloatImage NonMaximumSuppression(GrayscaleFloatImage magnitude, GrayscaleFloatImage directions)
        {
            GrayscaleFloatImage dest = new GrayscaleFloatImage(magnitude.Width, magnitude.Height);

            for (int y = 0; y < magnitude.Height; y++)
                for (int x = 0; x < magnitude.Width; x++)
                {
                    int direction = (int)directions[x, y];

                    var pointList = new List<Point>();
                    var xList = Enumerable.Range(x - 1, 3).ToList();
                    var yList = Enumerable.Range(y - 1, 3).ToList();

                    if (direction == 0)
                        pointList = xList.Where(n => n >= 0 && n <= magnitude.Width - 1)
                                             .Select(n => new Point() { X = n, Y = y })
                                             .ToList();
                    else if (direction == 90)
                        pointList = yList.Where(n => n >= 0 && n <= magnitude.Height - 1)
                                             .Select(n => new Point() { X = x, Y = n })
                                             .ToList();
                    else
                    {
                        if (direction == 45)
                            xList.Reverse();

                        pointList = xList.Zip(yList, (first, second) => new Point() { X = first, Y = second })
                                         .Where(p => p.X >= 0 && p.X <= magnitude.Width - 1 && p.Y >= 0 && p.Y <= magnitude.Height - 1)
                                         .ToList();
                    }

                    var values = new List<float>();

                    foreach (var point in pointList)
                        values.Add(magnitude[point.X, point.Y]);

                    if (values.Max() > magnitude[x, y])
                        dest[x, y] = 0;
                    else
                        dest[x, y] = magnitude[x, y];
                
                }

            return magnitude;
        }

        public static GrayscaleFloatImage Hysteresis(GrayscaleFloatImage magnitude, GrayscaleFloatImage directions)
        {
            GrayscaleFloatImage dest = new GrayscaleFloatImage(magnitude.Width, magnitude.Height);

            return dest;
        }

        public static double Canny(ColorFloatImage image, float sigma)
        {
            var kernels = CalculateKernelXY(sigma, GaussDerivativePoint);
            var magnAndDir = MagnitudeAndDirections(image, kernels[0].Kernel, kernels[1].Kernel);
            var magnitude = magnAndDir[0];
            var directions = magnAndDir[1];
            var withoutNonMax = NonMaximumSuppression(magnitude, directions);

            return 0;

        }
        public static double Gabor(ColorFloatImage i1)
        {
            return 0;
        }
    }
}
