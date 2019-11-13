using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace img2ass
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Use: img2ass <input.png> [<output.api>] [-raw]");
				Console.WriteLine("Specify -raw to skip compression.");
				return;
			}

			var inFile = args[0];
			var outFile = Path.ChangeExtension(inFile, ".api");
			var compress = true;
			if (args.Length > 1)
			{
				if (args[1] == "-raw")
					compress = false;
				else
				{
					outFile = args[1];
					if (args.Length > 2 && args[2] == "-raw")
						compress = false;
				}
			}

			if (inFile.EndsWith(".api", StringComparison.InvariantCultureIgnoreCase))
			{
				Console.WriteLine("You must've accidentally passed the wrong file. Kawa shares your pain.");
				inFile = Path.ChangeExtension(inFile, ".png");
			}

			if (!File.Exists(inFile))
			{
				Console.WriteLine("Input file {0} does not exist.", inFile);
				return;
			}

			var inBitmap = new Bitmap(inFile);
			if (inBitmap.PixelFormat != PixelFormat.Format8bppIndexed && inBitmap.PixelFormat != PixelFormat.Format4bppIndexed)
			{
				Console.WriteLine("Image is not indexed.");
				return;
			}
			var inPal = inBitmap.Palette.Entries;
			var outPal = new short[inPal.Length];
			for (var i = 0; i < inPal.Length; i++)
			{
				var r = inPal[i].R;
				var g = inPal[i].G;
				var b = inPal[i].B;
				var snes = ((b >> 3) << 10) | ((g >> 3) << 5) | (r >> 3);
				outPal[i] = (short)snes;
			}
			var size = inBitmap.Width * inBitmap.Height;
			if (inBitmap.PixelFormat == PixelFormat.Format4bppIndexed)
				size /= 2;
			var outData = new byte[size];
			var bitmapData = inBitmap.LockBits(new Rectangle(0, 0, inBitmap.Width, inBitmap.Height), ImageLockMode.ReadOnly, inBitmap.PixelFormat);
			var stride = bitmapData.Stride;
			Marshal.Copy(bitmapData.Scan0, outData, 0, size);
			inBitmap.UnlockBits(bitmapData);

			if (compress)
			{
				var compressed = outData.RleCompress();
				if (compressed.Length < outData.Length)
					outData = compressed;
				else
					compress = false;
			}

			using (var f = new BinaryWriter(File.Open(outFile, FileMode.Create)))
			{
				f.Write("AIMG".ToCharArray());
				f.Write((byte)(inBitmap.PixelFormat == PixelFormat.Format8bppIndexed ? 8 : 4));
				f.Write((byte)(compress ? 1 : 0));
				f.WriteMoto((ushort)inBitmap.Width);
				f.WriteMoto((ushort)inBitmap.Height);
				f.WriteMoto((ushort)stride);
				f.WriteMoto((uint)outData.Length); //size);
				f.WriteMoto((uint)0x18);
				f.WriteMoto((uint)(0x18 + (outPal.Length * 2)));
				for (var i = 0; i < outPal.Length; i++)
					f.WriteMoto(outPal[i]);
				f.Write(outData, 0, outData.Length);
			}
		}
	}

	static public class Extensions
	{
		public static byte[] RleCompress(this byte[] data)
		{
			var ret = new List<byte>();
			var i = 0;
			var count = 0;
			Action emit = new Action(() =>
			{
				if (i >= data.Length)
					return;
				if (data[i] == 204)
					Console.WriteLine("!");
				if (data[i] >= 0xC0 || count > 0)
					ret.Add((byte)(0xC0 | (count + 1)));
				ret.Add(data[i]);
				count = 0;
			});
			for (i = 0; i < data.Length - 1; i++)
			{
				if (data[i] == data[i + 1])
				{
					if (count == 62)
						emit();
					else
						count++;
				}
				else
				{
					emit();
				}
			}
			emit();
			ret.Add(0xC0);
			ret.Add(0xC0);
			return ret.ToArray();
		}

		public static void WriteMoto(this BinaryWriter stream, Int16 value)
		{
			var moto2 = BitConverter.GetBytes(value);
			Array.Reverse(moto2);
			stream.Write(moto2);
		}

		public static void WriteMoto(this BinaryWriter stream, UInt16 value)
		{
			var moto2 = BitConverter.GetBytes(value);
			Array.Reverse(moto2);
			stream.Write(moto2);
		}

		public static void WriteMoto(this BinaryWriter stream, UInt32 value)
		{
			var moto4 = BitConverter.GetBytes(value);
			Array.Reverse(moto4);
			stream.Write(moto4);
		}
	}
}
