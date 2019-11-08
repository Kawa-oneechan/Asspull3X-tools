using System;
using System.Drawing;
using System.IO;

namespace hdma2ass
{
	static class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Use: hdma2ass <input.png> [<output.s>] [<identifier>]");
				return;
			}

			var inFile = args[0];
			var outFile = Path.ChangeExtension(inFile, ".s");
			var identifier = Path.GetFileNameWithoutExtension(inFile);
			if (args.Length > 1)
			{
				outFile = args[1];
				if (args.Length > 2)
					identifier = args[2];
			}

			if (!File.Exists(inFile))
			{
				Console.WriteLine("Input file {0} does not exist.", inFile);
				return;
			}

			var bitmap = new Bitmap(inFile);
			var hdma = new StreamWriter(outFile);
			hdma.Write(@"	.section .rodata
	.align	2
	.global {0}
{0}:", identifier);
			for (var i = 0; i < bitmap.Height; i++)
			{
				var color = bitmap.GetPixel(0, i);
				var r = color.R;
				var g = color.G;
				var b = color.B;
				var snes = ((b >> 3) << 10) | ((g >> 3) << 5) | (r >> 3);
				if (i % 8 == 0)
				{
					hdma.Write(@"
	.short ");
				}
				else
					hdma.Write(",");
				hdma.Write("0x{0:X04}", snes);
			}
			hdma.Flush();
		}
	}
}
