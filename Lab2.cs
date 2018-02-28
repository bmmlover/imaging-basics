using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using static ImageReadCS.Lab1;

namespace ImageReadCS
{
	public class Point
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	public class ImageEdges
	{
		public GrayscaleFloatImage Image;
		public List<Point> Points;
	}

	public static class Lab2
	{
		public static double MSE( ColorFloatImage i1, ColorFloatImage i2 )
		{
			if ( i1.Height != i2.Height || i1.Width != i2.Width )
				return 0;

			double mse = 0;

			for ( int y = 0; y < i1.Height; y++ )
				for ( int x = 0; x < i1.Width; x++ )
					mse += Math.Pow( RGB2GrayPix( i1[ x, y ] ) - RGB2GrayPix( i2[ x, y ] ), 2 );

			return mse / ( i1.Width * i1.Height * 3 );
		}

		public static double PSNR( ColorFloatImage i1, ColorFloatImage i2 )
		{
			return 20 * Math.Log10( 255.0 / Math.Sqrt( MSE( i1, i2 ) ) );
		}

		public static double SSIM( ColorFloatImage i1, ColorFloatImage i2 )
		{
			if ( i1.Width != i2.Width || i1.Height != i2.Height )
				return -10;

			double K1 = 0.01, K2 = 0.03;
			int L = 255;
			var C1 = Math.Pow( K1 * L, 2 );
			var C2 = Math.Pow( K2 * L, 2 );

			double mu1 = 0;
			double mu2 = 0;
			double sigma1_sq = 0;
			double sigma2_sq = 0;
			double sigma12 = 0;
			double wh = i1.Width * i1.Height;

			for ( int y = 0; y < i1.Height; y++ )
				for ( int x = 0; x < i1.Width; x++ )
				{
					mu1 += RGB2GrayPix( i1[ x, y ] );
					mu2 += RGB2GrayPix( i2[ x, y ] );
				}

			mu1 = mu1 / wh;
			mu2 = mu2 / wh;

			for ( int y = 0; y < i1.Height; y++ )
				for ( int x = 0; x < i1.Width; x++ )
				{
					var d1 = RGB2GrayPix( i1[ x, y ] ) - mu1;
					var d2 = RGB2GrayPix( i2[ x, y ] ) - mu2;
					sigma1_sq += Math.Pow( d1, 2 );
					sigma2_sq += Math.Pow( d2, 2 );
					sigma12 += d1 * d2;
				}

			sigma1_sq = sigma1_sq / ( wh - 1 );
			sigma2_sq = sigma2_sq / ( wh - 1 );
			sigma12 = sigma12 / ( wh - 1 );

			return ( ( 2 * mu1 * mu2 + C1 ) * ( 2 * sigma12 + C2 ) ) /
				 ( ( mu1 * mu1 + mu2 * mu2 + C1 ) * ( sigma1_sq + sigma2_sq + C2 ) );

		}

		public static double SSIM( List<float> i1, List<float> i2 )
		{
			if ( i1.Count != i2.Count )
				return -10;

			double K1 = 0.01, K2 = 0.03;
			int L = 255;
			var C1 = Math.Pow( K1 * L, 2 );
			var C2 = Math.Pow( K2 * L, 2 );

			double mu1 = 0;
			double mu2 = 0;
			double sigma1_sq = 0;
			double sigma2_sq = 0;
			double sigma12 = 0;
			double wh = i1.Count;

			for ( int x = 0; x < i1.Count; x++ )
			{
				mu1 += i1[ x ];
				mu2 += i2[ x ];
			}

			mu1 = mu1 / wh;
			mu2 = mu2 / wh;

			for ( int x = 0; x < i1.Count; x++ )
			{
				var d1 = i1[ x ] - mu1;
				var d2 = i2[ x ] - mu2;
				sigma1_sq += Math.Pow( d1, 2 );
				sigma2_sq += Math.Pow( d2, 2 );
				sigma12 += d1 * d2;
			}

			sigma1_sq = sigma1_sq / ( wh - 1 );
			sigma2_sq = sigma2_sq / ( wh - 1 );
			sigma12 = sigma12 / ( wh - 1 );

			return ( ( 2 * mu1 * mu2 + C1 ) * ( 2 * sigma12 + C2 ) ) /
				 ( ( mu1 * mu1 + mu2 * mu2 + C1 ) * ( sigma1_sq + sigma2_sq + C2 ) );

		}

