﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileToVoxCore.Drawing;
using FileToVoxCore.Vox.Chunks;

namespace FileToVoxCore.Vox
{
	public class VoxReader : VoxParser
	{
		protected int VoxelCountLastXyziChunk;
		protected string LogOutputFile;
		protected bool WriteLog;
		protected bool OffsetPalette;

		public virtual VoxModel LoadModel(string absolutePath, bool writeLog = false, bool debug = false, bool offsetPalette = true)
		{
			VoxModel output = new VoxModel();
			var name = Path.GetFileNameWithoutExtension(absolutePath);
			VoxelCountLastXyziChunk = 0;
			LogOutputFile = name + "-" + DateTime.Now.ToString("y-MM-d_HH.m.s") + ".txt";
			WriteLog = writeLog;
			OffsetPalette = offsetPalette;
			ChildCount = 0;
			ChunkCount = 0;
			using (var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(absolutePath))))
			{
				var head = new string(reader.ReadChars(4));
				if (!head.Equals(HEADER))
				{
					Console.WriteLine("Not a Magicavoxel file! " + output);
					return null;
				}
				int version = reader.ReadInt32();
				if (version != VERSION)
				{
					Console.WriteLine("Version number: " + version + " Was designed for version: " + VERSION);
				}
				ResetModel(output);
				while (reader.BaseStream.Position != reader.BaseStream.Length)
					ReadChunk(reader, output);
			}

			if (debug)
			{
				CheckDuplicateIds(output);
				CheckDuplicateChildGroupIds(output);
				CheckTransformIdNotInGroup(output);
				Console.ReadKey();
			}


