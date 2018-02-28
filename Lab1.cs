using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageReadCS
{
	public class ConvolutionKernel
	{
		public float[] Kernel { get; set; }
		public float Sum { get; set; }
	}

	public enum FillMode
	{
		Constant = 1,
		Reflection = 2
	}

	public static class Lab1
	{
		public static ColorFloatImage InvertImage( ColorFloatImage image ) //ok
		{
			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					image[ x, y ] = new ColorFloatPixel( 255 - image[ x, y ].b, 255 - image[ x, y ].g,
						  255 - image[ x, y ].r, ( image[ x, y ].a ) / 2 );
				}
			return image;
		}

		public static ColorFloatImage MirrorX( ColorFloatImage image ) //ok
		{
			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width / 2; x++ )
				{
					ColorFloatPixel p = image[ x, y ];
					image[ x, y ] = image[ image.Width - 1 - x, y ];
					image[ image.Width - 1 - x, y ] = p;
				}
			return image;
		}

		public static ColorFloatImage MirrorY( ColorFloatImage image ) //ok
		{
			for ( int y = 0; y < image.Height / 2; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					ColorFloatPixel p = image[ x, y ];
					image[ x, y ] = image[ x, image.Height - 1 - y ];
					image[ x, image.Height - 1 - y ] = p;
				}
			return image;
		}

		public static double DegreeToRadian( double angle )
		{
			return ( Math.PI / 180 ) * angle;
		}

		static bool IsBlankPixel( ColorFloatPixel p )
		{
			return p.r == 0 && p.g == 0 && p.b == 0;
		}


		static ColorFloatPixel InterpolateBilinear( ColorFloatPixel p0, ColorFloatPixel p1, double delta )
		{
			return new ColorFloatPixel( (float) ( ( 1 - delta ) * p0.r + delta * p1.r ),
															 (float) ( ( 1 - delta ) * p0.g + delta * p1.g ),
															 (float) ( ( 1 - delta ) * p0.b + delta * p1.b ),
															 p1.a );
		}

		public static ColorFloatImage RotateCW( ColorFloatImage source, int angle ) //ok
		{
			ColorFloatImage dest;

			angle = angle % 360;

			switch ( angle )
			{
				case 0:
					return source;
				case 270:
					dest = new ColorFloatImage( source.Height, source.Width );
					for ( int y = 0; y < source.Height; y++ )
						for ( int x = 0; x < source.Width; x++ )
							dest[ x, y ] = source[ source.Width - 1 - y, x ];
					return dest;

				case 180:
					dest = new ColorFloatImage( source.Width, source.Height );
					for ( int y = 0; y < source.Height; y++ )
						for ( int x = 0; x < source.Width; x++ )
							dest[ x, y ] = source[ source.Width - 1 - x, source.Height - y - 1 ];
					return dest;

				case 90:
					dest = new ColorFloatImage( source.Height, source.Width );
					for ( int y = 0; y < source.Height; y++ )
						for ( int x = 0; x < source.Width; x++ )
							dest[ x, y ] = source[ y, source.Height - 1 - x ];
					return dest;
			}

			double rad = DegreeToRadian( angle );

			int destWidth = (int) Math.Ceiling( Math.Abs( source.Width * Math.Cos( rad ) ) + Math.Abs( source.Height * Math.Sin( rad ) ) );
			int destHeight = (int) Math.Ceiling( Math.Abs( source.Width * Math.Sin( rad ) ) + Math.Abs( source.Height * Math.Cos( rad ) ) );

			dest = new ColorFloatImage( destHeight, destWidth );

			int sourCenterX = source.Width / 2, sourCenterY = source.Height / 2;
			int destCenterX = destWidth / 2, destCenterY = destHeight / 2;

			for ( int yDest = 0; yDest < destHeight; yDest++ )
				for ( int xDest = 0; xDest < destWidth; xDest++ )
				{
					int xCenter = xDest - destCenterX;
					int yCenter = destCenterY - yDest;

					double ro = Math.Sqrt( xCenter * xCenter + yCenter * yCenter );
					double phi = 0;

					if ( xCenter != 0 )
						phi = Math.Atan2( yCenter, xCenter );
					else
						phi = yCenter > 0 ? 0.5 * Math.PI : 1.5 * Math.PI;

					phi += rad;

					double xSource = Math.Round( ro * Math.Cos( phi ) ) + sourCenterX;
					double ySource = sourCenterY - Math.Round( ro * Math.Sin( phi ) );

					if ( xSource < 0 || xSource >= source.Width || ySource < 0 || ySource >= source.Height )
						continue;

					var sLeft = (int) ( Math.Floor( xSource ) );
					var sTop = (int) ( Math.Floor( ySource ) );
					var sRight = (int) ( Math.Ceiling( xSource ) );
					var sBottom = (int) ( Math.Ceiling( ySource ) );

					var topPixel = InterpolateBilinear( source[ sLeft, sTop ],
						source[ sRight, sTop ], xSource - sLeft );
					var bottomPixel = InterpolateBilinear( source[ sLeft, sBottom ],
						source[ sRight, sBottom ], xSource - sLeft );

					dest[ xDest, yDest ] = InterpolateBilinear( topPixel, bottomPixel, ySource - sTop );
				}
			return dest;
		}

		public static ColorFloatPixel Convolve( float[] coef, List<ColorFloatPixel> pix, float divider )
		{
			float red = 0, green = 0, blue = 0;

			for ( int i = 0; i < pix.Count; i++ )
			{
				red += coef[ i ] * pix[ i ].r / divider;
				green += coef[ i ] * pix[ i ].g / divider;
				blue += coef[ i ] * pix[ i ].b / divider;
			}

			return new ColorFloatPixel( blue, green, red, pix[ 0 ].a );
		}


        public static float ConvolveGray(float[] coef, List<float> pix, float divider)
        {
            float res = 0;

            for (int i = 0; i < pix.Count; i++)
                res += coef[i] * pix[i] / divider;

            return res;
        }


		public static ColorFloatImage Sobel( ColorFloatImage image, string arg ) //todo rewrite
		{
			int axes = 0;
			if ( arg == "y" )
				axes = 1;
			ColorFloatImage dest = new ColorFloatImage( image.Width, image.Height );
			int a12 = 0, a13 = 1, a21 = -2, a22 = 0, a23 = 2, a31 = -1, a32 = 0;
			int a11 = -1;
			int a33 = 1;
			if ( axes == 0 ) // Gx
			{
				a12 = 0;
				a13 = 1;
				a21 = -2;
				a22 = 0;
				a23 = 2;
				a31 = -1;
				a32 = 0;
			}
			else if ( axes == 1 ) // Gy
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

			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					if ( x - 1 < 0 )
						border_x0 = 1;
					if ( y - 1 < 0 )
						border_y0 = 1;
					if ( x + 1 > image.Width - 1 )
						border_xn = 1;
					if ( y + 1 > image.Height - 1 )
						border_yn = 1;

					ColorFloatPixel p11 = image[ x - 1 + border_x0, y - 1 + border_y0 ];
					ColorFloatPixel p12 = image[ x, y - 1 + border_y0 ];
					ColorFloatPixel p13 = image[ x + 1 - border_xn, y - 1 + border_y0 ];
					ColorFloatPixel p21 = image[ x - 1 + border_x0, y ];
					ColorFloatPixel p22 = image[ x, y ];
					ColorFloatPixel p23 = image[ x + 1 - border_xn, y ];
					ColorFloatPixel p31 = image[ x - 1 + border_x0, y + 1 - border_yn ];
					ColorFloatPixel p32 = image[ x, y + 1 - border_yn ];
					ColorFloatPixel p33 = image[ x + 1 - border_xn, y + 1 - border_yn ];

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

					dest[ x, y ] = new ColorFloatPixel( blue + 128, green + 128, red + 128, image[ x, y ].a );
				}
			return dest;
		}

		public static GrayscaleFloatImage RGB2Gray( ColorFloatImage image )
		{
			GrayscaleFloatImage gray = new GrayscaleFloatImage( image.Width, image.Height );

			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					gray[ x, y ] = RGB2GrayPix( image[ x, y ] );
				}
			return gray;
		}

		public static float RGB2GrayPix( ColorFloatPixel p )
		{
			return (float) ( p.r * 0.299 + p.g * 0.587 + p.b * 0.114 );
		}

		public static readonly List<int> sobelWindowX = new List<int>() { -1, 0, 1,
															 -2, 0, 2,
															 -1, 0, 1 };

		public static readonly List<int> sobelWindowY = new List<int>() { -1, -2, -1,
															  0, 0, 0,
															  1, 2, 1 };

		public static List<int> NeighbourIndexes( int value, int max, int halfWindow, FillMode mode )
		{
			int leftBoundary = value - halfWindow;
			int rightBoundary = value + halfWindow;
			if ( leftBoundary >= 0 && rightBoundary <= max )
			{
				return Enumerable.Range( leftBoundary, 2 * halfWindow + 1 ).ToList();
			}

			List<int> leftPart = new List<int>();
			List<int> rightPart = new List<int>();

			if ( mode == FillMode.Constant )
			{
				if ( leftBoundary <= 0 )
				{
					leftPart = Enumerable.Repeat( 0, -leftBoundary + 1 ).ToList();
					rightPart = Enumerable.Range( 1, rightBoundary ).ToList();
				}
				else
				{
					leftPart = Enumerable.Range( leftBoundary, max - leftBoundary + 1 ).ToList();
					rightPart = Enumerable.Repeat( max, rightBoundary - max ).ToList();
				}
			}
			else if ( mode == FillMode.Reflection )
			{
				if ( leftBoundary <= 0 )
				{
					leftPart = Enumerable.Range( 0, -leftBoundary + 1 ).Reverse().ToList();
					rightPart = Enumerable.Range( 1, rightBoundary ).ToList();
				}
				else
				{
					leftPart = Enumerable.Range( leftBoundary, max - leftBoundary + 1 ).ToList();
					rightPart = Enumerable.Range( 2 * max - rightBoundary, rightBoundary - max ).Reverse().ToList();
				}
			}
			return leftPart.Concat( rightPart ).ToList();
		}

		public static float FitAngleInBin( double radian )
		{
			if ( radian < 0 || radian > Math.PI )
				throw new Exception( radian.ToString() );
			if ( radian <= Math.PI / 8 || radian > 7 * Math.PI / 8 )
				return 0;
			if ( radian > Math.PI / 8 && radian <= 3 * Math.PI / 8 )
				return 45;
			if ( radian > 3 * Math.PI / 8 && radian <= 5 * Math.PI / 8 )
				return 90;
			return 135;
		}

		public static List<GrayscaleFloatImage> MagnitudeAndDirections( ColorFloatImage image,
			ConvolutionKernel xKernel, ConvolutionKernel yKernel )
		{
			var xWindow = xKernel.Kernel;
			var yWindow = yKernel.Kernel;
			GrayscaleFloatImage magn = new GrayscaleFloatImage( image.Width, image.Height );
			GrayscaleFloatImage angles = new GrayscaleFloatImage( image.Width, image.Height );

			int windowSide = (int) Math.Pow( xWindow.Length, 0.5 );
			int halfWindowSide = ( windowSide - 1 ) / 2;

			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					List<int> i = NeighbourIndexes( x, image.Width - 1, halfWindowSide, FillMode.Reflection );
					List<int> j = NeighbourIndexes( y, image.Height - 1, halfWindowSide, FillMode.Reflection );
					List<float> pix = new List<float>();

					for ( int k = 0; k < windowSide; k++ )
						for ( int n = 0; n < windowSide; n++ )
							pix.Add( RGB2GrayPix( image[ i[ n ], j[ k ] ] ) );

					float xPix = ConvolveGray( xWindow, pix, xKernel.Sum );
					float yPix = ConvolveGray( yWindow, pix, yKernel.Sum );

					magn[ x, y ] = (float) Math.Sqrt( Math.Pow( xPix, 2 ) + Math.Pow( yPix, 2 ) );
					angles[ x, y ] = FitAngleInBin( Math.Abs( Math.Atan2( yPix, xPix ) ) );
				}
			return new List<GrayscaleFloatImage>() { magn, angles };
		}

		public static ColorFloatImage Gradient( ColorFloatImage image, float[] window, float divider )
		{
			ColorFloatImage dest = new ColorFloatImage( image.Width, image.Height );

			int windowSide = (int) Math.Pow( window.Length, 0.5 );
			int halfWindowSide = ( windowSide - 1 ) / 2;

			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					List<int> i = NeighbourIndexes( x, image.Width - 1, halfWindowSide, FillMode.Reflection );
					List<int> j = NeighbourIndexes( y, image.Height - 1, halfWindowSide, FillMode.Reflection );
					List<ColorFloatPixel> pix = new List<ColorFloatPixel>();

					for ( int k = 0; k < windowSide; k++ )
						for ( int n = 0; n < windowSide; n++ )
							pix.Add( image[ i[ n ], j[ k ] ] );

					dest[ x, y ] = Convolve( window, pix, divider );
				}

			return dest;
		}

		public static GrayscaleFloatImage GradientGrayscale( ColorFloatImage image, float[] window, float divider )
		{
			GrayscaleFloatImage dest = new GrayscaleFloatImage( image.Width, image.Height );

			int windowSide = (int) Math.Pow( window.Length, 0.5 );
			int halfWindowSide = ( windowSide - 1 ) / 2;

			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					List<int> i = NeighbourIndexes( x, image.Width - 1, halfWindowSide, FillMode.Reflection );
					List<int> j = NeighbourIndexes( y, image.Height - 1, halfWindowSide, FillMode.Reflection );
					List<ColorFloatPixel> pix = new List<ColorFloatPixel>();

					for ( int k = 0; k < windowSide; k++ )
						for ( int n = 0; n < windowSide; n++ )
							pix.Add( image[ i[ n ], j[ k ] ] );

					dest[ x, y ] = RGB2GrayPix( Convolve( window, pix, divider ) );// + 128;
				}

			return dest;
		}

		public static ColorFloatImage Median( ColorFloatImage image, int rad ) //todo rewrite
		{
			ColorFloatImage dest = new ColorFloatImage( image.Width, image.Height );

			int flag_i = 1, flag_j = 1;
			int flag_ni = 0, flag_nj = 0;

			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					float[] red = new float[ ( 2 * rad + 1 ) * ( 2 * rad + 1 ) ];
					float[] green = new float[ ( 2 * rad + 1 ) * ( 2 * rad + 1 ) ];
					float[] blue = new float[ ( 2 * rad + 1 ) * ( 2 * rad + 1 ) ];

					int counter = 0;

					for ( int i = y - rad; i <= y + rad; i++ )
						for ( int j = x - rad; j <= x + rad; j++ )
						{
							if ( i < 0 ) flag_i = 0;
							if ( j < 0 ) flag_j = 0;
							if ( i > image.Height - 1 ) flag_ni = i - image.Height + 1;
							if ( j > image.Width - 1 ) flag_nj = j - image.Width + 1;

							red[ counter ] = image[ j * flag_j - flag_nj, i * flag_i - flag_ni ].r;
							green[ counter ] = image[ j * flag_j - flag_nj, i * flag_i - flag_ni ].g;
							blue[ counter ] = image[ j * flag_j - flag_nj, i * flag_i - flag_ni ].b;
							++counter;
							flag_i = 1;
							flag_j = 1;
							flag_ni = 0;
							flag_nj = 0;
						}

					Array.Sort( red );
					Array.Sort( green );
					Array.Sort( blue );


					dest[ x, y ] = new ColorFloatPixel( blue[ 2 * rad * ( rad - 1 ) ],
						  green[ 2 * rad * ( rad - 1 ) ], red[ 2 * rad * ( rad - 1 ) ], image[ x, y ].a );
				}
			return dest;
		}

		public static float LoGPoint( int x, int y, double sigma )
		{
			double mul = 2 * sigma * sigma;
			double r = Math.Pow( x, 2 ) + Math.Pow( y, 2 );
			return (float) ( ( r - mul ) / Math.Pow( sigma, 4 ) * Math.Exp( -r / mul ) );
		}

		public static float GaussPoint( int x, int y, double sigma )
		{
			double mul = 2 * sigma * sigma;
			double piMul = 1 / Math.PI * mul;
			double r = Math.Pow( x, 2 ) + Math.Pow( y, 2 );
			return (float) ( piMul * Math.Exp( -r / mul ) );
		}

        public static float GaussPartBilaterialPoint(int x, int y, double sigma)
        {
            return (float)((Math.Pow(x, 2) + Math.Pow(y, 2)) / (2 * sigma * sigma));
        }

		public static float GaussDerivativePoint( int x, int y, double sigma )
		{
			double mul = 2 * sigma * sigma;
			double piMul = 1 / Math.PI * mul;
			double r = Math.Pow( x, 2 ) + Math.Pow( y, 2 );
			return (float) ( piMul * x * Math.Exp( -r / mul ) );
		}

		public static ConvolutionKernel CalculateKernel( double sigma,
									    Func<int, int, double, float> function ) //todo check formula
		{
			int sm = (int) sigma;
			int half = 3 * sm;
			int size = 6 * sm + 1;

			ConvolutionKernel kernel = new ConvolutionKernel();
			kernel.Kernel = new float[ size * size ];
			float sum = 0;

			for ( int j = 0; j < size; j++ )
				for ( int i = 0; i < size; i++ )
				{
					float val = function( i - half, j - half, sigma );
					kernel.Kernel[ j * size + i ] = val;
					sum += val;
				}

			kernel.Sum = sum;
			return kernel;
		}

		public static List<ConvolutionKernel> CalculateKernelXY( double sigma,
										Func<int, int, double, float> function ) //todo check formula
		{
			int sm = (int) sigma;
			int half = 3 * sm;
			int size = 6 * sm + 1;

			ConvolutionKernel kernelX = new ConvolutionKernel();
			kernelX.Kernel = new float[ size * size ];
			kernelX.Sum = 0;

			ConvolutionKernel kernelY = new ConvolutionKernel();
			kernelY.Kernel = new float[ size * size ];
			kernelY.Sum = 0;

			for ( int j = 0; j < size; j++ )
				for ( int i = 0; i < size; i++ )
				{
					var valX = function( i - half, j - half, sigma );
					var valY = function( j - half, i - half, sigma );
					kernelX.Kernel[ j * size + i ] = valX;
					kernelY.Kernel[ j * size + i ] = valY;
					kernelX.Sum += Math.Abs(valX);
					kernelY.Sum += Math.Abs(valY);
				}

			return new List<ConvolutionKernel>() { kernelX, kernelY };
		}

		public static ColorFloatImage Gauss( ColorFloatImage image, float sigma )
		{
			ConvolutionKernel kernel = CalculateKernel( sigma, GaussPoint );
			return Gradient( image, kernel.Kernel, kernel.Sum );
		}

		public static GrayscaleFloatImage GaussMagnitude( ColorFloatImage image, float sigma )
		{
			ConvolutionKernel kernel = CalculateKernel( sigma, LoGPoint );
			return GradientGrayscale( image, kernel.Kernel, 1 );
		}

	}
}