		public static double MSSIM( ColorFloatImage i1, ColorFloatImage i2 )
		{
			List<double> SSIMvalues = new List<double>();
			int step = 4;

			for ( int y = 4; y < i1.Height - 4; y = y + step )
				for ( int x = 4; x < i1.Width - 4; x = x + step )
				{
					var xList = Enumerable.Range( x - 4, 8 ).ToList();
					var yList = Enumerable.Range( y - 4, 8 ).ToList();

					var points = xList
						 .SelectMany( i => yList.Select( j => new Point() { X = i, Y = j } ) )
						 .ToList();

					List<float> i1Pix = new List<float>();
					List<float> i2Pix = new List<float>();

					foreach ( var point in points )
					{
						i1Pix.Add( RGB2GrayPix( i1[ point.X, point.Y ] ) );
						i2Pix.Add( RGB2GrayPix( i2[ point.X, point.Y ] ) );
					}

					SSIMvalues.Add( SSIM( i1Pix, i2Pix ) );
				}

			return SSIMvalues.Sum() / SSIMvalues.Count;

		}

		public static ImageEdges NonMaximumSuppression( GrayscaleFloatImage magnitude,
																		GrayscaleFloatImage directions,
																		float tMax )
		{
			ImageEdges dest = new ImageEdges();
			dest.Image = new GrayscaleFloatImage( magnitude.Width, magnitude.Height );
			dest.Points = new List<Point>();
			List<float> allvals = new List<float>();

			for ( int y = 0; y < magnitude.Height; y++ )
				for ( int x = 0; x < magnitude.Width; x++ )
				{
					allvals.Add( magnitude[ x, y ] );
					int direction = (int) directions[ x, y ];

					var pointList = new List<Point>();
					var xList = Enumerable.Range( x - 1, 3 ).ToList();
					var yList = Enumerable.Range( y - 1, 3 ).ToList();

					if ( direction == 0 )
						pointList = xList.Where( n => n >= 0 && n <= magnitude.Width - 1 )
													.Select( n => new Point() { X = n, Y = y } )
													.ToList();
					else if ( direction == 90 )
						pointList = yList.Where( n => n >= 0 && n <= magnitude.Height - 1 )
													.Select( n => new Point() { X = x, Y = n } )
													.ToList();
					else
					{
						if ( direction == 45 )
							xList.Reverse();

						pointList = xList.Zip( yList, ( first, second ) => new Point() { X = first, Y = second } )
											  .Where( p => p.X >= 0 && p.X <= magnitude.Width - 1 &&
																	  p.Y >= 0 && p.Y <= magnitude.Height - 1 )
											  .ToList();
					}

					var values = new List<float>();

					foreach ( var point in pointList )
						values.Add( magnitude[ point.X, point.Y ] );

					if ( values.Max() > magnitude[ x, y ] )
						dest.Image[ x, y ] = 0;
					else
					{
						dest.Image[ x, y ] = magnitude[ x, y ];
						if ( dest.Image[ x, y ] >= tMax )
							dest.Points.Add( new Point() { X = x, Y = y } );
					}

				}

			var a = allvals.Max();
			var b = allvals.Min();
			return dest;
		}