			if (output.Palette == null)
				output.Palette = LoadDefaultPalette();
			return output;
		}

		protected void CheckDuplicateIds(VoxModel output)
		{
			List<int> allIds = output.GroupNodeChunks.Select(t => t.Id).ToList();
			allIds.AddRange(output.TransformNodeChunks.Select(t => t.Id));
			allIds.AddRange(output.ShapeNodeChunks.Select(t => t.Id));

			List<int> duplicates = allIds.GroupBy(x => x)
				.Where(g => g.Count() > 1)
				.Select(y => y.Key)
				.ToList();

			foreach (int Id in duplicates)
			{
				Console.WriteLine("[ERROR] Duplicate Id: " + Id);
			}
		}

		protected void CheckDuplicateChildGroupIds(VoxModel output)
		{
			List<int> ChildIds = output.GroupNodeChunks.SelectMany(t => t.ChildIds).ToList();
			List<int> duplicates = ChildIds.GroupBy(x => x)
				.Where(g => g.Count() > 1)
				.Select(y => y.Key)
				.ToList();

			foreach (int Id in duplicates)
			{
				Console.WriteLine("[ERROR] Duplicate child group Id: " + Id);
			}
		}

		protected void CheckTransformIdNotInGroup(VoxModel output)
		{
			List<int> Ids = output.TransformNodeChunks.Select(t => t.Id).ToList();
			List<int> ChildIds = output.GroupNodeChunks.SelectMany(t => t.ChildIds).ToList();

			List<int> empty = new List<int>();
			foreach (int Id in Ids)
			{
				if (ChildIds.IndexOf(Id) == -1 && Id != 0)
				{
					empty.Add(Id);
				}
			}

			foreach (int Id in empty)
			{
				Console.WriteLine("[ERROR] Transform Id never called in any group: " + Id);
			}
		}

		protected Color[] LoadDefaultPalette()
		{
			var colorCount = default_palette.Length;
			var result = new Color[256];
			byte r, g, b, a;
			for (int i = 0; i < colorCount; i++)
			{
				var source = default_palette[i];
				r = (byte)(source & 0xff);
				g = (byte)((source >> 8) & 0xff);
				b = (byte)((source >> 16) & 0xff);
				a = (byte)((source >> 26) & 0xff);
				result[i] = Color.FromArgb(a, r, g, b);
			}
			return result;
		}

		/// <summary>
		/// Load the Palette color. Plattes are offset by 1
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		protected Color[] LoadPalette(BinaryReader reader)
		{
			var result = new Color[256];
			int iStart = OffsetPalette ? 0 : 1;
			int iMax = OffsetPalette ? 255 : 256;
			for (int i = iStart; i < iMax; i++)
			{
				byte r = reader.ReadByte();
				byte g = reader.ReadByte();
				byte b = reader.ReadByte();
				byte a = reader.ReadByte();
				result[i] = Color.FromArgb(a, r, g, b);
			}
			return result;
		}

		protected void ReadChunk(BinaryReader reader, VoxModel output)
		{
			var chunkName = new string(reader.ReadChars(4));
			var chunkSize = reader.ReadInt32();
			var childChunkSize = reader.ReadInt32();
			var chunk = reader.ReadBytes(chunkSize);
			var children = reader.ReadBytes(childChunkSize);
			ChunkCount++;

			using (var chunkReader = new BinaryReader(new MemoryStream(chunk)))
			{
				switch (chunkName)
				{
					case MAIN:
						break;
					case SIZE:
						ReadSIZENodeChunk(chunkReader, output);
						break;
					case XYZI:
						ReadXYZINodeChunk(chunkReader, output);
						break;
					case RGBA:
						output.Palette = LoadPalette(chunkReader);
						break;
					case MATT:
						break;
					case PACK:
						int frameCount = chunkReader.ReadInt32();
						for (int i = 0; i < frameCount; i++)
						{
							output.VoxelFrames.Add(new VoxelData());
						}
						break;
					case nTRN:
						output.TransformNodeChunks.Add(ReadTransformNodeChunk(chunkReader));
						break;
					case nGRP:
						output.GroupNodeChunks.Add(ReadGroupNodeChunk(chunkReader));
						break;
					case nSHP:
						output.ShapeNodeChunks.Add(ReadShapeNodeChunk(chunkReader));
						break;
					case LAYR:
						output.LayerChunks.Add(ReadLayerChunk(chunkReader));
						break;
					case MATL:
						output.MaterialChunks.Add(ReadMaterialChunk(chunkReader));
						break;
					case rOBJ:
						output.RendererSettingChunks.Add(ReaddRObjectChunk(chunkReader));
						break;
					case IMAP:
						output.PaletteColorIndex = new int[256];
						for (int i = 0; i < 256; i++)
						{
							int index = chunkReader.ReadByte();
							output.PaletteColorIndex[i] = index;
						}
						break;
					default:
						Console.WriteLine($"Unknown chunk: \"{chunkName}\"");
						break;
				}
			}

			if (WriteLog)
			{
				WriteLogs(chunkName, chunkSize, childChunkSize, output);
			}

			//read child chunks
			using (var childReader = new BinaryReader(new MemoryStream(children)))
			{
				while (childReader.BaseStream.Position != childReader.BaseStream.Length)
				{
					ReadChunk(childReader, output);
				}
			}

		}

		protected void WriteLogs(string chunkName, int chunkSize, int childChunkSize, VoxModel output)
		{
			if (!Directory.Exists("logs"))
				Directory.CreateDirectory("logs");

			string path = "logs/" + LogOutputFile;
			using (var writer = new StreamWriter(path, true))
			{
				writer.WriteLine("CHUNK NAME: " + chunkName + " (" + ChunkCount + ")");
				writer.WriteLine("CHUNK SIZE: " + chunkSize + " BYTES");
				writer.WriteLine("CHILD CHUNK SIZE: " + childChunkSize);
				switch (chunkName)
				{
					case SIZE:
						var frame = output.VoxelFrames[ChildCount - 1];
						writer.WriteLine("-> SIZE: " + frame.VoxelsWide + " " + frame.VoxelsTall + " " + frame.VoxelsDeep);
						break;
					case XYZI:
						writer.WriteLine("-> XYZI: " + VoxelCountLastXyziChunk);
						break;
					case nTRN:
						var transform = output.TransformNodeChunks.Last();
						writer.WriteLine("-> TRANSFORM NODE: " + transform.Id);
						writer.WriteLine("--> CHILD Id: " + transform.ChildId);
						writer.WriteLine("--> RESERVED Id: " + transform.ReservedId);
						writer.WriteLine("--> LAYER Id: " + transform.LayerId);
						DisplayAttributes(transform.Attributes, writer);
						DisplayFrameAttributes(transform.FrameAttributes, writer);
						break;
					case nGRP:
						var group = output.GroupNodeChunks.Last();
						writer.WriteLine("-> GROUP NODE: " + group.Id);
						group.ChildIds.ToList().ForEach(t => writer.WriteLine("--> CHILD Id: " + t));
						DisplayAttributes(group.Attributes, writer);
						break;
					case nSHP:
						var shape = output.ShapeNodeChunks.Last();
						writer.WriteLine("-> SHAPE NODE: " + shape.Id);
						DisplayAttributes(shape.Attributes, writer);
						DisplayModelAttributes(shape.Models, writer);
						break;
					case LAYR:
						var layer = output.LayerChunks.Last();
						writer.WriteLine("-> LAYER NODE: " + layer.Id + " " +
							layer.Name + " " +
							layer.Hidden + " " +
							layer.Unknown);
						DisplayAttributes(layer.Attributes, writer);
						break;
					case MATL:
						var material = output.MaterialChunks.Last();
						writer.WriteLine("-> MATERIAL NODE: " + material.Id.ToString("F1"));
						writer.WriteLine("--> ALPHA: " + material.Alpha.ToString("F1"));
						writer.WriteLine("--> EMISSION: " + material.Emission.ToString("F1"));
						writer.WriteLine("--> FLUX: " + material.Flux.ToString("F1"));
						writer.WriteLine("--> METALLIC: " + material.Metal.ToString("F1"));
						writer.WriteLine("--> ROUGH: " + material.Rough.ToString("F1"));
						writer.WriteLine("--> SMOOTHNESS: " + material.Smoothness.ToString("F1"));
						writer.WriteLine("--> SPEC: " + material.Specular.ToString("F1"));
						DisplayAttributes(material.Properties, writer);
						break;
					case IMAP:
						if (output.PaletteColorIndex != null)
						{
							foreach (int colorIndex in output.PaletteColorIndex)
							{
								writer.WriteLine("--> " + colorIndex);
							}
						}

						break;
				}
				writer.WriteLine("");
				writer.Close();
			}
		}

		protected void DisplayAttributes(Dictionary<string, string> attributes, StreamWriter writer)
		{
			attributes.ToList().ForEach(t => writer.WriteLine("--> ATTRIBUTE: Key=" + t.Key + " Value=" + t.Value));
		}

		protected void DisplayFrameAttributes(DICT[] FrameAttributes, StreamWriter writer)
		{
			var list = FrameAttributes.ToList();
			foreach (var item in list)
			{
				writer.WriteLine("--> FRAME ATTRIBUTE: " + item._r + " " + item._t.ToString());
			}
		}

		protected void DisplayModelAttributes(ShapeModel[] Models, StreamWriter writer)
		{
			Models.ToList().ForEach(t => writer.WriteLine("--> MODEL ATTRIBUTE: " + t.ModelId));
			Models.ToList().ForEach(t => DisplayAttributes(t.Attributes, writer));
		}

		protected static string ReadSTRING(BinaryReader reader)
		{
			var size = reader.ReadInt32();
			var bytes = reader.ReadBytes(size);
			string text = Encoding.UTF8.GetString(bytes);
			return text;
		}

		protected delegate T ItemReader<T>(BinaryReader reader);

		protected static T[] ReadArray<T>(BinaryReader reader, ItemReader<T> itemReader)
		{
			int size = reader.ReadInt32();
			return Enumerable.Range(0, size)
				.Select(i => itemReader(reader)).ToArray();
		}

		protected static Dictionary<string, string> ReadDICT(BinaryReader reader)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			int size = reader.ReadInt32();
			for (int i = 0; i < size; i++)
			{
				string key = ReadSTRING(reader);
				string value = ReadSTRING(reader);
				result.Add(key, value);
			}

			return result;
		}

		protected RendererSettingChunk ReaddRObjectChunk(BinaryReader chunkReader)
		{
			return new RendererSettingChunk
			{
				Attributes = ReadDICT(chunkReader)
			};
		}

		protected MaterialChunk ReadMaterialChunk(BinaryReader chunkReader)
		{
			return new MaterialChunk
			{
				Id = chunkReader.ReadInt32(),
				Properties = ReadDICT(chunkReader)
			};
		}

		protected LayerChunk ReadLayerChunk(BinaryReader chunkReader)
		{
			return new LayerChunk
			{
				Id = chunkReader.ReadInt32(),
				Attributes = ReadDICT(chunkReader),
				Unknown = chunkReader.ReadInt32()
			};
		}

		protected ShapeNodeChunk ReadShapeNodeChunk(BinaryReader chunkReader)
		{
			return new ShapeNodeChunk
			{
				Id = chunkReader.ReadInt32(),
				Attributes = ReadDICT(chunkReader),
				Models = ReadArray(chunkReader, r => new ShapeModel
				{
					ModelId = r.ReadInt32(),
					Attributes = ReadDICT(r)
				})
			};
		}

		protected GroupNodeChunk ReadGroupNodeChunk(BinaryReader chunkReader)
		{
			var groupNodeChunk = new GroupNodeChunk
			{
				Id = chunkReader.ReadInt32(),
				Attributes = ReadDICT(chunkReader),
				ChildIds = ReadArray(chunkReader, r => r.ReadInt32())
			};
			return groupNodeChunk;
		}

		protected TransformNodeChunk ReadTransformNodeChunk(BinaryReader chunkReader)
		{
			var transformNodeChunk = new TransformNodeChunk()
			{
				Id = chunkReader.ReadInt32(),
				Attributes = ReadDICT(chunkReader),
				ChildId = chunkReader.ReadInt32(),
				ReservedId = chunkReader.ReadInt32(),
				LayerId = chunkReader.ReadInt32(),
				FrameAttributes = ReadArray(chunkReader, r => new DICT(ReadDICT(r)))
			};
			return transformNodeChunk;
		}

		protected virtual void ReadXYZINodeChunk(BinaryReader chunkReader, VoxModel output)
		{
			VoxelCountLastXyziChunk = chunkReader.ReadInt32();
			VoxelData frame = output.VoxelFrames[ChildCount - 1];
			for (int i = 0; i < VoxelCountLastXyziChunk; i++)
			{
				byte x = chunkReader.ReadByte();
				byte y = chunkReader.ReadByte();
				byte z = chunkReader.ReadByte();
				byte color = chunkReader.ReadByte();
				if (color > 0)
				{
					frame.Set(x, y, z, color);
				}
				output.ColorUsed.Add(color);
			}
		}

		protected virtual void ReadSIZENodeChunk(BinaryReader chunkReader, VoxModel output)
		{
			int xSize = chunkReader.ReadInt32();
			int ySize = chunkReader.ReadInt32();
			int zSize = chunkReader.ReadInt32();
			if (ChildCount >= output.VoxelFrames.Count)
				output.VoxelFrames.Add(new VoxelData());
			output.VoxelFrames[ChildCount].Resize(xSize, ySize, zSize);
			ChildCount++;
		}
	}
}
