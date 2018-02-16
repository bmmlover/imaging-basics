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
			List<string> outputFileName = new List<string>() { "mandarin_", "", ".bmp" };
			List<int> angles = new List<int>() { 0, 90, 180, 270, 45, 135, 225, 315, 360, 405, -45 };

			if ( !File.Exists( inputFileName ) )
			{
				Console.WriteLine( "File doesn't exist" );
				return;
			}

			foreach (var angle in angles )
			{
				ColorFloatImage image = ImageIO.FileToColorFloatImage( inputFileName );
				image = RotateCW( image, angle );
				outputFileName[ 1 ] = angle.ToString();
				ImageIO.ImageToFile( image, String.Concat( outputFileName ) );
			}
		}
	}
}