		public static GrayscaleFloatImage Hysteresis( ImageEdges edges, float tMin )
		{
			var magnitude = edges.Image;

			GrayscaleFloatImage dest = new GrayscaleFloatImage( magnitude.Width, magnitude.Height );

			float maxBrightness = 256;

			var queue = new Queue<Point>( edges.Points );

			while ( queue.Count > 0 )
			{
				var currentPoint = queue.Dequeue();
				var x = currentPoint.X;
				var y = currentPoint.Y;

				dest[ x, y ] = maxBrightness;
				var xList = Enumerable.Range( x - 1, 3 ).ToList();
				var yList = Enumerable.Range( y - 1, 3 ).ToList();

				var points = xList.SelectMany( i => yList.Select( j => new Point() { X = i, Y = j } ) )
					  .Where( p => p.X >= 0 && p.X <= magnitude.Width - 1 &&
								 p.Y >= 0 && p.Y <= magnitude.Height - 1 ).ToList();

				foreach ( var point in points )
					if ( dest[ point.X, point.Y ] == 0 && magnitude[ point.X, point.Y ] >= tMin )
						queue.Enqueue( point );

			}

			return dest;
		}

		public static GrayscaleFloatImage Canny( ColorFloatImage image, float sigma, float tMax, float tMin )
		{
			var kernels = CalculateKernelXY( sigma, GaussDerivativePoint );
			var magnAndDir = MagnitudeAndDirections( image, kernels[ 0 ], kernels[ 1 ] );
			var magnitude = magnAndDir[ 0 ];
			var directions = magnAndDir[ 1 ];
			tMax *= 256;
			tMin *= 256;
			var withoutNonMax = NonMaximumSuppression( magnitude, directions, tMax );
			return Hysteresis( withoutNonMax, tMin );
		}

		public static float GaborPoint( int x, int y, List<double> param )
		{
			double _x = x * Math.Cos( param[ 2 ] ) + y * Math.Sin( param[ 2 ] );
			double _y = -x * Math.Sin( param[ 2 ] ) + y * Math.Cos( param[ 2 ] );
			double mul = 2 * param[ 0 ] * param[ 0 ];
			double r1 = Math.Pow( _x, 2 ) + Math.Pow( _y * param[ 1 ], 2 );
			double r2 = Math.Cos( 2 * Math.PI * _x / param[ 3 ] + param[ 4 ] );
			return (float) ( r2 * Math.Exp( -r1 / mul ) );
		}

		// 0 sigma, 1 gamma, 2 theta, 3 lambda, 4 psi

		public static ConvolutionKernel CalculateGaborKernel( List<double> param,
										  Func<int, int, List<double>, float> function ) //todo check formula
		{
			int sm = (int) param[ 0 ];
			int half = 3 * sm;
			int size = 6 * sm + 1;

			ConvolutionKernel kernel = new ConvolutionKernel();
			kernel.Kernel = new float[ size * size ];
			float sum = 0;

			for ( int j = 0; j < size; j++ )
				for ( int i = 0; i < size; i++ )
				{
					float val = function( i - half, j - half, param );
					kernel.Kernel[ j * size + i ] = val;
					sum += val;
				}

			kernel.Sum = sum;
			return kernel;
		}

		public static GrayscaleFloatImage Gabor( ColorFloatImage image, double sigma,
											double gamma, double theta, double lambda,
											double psi )
		{
			List<double> param = new List<double>();
			param.Add( sigma );
			param.Add( gamma );
			param.Add( theta );
			param.Add( lambda );
			param.Add( psi );

			ConvolutionKernel kernel = CalculateGaborKernel( param, GaborPoint );
			return GradientGrayscale( image, kernel.Kernel, 1 );
		}

		public static GrayscaleFloatImage Vessels( ColorFloatImage image, double sigma )
		{
			double angle = 0;

			List<ConvolutionKernel> gaborBank = new List<ConvolutionKernel>();

			List<double> param = new List<double>();
			param.Add( sigma );
			param.Add( 1 );
			param.Add( angle );
			param.Add( 6 );
			param.Add( 0 );

			List<double> lambdas = new List<double>() { 0.5, 1, 1.5 };

			for ( int i = 0; i < 8; i++ )
			{
				foreach ( var lambda in lambdas )
				{
					param[ 3 ] = lambda;
					gaborBank.Add( CalculateGaborKernel( param, GaborPoint ) );
				}
				param[ 2 ] += Math.PI / 8;
			}

			GrayscaleFloatImage dest = new GrayscaleFloatImage( image.Width, image.Height );

			int windowSide = (int) Math.Pow( gaborBank[ 0 ].Kernel.Length, 0.5 );
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

					List<double> res = new List<double>();

					foreach ( var kernel in gaborBank )
					{
						res.Add( ConvolveGray( kernel.Kernel, pix ) );
					}


					var resList = res.Where( r => r < 0 ).Select( r => Math.Abs( r ) ).ToList();

					if ( resList.Count > 0 )
						dest[ x, y ] = (float) resList.Max();
					else
						dest[ x, y ] = 0;

				}
			return dest;

		}

