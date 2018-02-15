using System;
using static ImageReadCS.Lab1;

namespace ImageReadCS
{
    public static class Lab2
    {
        static GrayscaleFloatImage RgbToGray(ColorFloatImage image)
        {
            GrayscaleFloatImage gray = new GrayscaleFloatImage(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    gray[x, y] = (float)(image[x, y].r * 0.299 + image[x, y].g * 0.587 + image[x, y].b * 0.114);
                }
            return gray;
        }

        static float ColorPixelToGray(ColorFloatPixel p)
        {
            return (float)(p.r * 0.299 + p.g * 0.587 + p.b * 0.114);
        }

        public static double MSE(ColorFloatImage i1, ColorFloatImage i2)
        {
            if (i1.Height != i2.Height || i1.Width != i2.Width)
                return 0;
            
            double mse = 0;

            for (int y = 0; y < i1.Height; y++)
                for (int x = 0; x < i1.Width; x++)
                {
                    mse += Math.Pow(ColorPixelToGray(i1[x, y]) - ColorPixelToGray(i2[x, y]), 2);
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

        public static double Canny(ColorFloatImage i1, float sigma)
        {
            GrayscaleFloatImage gray = RgbToGray(i1);


            return 0;

        }
        public static double Gabor(ColorFloatImage i1)
        {
            return 0;
        }
    }
}
