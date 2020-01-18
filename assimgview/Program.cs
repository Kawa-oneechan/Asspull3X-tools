using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace assimgview
{
	static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 0)
				return;

			if (!File.Exists(args[0]))
			{
				Console.WriteLine("Input file {0} does not exist.", args[0]);
				return;
			}

			var file = new BinaryReader(File.OpenRead(args[0]));

			var magic = new string(file.ReadChars(4));
			if (magic != "AIMG")
			{
				MessageBox.Show("Input file is not a valid Asspull IIIx image.");
				return;
			}
			//TODO: more checks?
			var depth = file.ReadByte();
			var compressed = file.ReadByte() == 1;
			var width = file.ReadMotoUInt16();
			var height = file.ReadMotoUInt16();
			var stride = file.ReadMotoUInt16();
			var palSize = (depth == 8) ? 256 : 16;
			var dataSize = file.ReadMotoUInt32();
			var palOffset = file.ReadMotoUInt32();
			var dataOffset = file.ReadMotoUInt32();

			var bitmap = new Bitmap(width, height, (depth == 8) ? PixelFormat.Format8bppIndexed : PixelFormat.Format4bppIndexed);
			var palette = bitmap.Palette;
			file.BaseStream.Seek(palOffset, SeekOrigin.Begin);
			for (var i = 0; i < palSize; i++)
			{
				var snes = file.ReadMotoUInt16();
				var r = (snes >> 0) & 0x1F;
				var g = (snes >> 5) & 0x1F;
				var b = (snes >> 10) & 0x1F;
				palette.Entries[i] = Color.FromArgb((r << 3) + (r >> 2), (g << 3) + (g >> 2), (b << 3) + (b >> 2));
			}
			bitmap.Palette = palette;
			file.BaseStream.Seek(dataOffset, SeekOrigin.Begin);
			var screen = new byte[width * height];
			var pos = 0;
			if (compressed)
			{
				while (file.BaseStream.Position < file.BaseStream.Length)
				{
					var data = file.ReadByte();
					if ((data & 0xC0) == 0xC0)
					{
						var rle = data & 0x3F;
						var original = data;
						var from = file.BaseStream.Position - 1;
						data = file.ReadByte();
						if (data == 0xC0 && rle == 0)
							break;
						for (; rle > 0; rle--)
						{
							screen[pos++] = data;
						}
					}
					else
						screen[pos++] = data;
				}
			}
			else
			{
				//TODO: check this, I only have compressed images right now.
				screen = file.ReadBytes((int)dataSize);
			}
			var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
			Marshal.Copy(screen, 0, bitmapData.Scan0, (width * height) / ((depth == 8) ? 1 : 2));

			var form = new Form()
			{
				ClientSize = new Size(width, height),
				BackgroundImage = bitmap,
				Text = Path.GetFileNameWithoutExtension(args[0])
			};

			Application.Run(form);
		}
	}

	static class Extensions
	{
		public static UInt16 ReadMotoUInt16(this BinaryReader stream)
		{
			var moto2 = stream.ReadBytes(2);
			Array.Reverse(moto2);
			return BitConverter.ToUInt16(moto2, 0);
		}

		public static UInt32 ReadMotoUInt32(this BinaryReader stream)
		{
			var moto4 = stream.ReadBytes(4);
			Array.Reverse(moto4);
			return BitConverter.ToUInt32(moto4, 0);
		}
	}
}
