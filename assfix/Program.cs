using System;
using System.IO;
using System.Text;

namespace assfix
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Use: assfix <input.ap3>");
				return;
			}
			var inFile = args[0];
			UInt32 sum = 0;
			var bytes = File.ReadAllBytes(inFile);
			for (var i = 0; i < bytes.Length; i++)
			{
				if (i == 0x20)
					i += 4;
				sum += bytes[i];
			}
			Console.WriteLine("Checksum is {0:X08}.", sum);
			using (var file = new BinaryWriter(File.Open(inFile, FileMode.Open)))
			{
				file.Seek(0x20, SeekOrigin.Begin);
				file.WriteMoto(sum);

				var fileSize = (UInt32)file.BaseStream.Length;
				var romSize = fileSize.RoundUp();
				if (fileSize != romSize)
				{
					Console.WriteLine("Padding up to {0:X08}...", romSize);
					file.Seek((int)romSize - 1, SeekOrigin.Begin);
					file.Write((byte)0);
				}
			}
		}
	}

	static public class Extensions
	{
		public static UInt32 RoundUp(this UInt32 v)
		{
			v--;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v++;
			return v;
		}


		public static void WriteMoto(this BinaryWriter stream, UInt32 value)
		{
			var moto4 = BitConverter.GetBytes(value);
			Array.Reverse(moto4);
			stream.Write(moto4);
		}
	}
}
