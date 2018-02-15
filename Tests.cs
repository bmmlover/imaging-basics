using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using static ImageReadCS.Lab1;

namespace ImageReadCS
{
	[TestFixture]
	class Tests
	{
		[Test]
		public void TestRotation()
		{
			var dir = Path.GetDirectoryName( typeof( Program ).Assembly.Location );
			Environment.CurrentDirectory = dir;

			string[] fileEntries = Directory.GetFiles( Directory.GetCurrentDirectory() );
			string inputFileName = "input//mandarin.bmp";
			string outputFileName = "output//rotated_mandarin.bmp";

			if ( !File.Exists( inputFileName ) )
			{
				Console.WriteLine( "File doesn't exist" );
				return;
			}

			ColorFloatImage image = ImageIO.FileToColorFloatImage( inputFileName );

			image = RotateCW( image, 25 );

			ImageIO.ImageToFile( image, outputFileName );
		}
	}
}