		public static double NormSqL2( float x, float y )
		{
			return Math.Pow( x, 2 ) + Math.Pow( y, 2 );
		}

		public static double NormSqPixel( ColorFloatPixel x, ColorFloatPixel y )
		{
			return Math.Pow( x.r - y.r, 2 ) + Math.Pow( x.g - y.g, 2 ) + Math.Pow( x.b - y.b, 2 );
		}

		public static float BilaterialPoint( int x, int y, double sigma_d, ColorFloatPixel Ix, ColorFloatPixel Iy, double sigma_r )
		{
			double mul_d = 2 * sigma_d * sigma_d;
			double mul_r = 2 * sigma_r * sigma_r;

			var a = Math.Exp( -NormSqL2( x, y ) / mul_d - NormSqPixel( Ix, Iy ) / mul_r );
			return (float) a;
		}

		public static ColorFloatImage Bilaterial( ColorFloatImage image, double sigma_d, double sigma_r )
		{
			ColorFloatImage dest = new ColorFloatImage( image.Width, image.Height );

			//ConvolutionKernel gaussPartKernel = CalculateKernel( sigma_d, GaussPartBilaterialPoint );
			//var window = gaussPartKernel.Kernel;
			Dictionary<string, float> intensityDistances = new Dictionary<string, float>();

			int windowSide = 6 * (int)sigma_d + 1;// (int) Math.Pow( window.Length, 0.5 );
			int halfWindowSide = ( windowSide - 1 ) / 2;

			for ( int y = 0; y < image.Height; y++ )
				for ( int x = 0; x < image.Width; x++ )
				{
					List<int> i = NeighbourIndexes( x, image.Width - 1, halfWindowSide, FillMode.Constant );
					List<int> j = NeighbourIndexes( y, image.Height - 1, halfWindowSide, FillMode.Constant );
					List<ColorFloatPixel> pix = new List<ColorFloatPixel>();

					var currentPixel = image[ x, y ];
					//var newWindow = (float[]) window.Clone();
					var newWindow = new List<float>();

					for ( int k = 0; k < windowSide; k++ )
						for ( int n = 0; n < windowSide; n++ )
						{
							pix.Add( image[ i[ n ], j[ k ] ] );
							var coords = new int[] { x, y, i[ n ], j[ k ] };
							Array.Sort( coords );
							string hashCode = String.Format( "{0:X}", String.Concat( coords
																.Select( val => val.ToString() ) ).GetHashCode() );
							float dist = 0;
							if ( intensityDistances.ContainsKey( hashCode ) )
								dist = intensityDistances[ hashCode ];
							else
							{
								//dist = (float) NormL2( RGB2GrayPix( image[ x, y ] ), RGB2GrayPix( image[ i[ n ], j[ k ] ] ) );
								intensityDistances.Add( hashCode, dist );
							}
							newWindow.Add( BilaterialPoint( x - i[ n ], y - j[ k ], sigma_d, image[ x, y ], image[ i[ n ], j[ k ] ], sigma_r ) );
							//newWindow[ pix.Count - 1 ] = (float) ( 1 / Math.PI * mul_d *Math.Exp( window[ pix.Count - 1 ] ));// - dist * mul_r);

						}
					var c = newWindow.Where( v => v < 0 ).ToList();
					dest[ x, y ] = Convolve( newWindow.ToArray(), pix, newWindow.Sum() );
				}

			return dest;
		}
	}
}
