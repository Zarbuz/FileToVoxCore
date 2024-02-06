﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FileToVoxCore.Drawing;
using FileToVoxCore.Extensions;
using FileToVoxCore.Schematics.Tools;
using FileToVoxCore.Utils;

namespace FileToVoxCore.Schematics
{
	public class Region
	{
		public int X;
		public int Y;
		public int Z;
		public Dictionary<long, Voxel> BlockDict { get; private set; }

		public Region(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
			BlockDict = new Dictionary<long, Voxel>();
		}

		public override string ToString()
		{
			return $"{X} {Y} {Z}";
		}

		public uint GetColorAtVoxelIndex(int x, int y, int z)
		{
			return GetVoxel(x, y, z, out Voxel voxel) ? voxel.Color : 0;
		}

		public bool GetVoxel(int x, int y, int z, out Voxel voxel)
		{
			long voxelIndex = Schematic.GetVoxelIndex(x, y, z);

			bool found = BlockDict.TryGetValue(voxelIndex, out Voxel foundVoxel);
			voxel = foundVoxel;
			return found;
		}
	}

	public enum RotationMode
	{
		X,
		Y,
		Z,
	}

	public class Schematic
	{
		#region ConstStatic

		public const int MAX_WORLD_WIDTH = 2000;
		public const int MAX_WORLD_HEIGHT = 1000;
		public const int MAX_WORLD_LENGTH = 2000;
		public const int MAX_COLORS_IN_PALETTE = 256;
		public static int CHUNK_SIZE = 128;
		public static bool DEBUG;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long GetVoxelIndex(int x, int y, int z)
		{
			return (y * MAX_WORLD_LENGTH + z) * MAX_WORLD_WIDTH + x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetVoxelIndex2D(int x, int z)
		{
			return x + MAX_WORLD_WIDTH * z;
		}

		public static int GetVoxelIndex2DFromRotation(int x, int y, int z, RotationMode rotationMode)
		{
			switch (rotationMode)
			{
				case RotationMode.X:
					return GetVoxelIndex2D(z, y);
				case RotationMode.Y:
					return GetVoxelIndex2D(x, z);
				case RotationMode.Z:
					return GetVoxelIndex2D(x, y);
			}
			return GetVoxelIndex2D(x, z);
		}

		#endregion

		#region Fields

		private ushort mWidth;

		public ushort Width
		{
			get
			{
				if (UsedColors.Count == 0)
				{
					mWidth = 0;
					return mWidth;
				}

				mWidth = (ushort)(MaxX - MinX + 1);
				return mWidth;
			}
		}

		private ushort mHeight;

		public ushort Height
		{
			get
			{
				if (UsedColors.Count == 0)
				{
					mHeight = 0;
					return mHeight;
				}

				mHeight = (ushort)(MaxY - MinY + 1);

				return mHeight;
			}
		}

		private ushort mLength;

		public ushort Length
		{
			get
			{
				if (UsedColors.Count == 0)
				{
					mLength = 0;
					return mLength;
				}

				mLength = (ushort)(MaxZ - MinZ + 1);

				return mLength;
			}
		}

		public Dictionary<long, Region> RegionDict { get; private set; }
		public List<uint> UsedColors { get; private set; }

		public int MinX { get; private set; }
		public int MinY { get; private set; }
		public int MinZ { get; private set; }

		public int MaxX { get; private set; }
		public int MaxY { get; private set; }
		public int MaxZ { get; private set; }
		private readonly bool mCanReorderPalette = true;

		public Schematic()
		{
			UsedColors = new List<uint>();
			CreateAllRegions();
		}

		public Schematic(List<uint> palette) : this()
		{
			UsedColors = palette;
			mCanReorderPalette = false;
		}

		public Schematic(List<Voxel> voxels) : this()
		{
			AddVoxels(voxels);
		}

		#endregion

		#region PublicMethods

		public void AddVoxels(IEnumerable<Voxel> voxels)
		{
			foreach (Voxel voxel in voxels)
			{
				AddVoxel(voxel);
			}
		}

		public void AddVoxel(Voxel voxel)
		{
			AddVoxel(voxel.X, voxel.Y, voxel.Z, voxel.Color);
		}

		public void AddVoxel(int x, int y, int z, Color color)
		{
			AddVoxel(x, y, z, color.ColorToUInt());
		}

		public void AddVoxel(int x, int y, int z, uint color)
		{
			AddVoxel(x, y, z, color, true);
		}

		public void AddVoxel(int x, int y, int z, uint color, bool replaceIfExist)
		{
			if (color != 0 && x < MAX_WORLD_WIDTH && y < MAX_WORLD_HEIGHT && z < MAX_WORLD_LENGTH)
			{
				if (!replaceIfExist && ContainsVoxel(x, y, z))
				{
					return;
				}
				AddColorInUsedColors(color);
				AddUsageForRegion(x, y, z, color);
				ComputeMinMax(x, y, z);
			}
		}

		public void ReplaceVoxel(Voxel voxel, uint color)
		{
			AddColorInUsedColors(color);
			ReplaceVoxel(voxel.X, voxel.Y, voxel.Z, color);
		}

		public void ReplaceVoxel(int x, int y, int z, uint color)
		{
			if (color != 0)
			{
				AddColorInUsedColors(color);
				ReplaceUsageForRegion(x, y, z, color);
			}
		}

		public void RemoveVoxel(Voxel voxel)
		{
			RemoveUsageForRegion(voxel.X, voxel.Y, voxel.Z);
		}

		public void RemoveVoxel(int x, int y, int z)
		{
			RemoveUsageForRegion(x, y, z);
		}

		public uint GetColorAtVoxelIndex(Vector3Int pos)
		{
			return GetVoxel(pos.X, pos.Y, pos.Z, out Voxel voxel) ? voxel.Color : 0;
		}

		public uint GetColorAtVoxelIndex(int x, int y, int z)
		{
			return GetVoxel(x, y, z, out Voxel voxel) ? voxel.Color : 0;
		}

		public bool ContainsVoxel(int x, int y, int z)
		{
			return GetVoxel(x, y, z, out _);
		}

		public bool GetVoxel(int x, int y, int z, out Voxel voxel)
		{
			FastMath.FloorToInt(x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);

			long chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			long voxelIndex = GetVoxelIndex(x, y, z);
			if (RegionDict.ContainsKey(chunkIndex))
			{
				bool found = RegionDict[chunkIndex].BlockDict.TryGetValue(voxelIndex, out Voxel foundVoxel);
				voxel = foundVoxel;
				return found;
			}
			else
			{
				voxel = null;
				return false;
			}
		}

		public List<Region> GetAllRegions()
		{
			return RegionDict.Values.Where(region => region.BlockDict.Count > 0).ToList();
		}

		public uint GetColorAtPaletteIndex(int index)
		{
			return index < UsedColors.Count ? UsedColors[index] : 0;
		}

		public List<Voxel> GetAllVoxels()
		{
			List<Voxel> voxels = new List<Voxel>();
			foreach (KeyValuePair<long, Region> region in RegionDict)
			{
				voxels.AddRange(region.Value.BlockDict.Values);
			}

			return voxels;
		}
		public int GetPaletteIndex(uint color)
		{
			return UsedColors.IndexOf(color);
		}

		#endregion

		#region PrivateMethods

		private void CreateAllRegions()
		{
			RegionDict = new Dictionary<long, Region>();

			int worldRegionX = (int)Math.Ceiling((decimal)MAX_WORLD_WIDTH / CHUNK_SIZE);
			int worldRegionY = (int)Math.Ceiling((decimal)MAX_WORLD_HEIGHT / CHUNK_SIZE);
			int worldRegionZ =(int)Math.Ceiling((decimal)MAX_WORLD_LENGTH / CHUNK_SIZE);

			int countSize = worldRegionX * worldRegionY * worldRegionZ;

			for (int i = 0; i < countSize; i++)
			{
				int x = i % worldRegionX;
				int y = (i / worldRegionX) % worldRegionY;
				int z = i / (worldRegionX * worldRegionY);

				RegionDict[GetVoxelIndex(x, y, z)] = new Region(x * CHUNK_SIZE, y * CHUNK_SIZE, z * CHUNK_SIZE);
			}
		}

		private void AddUsageForRegion(int x, int y, int z, uint color)
		{
			FastMath.FloorToInt(x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);

			long chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			long voxelIndex = GetVoxelIndex(x, y, z);

			RegionDict[chunkIndex].BlockDict[voxelIndex] = new Voxel((ushort)x, (ushort)y, (ushort)z, color);
		}

		private void ReplaceUsageForRegion(int x, int y, int z, uint color)
		{
			FastMath.FloorToInt(x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);

			long chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			long voxelIndex = GetVoxelIndex(x, y, z);

			if (RegionDict[chunkIndex].BlockDict.ContainsKey(voxelIndex))
			{
				RegionDict[chunkIndex].BlockDict[voxelIndex].Color = color;
			}
		}

		private void RemoveUsageForRegion(int x, int y, int z)
		{
			int chunkX = x / CHUNK_SIZE;
			int chunkY = y / CHUNK_SIZE;
			int chunkZ = z / CHUNK_SIZE;
			long chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			long voxelIndex = GetVoxelIndex(x, y, z);

			if (RegionDict[chunkIndex].BlockDict.ContainsKey(voxelIndex))
			{
				RegionDict[chunkIndex].BlockDict.Remove(voxelIndex);
			}
		}

		private void AddColorInUsedColors(uint color)
		{
			if (UsedColors.Count < MAX_COLORS_IN_PALETTE && !UsedColors.Contains(color))
			{
				UsedColors.Add(color);
				if (mCanReorderPalette)
				{
					UsedColors = UsedColors.OrderByDescending(c => c).ToList();
				}
			}
		}

		

		private void ComputeMinMax(int x, int y, int z)
		{
			MinX = Math.Min(x, MinX);
			MinY = Math.Min(y, MinY);
			MinZ = Math.Min(z, MinZ);


			MaxX = Math.Max(x, MaxX);
			MaxY = Math.Max(y, MaxY);
			MaxZ = Math.Max(z, MaxZ);
		}
		#endregion
	}
}
