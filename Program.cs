using System;
using System.Collections.Generic;
using System.IO;
using static ImageReadCS.Lab1;
using static ImageReadCS.Lab2;

namespace ImageReadCS
{
	class Program
	{
		static void Main( string[] args )
		{
			string path = Directory.GetCurrentDirectory();

			if ( args.Length < 2 )
			{
				Console.WriteLine( "Empty arguments" );
				return;
			}

			string inputFileName = String.Concat( new List<string>() { "", args[ 0 ], ".bmp" } );
			string outputFileName = String.Empty;
			List<string> outputName = new List<string>() { "", "", "", "", ".bmp" };

			if ( !File.Exists( inputFileName ) )
			{
				Console.WriteLine( "File doesn't exist" );
				return;
			}

			ColorFloatImage image = ImageIO.FileToColorFloatImage( inputFileName );

			switch ( args[ 2 ] )
			{
				case "invert":
					image = InvertImage( image );
					outputName[ 1 ] = "invert";
					break;

				case "mirror":
					outputName[ 1 ] = "mirror";
					switch ( args[ 3 ] )
					{
						case "x":
							outputName[ 2 ] = "_x";
							image = MirrorX( image );
							break;
						case "y":
							outputName[ 2 ] = "_y";
							image = MirrorY( image );
							break;
					}
					break;

				case "rotate":
					outputName[ 1 ] = "rotate";
					string num = args[ 4 ].Trim( new char[] { '(', ')' } );
					outputName[ 2 ] = $"_{num}";
					int angle = 0;
					int.TryParse( num, out angle );

					switch ( args[ 3 ] )
					{
						case "cw":
							outputName[ 3 ] = "_cw";
							image = RotateCW( image, angle );
							break;
						case "ccw":
							outputName[ 3 ] = "_ccw";
							image = RotateCW( image, 360 - angle );
							break;
					}
					break;

				case "prewitt":
					outputName[ 1 ] = "prewitt";
					image = Prewitt( image, args[ 3 ] );
					break;

				case "sobel":
					outputName[ 1 ] = "sobel";
					image = Sobel( image, args[ 3 ] );
					break;

				case "roberts":
					outputName[ 1 ] = "roberts";
					string rob = args[ 3 ].Trim( new char[] { '(', ')' } );
					outputName[ 2 ] = $"_{rob}";
					int rob_param = 1;
					int.TryParse( rob, out rob_param );
					image = Roberts( image, rob_param );
					break;

				case "median":
					outputName[ 1 ] = "median";
					string med = args[ 3 ].Trim( new char[] { '(', ')' } );
					outputName[ 2 ] = $"_{med}";
					int rad = 1;
					int.TryParse( med, out rad );
					image = Median( image, rad );
					break;

				case "gauss":
					outputName[ 1 ] = "gauss";
					string g = args[ 3 ].Trim( new char[] { '(', ')' } );
					outputName[ 2 ] = $"_{g}";
					float sigma = 1;
					float.TryParse( g, out sigma );
					image = Gauss( image, sigma );
					break;
			}

			outputFileName = String.Concat( outputName );
			ImageIO.ImageToFile( image, outputFileName );
		}
	}
}
