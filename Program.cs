using System;
using System.Collections.Generic;
using System.IO;
using static ImageReadCS.Lab1;
using static ImageReadCS.Lab2;

namespace ImageReadCS
{
	class Program
	{
		static List<string> _lab1 = new List<string>()
		{
			"mirror",
			"rotate",
			"sobel",
			"median",
			"gauss",
			"gradient",
		};

		static List<string> _lab2 = new List<string>()
		{
			"mse",
			"psnr",
			"ssim",
			"mssim",
			"canny",
			"gabor",
			"vessels"
		};

		static ColorFloatImage ReadImage( string filename )
		{
			if ( !File.Exists( filename ) )
				throw new Exception( "File doesn't exist" );

			return ImageIO.FileToColorFloatImage( filename );
		}

		static float GetNumericArg( string text )
		{
			string str = text.Trim( new char[] { '(', ')' } );
			float num = 0;
			float.TryParse( str, out num );
			return num;
		}

		static void CompareImages(string i1, string i2, Func<ColorFloatImage, ColorFloatImage, double> metric)
		{
			ColorFloatImage inputImage = ReadImage( i1 );
			ColorFloatImage imageToCompare = ReadImage( i2 );
			double res = metric( inputImage, imageToCompare );
			if ( Double.IsInfinity( res ) )
				Console.WriteLine( "Infinity" );
			else
				Console.WriteLine( res );
		}

		static void Main( string[] args )
		{
			//string path = Directory.GetCurrentDirectory();

			if ( args.Length < 2 )
			{
				Console.WriteLine( "Empty arguments" );
				return;
			}

			//string inputFileName = String.Concat( new List<string>() { "input//", args[ 0 ], ".bmp" } );
			string outputFileName = String.Empty;
			List<string> outputName = new List<string>() { "", "", "", "", ".bmp" };

			ColorFloatImage inputImage;

			#region lab1

			if ( _lab1.Contains( args[ 1 ] ) )
			{
				ColorFloatImage outputImage = null;

				switch ( args[ 1 ] )
				{

					case "mirror":

						outputName[ 1 ] = "mirror";
						inputImage = ReadImage( args[ 3 ] );

						switch ( args[ 2 ] )
						{
							case "x":
								outputName[ 2 ] = "_x";
								outputImage = MirrorX( inputImage );
								break;
							case "y":
								outputName[ 2 ] = "_y";
								outputImage = MirrorY( inputImage );
								break;
						}
						break;

					case "rotate":

						outputName[ 1 ] = "rotate";
						int angle = (int) GetNumericArg( args[ 3 ] );
						inputImage = ReadImage( args[ 4 ] );

						switch ( args[ 2 ] )
						{
							case "cw":
								outputName[ 3 ] = "_cw";
								outputImage = RotateCW( inputImage, angle );
								break;
							case "ccw":
								outputName[ 3 ] = "_ccw";
								outputImage = RotateCW( inputImage, 360 - angle );
								break;
						}
						break;

					case "sobel":
						outputName[ 1 ] = "sobel";
						outputName[ 2 ] = $"_{args[2]}";
						inputImage = ReadImage( args[ 3 ] );
						outputImage = Sobel( inputImage, args[ 2 ] );
						break;

					case "median":
						outputName[ 1 ] = "median";
						inputImage = ReadImage( args[ 3 ] );
						int rad = (int) GetNumericArg( args[ 2 ] );
						outputImage = Median( inputImage, rad );
						break;

					case "gauss":
						outputName[ 1 ] = "gauss";
						float sigma = GetNumericArg( args[ 2 ] );
						inputImage = ReadImage( args[ 3 ] );
						outputImage = Gauss( inputImage, sigma );
						break;

					case "gradient":
						outputName[ 1 ] = "gradient";
						sigma = GetNumericArg( args[ 2 ] );
						inputImage = ReadImage( args[ 3 ] );
						GrayscaleFloatImage newOutputImage = GaussMagnitude( inputImage, sigma );
						outputFileName = String.Concat( outputName );
						ImageIO.ImageToFile( newOutputImage, outputFileName );
						break;
				}

				if (outputImage != null )
				{
					outputFileName = String.Concat( outputName );
					ImageIO.ImageToFile( outputImage, outputFileName );
				}
				return;
			}

			#endregion

			#region lab2

			if ( _lab2.Contains( args[ 1 ] ) )
			{
				GrayscaleFloatImage outputImage = null;

				switch ( args[ 1 ] )
				{
					case "mse":
						CompareImages( args[ 2 ], args[ 3 ], MSE );
						break;

					case "psnr":
						CompareImages( args[ 2 ], args[ 3 ], PSNR );
						break;

					case "ssim":
						CompareImages( args[ 2 ], args[ 3 ], SSIM );
						break;

					case "mssim":
						CompareImages( args[ 2 ], args[ 3 ], MSSIM );
						break;

					case "canny":
						outputName[ 1 ] = "canny";
						float sigma = GetNumericArg( args[ 2 ] );
						float thrHigh = GetNumericArg( args[ 3 ] );
						float thrLow = GetNumericArg( args[ 4 ] );
						inputImage = ReadImage( args[ 5 ] );
						outputImage = Canny( inputImage, sigma, thrHigh, thrLow );
						break;

					case "gabor":
						outputName[ 1 ] = "gabor";
						sigma = GetNumericArg( args[ 2 ] );
						float gamma = GetNumericArg( args[ 3 ] );
						float theta = GetNumericArg( args[ 4 ] );
						float lambda = GetNumericArg( args[ 5 ] );
						float psi = GetNumericArg( args[ 6 ] );
						inputImage = ReadImage( args[ 7 ] );
						outputImage = Gabor( inputImage, sigma, gamma, theta, lambda, psi );
						break;

					case "vessels":
						outputName[ 1 ] = "vessels";
						sigma = GetNumericArg( args[ 2 ] );
						inputImage = ReadImage( args[ 3 ] );
						outputImage = Vessels( inputImage, sigma );
						break;
				}

				if (outputImage != null )
				{
					outputFileName = String.Concat( outputName );
					ImageIO.ImageToFile( outputImage, outputFileName );
				}
				return;
			}
			#endregion
		}
	}
}
