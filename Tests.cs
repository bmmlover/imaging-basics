using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using static ImageReadCS.Lab1;
using static ImageReadCS.Lab2;

namespace ImageReadCS
{
	[TestFixture]
	class Tests
	{
		public static string inputname = "parrot";
		public void SetDir()
		{
			var dir = Path.GetDirectoryName( typeof( Program ).Assembly.Location );
			Environment.CurrentDirectory = dir;
		}

		public ColorFloatImage ReadImage()
		{
			string inputFileName = $"input//{inputname}.bmp";
			if ( !File.Exists( inputFileName ) )
			{
				Console.WriteLine( "File doesn't exist" );
				return null;
			}
			return ImageIO.FileToColorFloatImage( inputFileName );
		}

		public void SaveImage( ColorFloatImage image, string filename )
		{
			List<string> outputFileName = new List<string>() { $"{inputname}_", "", ".bmp" };
			outputFileName[ 1 ] = filename;
			ImageIO.ImageToFile( image, String.Concat( outputFileName ) );
		}

		[Test]
		public void TestRotation()
		{
			SetDir();

			List<int> angles = new List<int>() { 0, 90, 180, 270, 45, 135, 225, 315, 360, 405, -45 };

			foreach ( var angle in angles )
			{
				ColorFloatImage image = ReadImage();
				image = RotateCW( image, angle );
				//SaveImage( image, angle.ToString() );
			}
		}

		[Test]
		public void TestGradient()
		{
			SetDir();
			ColorFloatImage image = ReadImage();
			var nimage = Bilaterial(image, 5, 20);
			SaveImage( nimage, "bilateral" );
		}
	}
}
