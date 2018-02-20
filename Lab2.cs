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
			double K1 = 0.01, K2 = 0.03;
			int L = 255;

			var C1 = Math.Pow( K1 * L, 2 );
			var C2 = Math.Pow( K2 * L, 2 );
			var mu1 = 0;//signal.fftconvolve(window, img1, mode = 'valid');
			var mu2 = 0;//signal.fftconvolve(window, img2, mode = 'valid');
			var mu1_sq = mu1 * mu1;
			var mu2_sq = mu2 * mu2;
			var mu1_mu2 = mu1 * mu2;
			var sigma1_sq = 0;//signal.fftconvolve(window, img1 * img1, mode = 'valid') - mu1_sq;
			var sigma2_sq = 0;//signal.fftconvolve(window, img2 * img2, mode = 'valid') - mu2_sq;
			var sigma12 = 0;//signal.fftconvolve(window, img1 * img2, mode = 'valid') - mu1_mu2;

			return ( ( 2 * mu1_mu2 + C1 ) * ( 2 * sigma12 + C2 ) ) / ( ( mu1_sq + mu2_sq + C1 ) *
																				 ( sigma1_sq + sigma2_sq + C2 ) );

		}
		public static double MSSIM( ColorFloatImage i1, ColorFloatImage i2 )
		{
			return 0;
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
		public static double Gabor( ColorFloatImage i1 )
		{
			return 0;
		}
	}
}
