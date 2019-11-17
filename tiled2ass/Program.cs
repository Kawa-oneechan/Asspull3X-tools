using System;
using System.Collections.Generic;
using System.IO;
using Kawa.Json;

namespace tiled2ass
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Use: tiled2ass <input.json> [<output.s>]");
				return;
			}
			var inFile = args[0];
			var outFile = Path.ChangeExtension(inFile, ".s");
			if (args.Length > 1)
				outFile = args[1];

			if (!File.Exists(inFile))
			{
				Console.WriteLine("Input file {0} does not exist.", inFile);
				return;
			}

			var data = new Dictionary<string, List<short>>();
			var tmx = Json5.Parse(File.ReadAllText(inFile)) as JsonObj;

			var palettes = new byte[2048]; //or so~
			foreach (var tileset in tmx.Path<List<JsonObj>>("/tilesets"))
			{
				if (!tileset.ContainsKey("tiles"))
					continue;
				foreach (var tile in tileset.Path<List<JsonObj>>("/tiles"))
				{
					var id = tile.Path<int>("/id");
					if (tile.ContainsKey("properties"))
					{
						foreach (var property in tile.Path<JsonObj[]>("/properties"))
						{
							if (property.Path<string>("/name") == "palette")
								palettes[id] = (byte)property.Path<int>("/value");
						}
					}
				}
			}

			foreach (var layer in tmx.Path<List<JsonObj>>("/layers"))
			{
				if (layer.Path<string>("/type") != "tilelayer")
					continue;
				if (!layer.Path<bool>("/visible"))
					continue;
				var mapName = layer.Path<string>("/name").Replace(" ", "");
				var mapData = layer.Path<long[]>("/data");
				var tileOffset = 0;
				if (layer.ContainsKey("properties"))
				{
					foreach (var property in layer.Path<JsonObj[]>("/properties"))
					{
						if (property.Path<string>("/name") == "tileOffset")
							tileOffset = property.Path<int>("/value");
					}
				}
				var newData = new List<short>();
				foreach (var value in mapData)
				{
					var v = (value > 0) ? (short)((value - 1) & 0x1FF) : 0;
					var o = v;
					v += tileOffset;
					if ((value & 0x80000000) == 0x80000000) //hflip
						v |= 0x0400;
					if ((value & 0x40000000) == 0x40000000) //vflip
						v |= 0x0800;
					v |= (palettes[o] << 12);
					newData.Add((short)v);
				}
				data.Add(mapName, newData);
			}

			var output = new StreamWriter(outFile);
			output.WriteLine("\t.section .rodata");
			output.WriteLine("\t.align\t2");
			foreach (var layer in data)
				output.WriteLine("\t.global {0}", layer.Key);
			foreach (var layer in data)
			{
				output.Write("{0}:", layer.Key);
				var num = 0;
				foreach (var value in layer.Value)
				{
					if (num % 8 == 0)
						output.Write("\n\t.short ");
					else
						output.Write(",");
					output.Write("0x{0:X04}", value);
					num++;
				}
				output.WriteLine();
				output.Flush();
			}
		}
	}
}
