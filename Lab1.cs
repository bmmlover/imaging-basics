﻿using System;
using System.Collections.Generic;

namespace ImageReadCS
{
    public static class Lab1
    {
        public static ColorFloatImage InvertImage(ColorFloatImage image)
        {
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    image[x, y] = new ColorFloatPixel(255 - image[x, y].b, 255 - image[x, y].g,
                         255 - image[x, y].r, (image[x, y].a) / 2);
                }
            return image;
        }

        public static ColorFloatImage MirrorX(ColorFloatImage image)
        {
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width / 2; x++)
                {
                    ColorFloatPixel p = image[x, y];
                    image[x, y] = image[image.Width - 1 - x, y];
                    image[image.Width - 1 - x, y] = p;
                }
            return image;
        }

        public static ColorFloatImage MirrorY(ColorFloatImage image)
        {
            for (int y = 0; y < image.Height / 2; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    ColorFloatPixel p = image[x, y];
                    image[x, y] = image[x, image.Height - 1 - y];
                    image[x, image.Height - 1 - y] = p;
                }
            return image;
        }

        static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        static bool IsBlank(ColorFloatPixel p)
        {
            return p.r == 0 && p.g == 0 && p.b == 0;
        }


        static ColorFloatPixel Interpolate(ColorFloatPixel p0, ColorFloatPixel p1, double delta)
        {
            return new ColorFloatPixel((float)((1 - delta) * p0.r + delta * p1.r),
                                                 (float)((1 - delta) * p0.g + delta * p1.g),
                                                 (float)((1 - delta) * p0.b + delta * p1.b),
                                                 p1.a);
        }

        public static ColorFloatImage RotateCW(ColorFloatImage image, int angle)
        {
            switch (angle)
            {
                case 270:
                    ColorFloatImage image270 = new ColorFloatImage(image.Height, image.Width);
                    for (int y = 0; y < image.Height; y++)
                        for (int x = 0; x < image.Width; x++)
                        {
                            //image270[x, y] = image[image.Width - y - 1, x];
                            image270[y, image.Width - 1 - x] = image[x, y];
                        }
                    return image270;

                case 180:
                    ColorFloatImage image180 = new ColorFloatImage(image.Width, image.Height);
                    for (int y = 0; y < image.Height; y++)
                        for (int x = 0; x < image.Width; x++)
                        {
                            image180[x, y] = image[image.Width - x - 1, image.Height - y - 1];
                        }
                    return image180;

                case 90:
                    ColorFloatImage image90 = new ColorFloatImage(image.Height, image.Width);
                    for (int y = 0; y < image.Height; y++)
                        for (int x = 0; x < image.Width; x++)
                        {
                            //new_image[x, y] = image[y, image.Height - 1 - x];
                            image90[image.Height - 1 - y, x] = image[x, y];
                        }
                    return image90;
            }

            double rad = ConvertToRadians(angle);

            var newWidth = 0;
            var newHeight = 0;

            if (rad <= Math.PI / 2)
            {
                newWidth = (int)Math.Ceiling(Math.Abs(image.Width * Math.Cos(rad)) + Math.Abs(image.Height * Math.Sin(rad)));
                newHeight = (int)Math.Ceiling(Math.Abs(image.Width * Math.Sin(rad)) + Math.Abs(image.Height * Math.Cos(rad)));
            }
            else
            {
                var teta = rad - Math.PI / 2;
                newWidth = (int)Math.Ceiling(Math.Abs(image.Height * Math.Cos(teta)) + Math.Abs(image.Width * Math.Sin(teta)));
                newHeight = (int)Math.Ceiling(Math.Abs(image.Height * Math.Sin(teta)) + Math.Abs(image.Width * Math.Cos(teta)));
            }

            ColorFloatImage new_image = new ColorFloatImage(newHeight, newWidth);

            int centerX = image.Width / 2, centerY = image.Height / 2;
            int ncenterX = newWidth / 2, ncenterY = newHeight / 2;

            for (int y = 0; y < newHeight; y++)
                for (int x = 0; x < newWidth; x++)
                {
                    int cx = x - ncenterX;
                    int cy = ncenterY - y;

                    double ro = Math.Sqrt(cx * cx + cy * cy);
                    double fi = 0;
                    double nx = 0, ny = 0;

                    if (cx != 0)
                    {
                        fi = Math.Atan2(cy, cx);
                    }
                    else
                    {
                        if (cy == 0)
                        {
                            new_image[(int)nx, (int)ny] = image[x, y];
                            continue;
                            //maybe no continue;
                        }

                        fi = cy > 0 ? 0.5 * Math.PI : 1.5 * Math.PI;
                    }

                    fi -= rad;

                    nx = Math.Round(ro * Math.Cos(fi));
                    ny = Math.Round(ro * Math.Sin(fi));

                    nx = nx + centerX;
                    ny = centerY - ny;

                    if (nx < 0 || nx > image.Width || ny < 0 || ny > image.Height)
                        continue;

                    var nx0 = (int)(Math.Floor(nx));
                    var ny0 = (int)(Math.Floor(ny));
                    var nx1 = (int)(Math.Ceiling(nx));
                    var ny1 = (int)(Math.Ceiling(ny));

                    var dx = nx - nx0;
                    var dy = ny - ny0;

                    var topPixel = Interpolate(image[nx0, ny0], image[nx1, ny0], dx);
                    var bottomPixel = Interpolate(image[nx0, ny1], image[nx1, ny1], dx);
                    var newPixel = Interpolate(topPixel, bottomPixel, dy);

                    new_image[x, y] = newPixel;
                }
            return new_image;
        }

        static ColorFloatImage Prewitt(ColorFloatImage image, string arg)
        {
            int axes = 0;
            if (arg == "y")
                axes = 1;
            ColorFloatImage dest = new ColorFloatImage(image.Width, image.Height);
            int a12 = 0, a13 = 1, a21 = -1, a22 = 0, a23 = 1, a31 = -1, a32 = 0;
            int a11 = -1;
            int a33 = 1;
            if (axes == 0) // Gx
            {
                a12 = 0;
                a13 = 1;
                a21 = -1;
                a22 = 0;
                a23 = 1;
                a31 = -1;
                a32 = 0;
            }
            else if (axes == 1) // Gy
            {
                a12 = -1;
                a13 = -1;
                a21 = 0;
                a22 = 0;
                a23 = 0;
                a31 = 1;
                a32 = 1;
            }

            int border_x0 = 0, border_y0 = 0, border_xn = 0, border_yn = 0;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    if (x - 1 < 0)
                        border_x0 = 1;
                    if (y - 1 < 0)
                        border_y0 = 1;
                    if (x + 1 > image.Width - 1)
                        border_xn = 1;
                    if (y + 1 > image.Height - 1)
                        border_yn = 1;

                    ColorFloatPixel p11 = image[x - 1 + border_x0, y - 1 + border_y0];
                    ColorFloatPixel p12 = image[x, y - 1 + border_y0];
                    ColorFloatPixel p13 = image[x + 1 - border_xn, y - 1 + border_y0];
                    ColorFloatPixel p21 = image[x - 1 + border_x0, y];
                    ColorFloatPixel p22 = image[x, y];
                    ColorFloatPixel p23 = image[x + 1 - border_xn, y];
                    ColorFloatPixel p31 = image[x - 1 + border_x0, y + 1 - border_yn];
                    ColorFloatPixel p32 = image[x, y + 1 - border_yn];
                    ColorFloatPixel p33 = image[x + 1 - border_xn, y + 1 - border_yn];

                    border_x0 = 0;
                    border_y0 = 0;
                    border_xn = 0;
                    border_yn = 0;

                    float blue =
                         a11 * p11.b + a12 * p12.b + a13 * p13.b +
                         a21 * p21.b + a22 * p22.b + a23 * p23.b +
                         a31 * p31.b + a32 * p32.b + a33 * p33.b;
                    float green =
                         a11 * p11.g + a12 * p12.g + a13 * p13.g +
                         a21 * p21.g + a22 * p22.g + a23 * p23.g +
                         a31 * p31.g + a32 * p32.g + a33 * p33.g;
                    float red =
                         a11 * p11.r + a12 * p12.r + a13 * p13.r +
                         a21 * p21.r + a22 * p22.r + a23 * p23.r +
                         a31 * p31.r + a32 * p32.r + a33 * p33.r;

                    dest[x, y] = new ColorFloatPixel(blue + 128, green + 128, red + 128, image[x, y].a);
                }
            return dest;
        }

        static float Convolve(List<int> coef, List<ColorFloatPixel> pix)
        {
            float red = 0, green = 0, blue = 0;

            for (int i = 0; i < 9; i++)
            {
                red += coef[i] * pix[i].r;
                green += coef[i] * pix[i].g;
                blue += coef[i] * pix[i].b;
            }

            return (float)Math.Pow((red * 0.299 + green * 0.587 + blue * 0.114), 2);
        }


        public static ColorFloatImage Sobel(ColorFloatImage image, string arg)
        {
            int axes = 0;
            if (arg == "y")
                axes = 1;
            ColorFloatImage dest = new ColorFloatImage(image.Width, image.Height);
            int a12 = 0, a13 = 1, a21 = -2, a22 = 0, a23 = 2, a31 = -1, a32 = 0;
            int a11 = -1;
            int a33 = 1;
            if (axes == 0) // Gx
            {
                a12 = 0;
                a13 = 1;
                a21 = -2;
                a22 = 0;
                a23 = 2;
                a31 = -1;
                a32 = 0;
            }
            else if (axes == 1) // Gy
            {
                a12 = -2;
                a13 = -1;
                a21 = 0;
                a22 = 0;
                a23 = 0;
                a31 = 1;
                a32 = 2;
            }

            int border_x0 = 0, border_y0 = 0, border_xn = 0, border_yn = 0;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    if (x - 1 < 0)
                        border_x0 = 1;
                    if (y - 1 < 0)
                        border_y0 = 1;
                    if (x + 1 > image.Width - 1)
                        border_xn = 1;
                    if (y + 1 > image.Height - 1)
                        border_yn = 1;

                    ColorFloatPixel p11 = image[x - 1 + border_x0, y - 1 + border_y0];
                    ColorFloatPixel p12 = image[x, y - 1 + border_y0];
                    ColorFloatPixel p13 = image[x + 1 - border_xn, y - 1 + border_y0];
                    ColorFloatPixel p21 = image[x - 1 + border_x0, y];
                    ColorFloatPixel p22 = image[x, y];
                    ColorFloatPixel p23 = image[x + 1 - border_xn, y];
                    ColorFloatPixel p31 = image[x - 1 + border_x0, y + 1 - border_yn];
                    ColorFloatPixel p32 = image[x, y + 1 - border_yn];
                    ColorFloatPixel p33 = image[x + 1 - border_xn, y + 1 - border_yn];

                    border_x0 = 0;
                    border_y0 = 0;
                    border_xn = 0;
                    border_yn = 0;

                    float blue =
                         a11 * p11.b + a12 * p12.b + a13 * p13.b +
                         a21 * p21.b + a22 * p22.b + a23 * p23.b +
                         a31 * p31.b + a32 * p32.b + a33 * p33.b;
                    float green =
                         a11 * p11.g + a12 * p12.g + a13 * p13.g +
                         a21 * p21.g + a22 * p22.g + a23 * p23.g +
                         a31 * p31.g + a32 * p32.g + a33 * p33.g;
                    float red =
                         a11 * p11.r + a12 * p12.r + a13 * p13.r +
                         a21 * p21.r + a22 * p22.r + a23 * p23.r +
                         a31 * p31.r + a32 * p32.r + a33 * p33.r;

                    dest[x, y] = new ColorFloatPixel(blue + 128, green + 128, red + 128, image[x, y].a);
                }
            return dest;
        }

        public static GrayscaleFloatImage SobelMagnitude(ColorFloatImage image)
        {
            GrayscaleFloatImage dest = new GrayscaleFloatImage(image.Width, image.Height);
            List<int> a_x = new List<int>() { -1, 0, 1,
                                              -2, 2,
                                              -1, 0, 1 };

            List<int> a_y = new List<int>() { -1, -2, -1,
                                               0, 0,
                                               1, 2, 1 };

            int border_x0, border_y0, border_xn, border_yn;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    border_x0 = 0;
                    border_y0 = 0;
                    border_xn = 0;
                    border_yn = 0;

                    if (x - 1 < 0)
                        border_x0 = 1;
                    if (y - 1 < 0)
                        border_y0 = 1;
                    if (x + 1 > image.Width - 1)
                        border_xn = 1;
                    if (y + 1 > image.Height - 1)
                        border_yn = 1;

                    List<ColorFloatPixel> pix = new List<ColorFloatPixel>();

                    pix.Add(image[x - 1 + border_x0, y - 1 + border_y0]);
                    pix.Add(image[x, y - 1 + border_y0]);
                    pix.Add(image[x + 1 - border_xn, y - 1 + border_y0]);
                    pix.Add(image[x - 1 + border_x0, y]);
                    pix.Add(image[x, y]);
                    pix.Add(image[x + 1 - border_xn, y]);
                    pix.Add(image[x - 1 + border_x0, y + 1 - border_yn]);
                    pix.Add(image[x, y + 1 - border_yn]);
                    pix.Add(image[x + 1 - border_xn, y + 1 - border_yn]);

                    dest[x, y] = (float)Math.Sqrt(Convolve(a_x, pix) + Convolve(a_y, pix));
                }
            return dest;
        }

        public static ColorFloatImage Roberts(ColorFloatImage image, int diag)
        {
            ColorFloatImage dest = new ColorFloatImage(image.Width, image.Height);
            int a11 = 1, a12 = 0, a21 = 0, a22 = -1;
            if (diag == 1) // main
            {
                a11 = 1;
                a12 = 0;
                a21 = 0;
                a22 = -1;
            }
            else if (diag == 2) // other
            {
                a11 = 0;
                a12 = 1;
                a21 = -1;
                a22 = 0;
            }

            int border_xn = 0, border_yn = 0;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {

                    if (x + 1 > image.Width - 1)
                        border_xn = 1;
                    if (y + 1 > image.Height - 1)
                        border_yn = 1;

                    ColorFloatPixel p11 = image[x, y];
                    ColorFloatPixel p12 = image[x + 1 - border_xn, y];
                    ColorFloatPixel p21 = image[x, y + 1 - border_yn];
                    ColorFloatPixel p22 = image[x + 1 - border_xn, y + 1 - border_yn];


                    border_xn = 0;
                    border_yn = 0;

                    float blue =
                         a11 * p11.b + a12 * p12.b +
                         a21 * p21.b + a22 * p22.b;
                    float green =
                         a11 * p11.g + a12 * p12.g +
                         a21 * p21.g + a22 * p22.g;
                    float red =
                         a11 * p11.r + a12 * p12.r +
                         a21 * p21.r + a22 * p22.r;

                    dest[x, y] = new ColorFloatPixel(blue + 128, green + 128, red + 128, image[x, y].a);
                }
            return dest;
        }

        public static ColorFloatImage Median(ColorFloatImage image, int rad)
        {
            ColorFloatImage dest = new ColorFloatImage(image.Width, image.Height);

            int flag_i = 1, flag_j = 1;
            int flag_ni = 0, flag_nj = 0;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    float[] red = new float[(2 * rad + 1) * (2 * rad + 1)];
                    float[] green = new float[(2 * rad + 1) * (2 * rad + 1)];
                    float[] blue = new float[(2 * rad + 1) * (2 * rad + 1)];

                    int counter = 0;

                    for (int i = y - rad; i <= y + rad; i++)
                        for (int j = x - rad; j <= x + rad; j++)
                        {
                            if (i < 0) flag_i = 0;
                            if (j < 0) flag_j = 0;
                            if (i > image.Height - 1) flag_ni = i - image.Height + 1;
                            if (j > image.Width - 1) flag_nj = j - image.Width + 1;

                            red[counter] = image[j * flag_j - flag_nj, i * flag_i - flag_ni].r;
                            green[counter] = image[j * flag_j - flag_nj, i * flag_i - flag_ni].g;
                            blue[counter] = image[j * flag_j - flag_nj, i * flag_i - flag_ni].b;
                            ++counter;
                            flag_i = 1;
                            flag_j = 1;
                            flag_ni = 0;
                            flag_nj = 0;
                        }

                    Array.Sort(red);
                    Array.Sort(green);
                    Array.Sort(blue);


                    dest[x, y] = new ColorFloatPixel(blue[2 * rad * (rad - 1)],
                         green[2 * rad * (rad - 1)], red[2 * rad * (rad - 1)], image[x, y].a);
                }
            return dest;
        }

        public static double[,] Calculate2DLoGSampleKernel(double deviation, int size)
        {
            double[,] ret = new double[size, size];
            int half = size / 2;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    double r = Math.Sqrt((i - half) * (i - half) + (j - half) * (j - half));
                    ret[i, j] = ((r * r - 2 * deviation * deviation) / Math.Pow(deviation, 4)) * Math.Exp(-r * r / (2 * deviation * deviation));
                }
            }
            return ret;
        }

        public static ColorFloatImage GaussMagnitude(ColorFloatImage image, float sigma)
        {
            ColorFloatImage dest = new ColorFloatImage(image.Width, image.Height);

            float mul = 2 * sigma * sigma;
            int sm = (int)sigma;
            const float PI = (float)(Math.PI);
            float n = 1 / PI * mul;

            float[,] Gauss_matrix = new float[6 * sm + 1, 6 * sm + 1];

            float gauss_sum = 0;

            for (int j = 0; j <= 6 * sm; j++)
            {
                for (int i = 0; i <= 6 * sm; i++)
                {
                    Gauss_matrix[i, j] = n * (float)Math.Exp(-1 *
                         ((j - (3 * sm)) * (j - (3 * sm)) + (i - (3 * sm)) * (i - (3 * sm))) / mul);
                    gauss_sum += Gauss_matrix[i, j];
                }
            }

            int flag_i = 1, flag_j = 1;
            int flag_ni = 0, flag_nj = 0;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    float r = 0, g = 0, b = 0;
                    for (int j = y - 3 * sm; j <= y + 3 * sm; j++)
                        for (int i = x - 3 * sm; i <= x + 3 * sm; i++)
                        {
                            if (i < 0) flag_i = 0;
                            if (j < 0) flag_j = 0;
                            if (i > image.Width - 1) flag_ni = i - image.Width + 1;
                            if (j > image.Height - 1) flag_nj = j - image.Height + 1;

                            r += image[i * flag_i - flag_ni, j * flag_j - flag_nj].r *
                                 Gauss_matrix[i + 3 * sm - x, j + 3 * sm - y] / gauss_sum;
                            g += image[i * flag_i - flag_ni, j * flag_j - flag_nj].g *
                                 Gauss_matrix[i + 3 * sm - x, j + 3 * sm - y] / gauss_sum;
                            b += image[i * flag_i - flag_ni, j * flag_j - flag_nj].b *
                                 Gauss_matrix[i + 3 * sm - x, j + 3 * sm - y] / gauss_sum;

                            flag_i = 1;
                            flag_j = 1;
                            flag_ni = 0;
                            flag_nj = 0;
                        }

                    dest[x, y] = new ColorFloatPixel(b, g, r, image[x, y].a);

                }
            return dest;
        }


        public static ColorFloatImage Gauss(ColorFloatImage image, float sigma)
        {
            ColorFloatImage dest = new ColorFloatImage(image.Width, image.Height);

            float mul = 2 * sigma * sigma;
            int sm = (int)sigma;
            const float PI = (float)(Math.PI);
            float n = 1 / PI * mul;

            float[,] Gauss_matrix = new float[6 * sm + 1, 6 * sm + 1];

            float gauss_sum = 0;

            for (int j = 0; j <= 6 * sm; j++)
            {
                for (int i = 0; i <= 6 * sm; i++)
                {
                    Gauss_matrix[i, j] = n * (float)Math.Exp(-1 *
                         ((j - (3 * sm)) * (j - (3 * sm)) + (i - (3 * sm)) * (i - (3 * sm))) / mul);
                    gauss_sum += Gauss_matrix[i, j];
                }
            }

            int flag_i = 1, flag_j = 1;
            int flag_ni = 0, flag_nj = 0;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    float r = 0, g = 0, b = 0;
                    for (int j = y - 3 * sm; j <= y + 3 * sm; j++)
                        for (int i = x - 3 * sm; i <= x + 3 * sm; i++)
                        {
                            if (i < 0) flag_i = 0;
                            if (j < 0) flag_j = 0;
                            if (i > image.Width - 1) flag_ni = i - image.Width + 1;
                            if (j > image.Height - 1) flag_nj = j - image.Height + 1;

                            r += image[i * flag_i - flag_ni, j * flag_j - flag_nj].r *
                                 Gauss_matrix[i + 3 * sm - x, j + 3 * sm - y] / gauss_sum;
                            g += image[i * flag_i - flag_ni, j * flag_j - flag_nj].g *
                                 Gauss_matrix[i + 3 * sm - x, j + 3 * sm - y] / gauss_sum;
                            b += image[i * flag_i - flag_ni, j * flag_j - flag_nj].b *
                                 Gauss_matrix[i + 3 * sm - x, j + 3 * sm - y] / gauss_sum;

                            flag_i = 1;
                            flag_j = 1;
                            flag_ni = 0;
                            flag_nj = 0;
                        }

                    dest[x, y] = new ColorFloatPixel(b, g, r, image[x, y].a);

                }
            return dest;
        }

    }
}