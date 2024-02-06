﻿using System.Collections.Generic;
using System.Linq;
using FileToVoxCore.Schematics.Tools;

namespace FileToVoxCore.Vox
{
    public class VoxelData
    {
        private int mVoxelWide, mVoxelsTall, mVoxelsDeep;
        //private byte[] mColors;
        private Dictionary<int, byte> mColors;

        /// <summary>
        /// Creates a voxeldata with provided dimensions
        /// </summary>
        /// <param name="voxelWide"></param>
        /// <param name="voxelsTall"></param>
        /// <param name="voxelsDeep"></param>
        public VoxelData(int voxelWide, int voxelsTall, int voxelsDeep)
        {
            Resize(voxelWide, voxelsTall, voxelsDeep);
        }

        public VoxelData()
        {
        }

        public void Resize(int voxelsWide, int voxelsTall, int voxelsDeep)
        {
            mVoxelWide = voxelsWide + 1;
            mVoxelsTall = voxelsTall + 1;
            mVoxelsDeep = voxelsDeep + 1;
            mColors = new Dictionary<int, byte>();
            //mColors = new byte[mVoxelWide * mVoxelsTall * mVoxelsDeep];
        }

        /// <summary>
        /// Gets a grid position from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int GetGridPos(int x, int y, int z)
            => (mVoxelWide * mVoxelsTall) * z + (mVoxelWide * y) + x;

        public void Get3DPos(int idx, out int x, out int y, out int z)
        {
	        z = idx / (mVoxelWide * mVoxelsTall);
	        idx -= (z * mVoxelWide * mVoxelsTall);
	        y = idx / mVoxelWide;
	        x = idx % mVoxelWide;
        }

        /// <summary>
        /// Set a color index from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Set(int x, int y, int z, byte value)
            => mColors[GetGridPos(x, y, z)] = value;

        /// <summary>
        /// Sets a colors index from grid position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Set(int x, byte value)
            => mColors[x] = value;

        /// <summary>
        /// Gets a palette index from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int Get(int x, int y, int z)
            => mColors[GetGridPos(x, y, z)];

        /// <summary>
        /// Gets a palette index from a grid position
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public byte Get(int x) => mColors[x];

        /// <summary>
        /// Width of the data in voxels
        /// </summary>
        public int VoxelsWide => mVoxelWide;

        /// <summary>
        /// Height of the data in voxels
        /// </summary>
        public int VoxelsTall => mVoxelsTall;

        /// <summary>
        /// Depth of the data in voxels
        /// </summary>
        public int VoxelsDeep => mVoxelsDeep;

        public Dictionary<int, byte> Colors => mColors;

        public bool ContainsKey(int x, int y, int z)
            => mColors.ContainsKey(GetGridPos(x, y, z));

        public byte GetSafe(int x, int y, int z)
            => ContainsKey(x, y, z) ? mColors[GetGridPos(x, y, z)] : (byte)0;

        public Vector3 GetVolumeSize()
	        => new Vector3(VoxelsWide, VoxelsTall, VoxelsDeep);

        public VoxelData ToSmaller()
        {
            var work = new byte[8];
            var result = new VoxelData(
                (mVoxelWide + 1) >> 1,
                (mVoxelsTall + 1) >> 1,
                (mVoxelsDeep + 1) >> 1);
            int i = 0;
            for (int z = 0; z < mVoxelsDeep; z += 2)
            {
                int z1 = z + 1;
                for (int y = 0; y < mVoxelsTall; y += 2)
                {
                    int y1 = y + 1;
                    for (int x = 0; x < mVoxelWide; x += 2)
                    {
                        int x1 = x + 1;
                        work[0] = GetSafe(x, y, z);
                        work[1] = GetSafe(x1, y, z);
                        work[2] = GetSafe(x, y1, z);
                        work[3] = GetSafe(x1, y1, z);
                        work[4] = GetSafe(x, y, z1);
                        work[5] = GetSafe(x1, y1, z1);
                        work[6] = GetSafe(x, y1, z1);
                        work[7] = GetSafe(x1, y1, z1);

                        if (work.Any(color => color != 0))
                        {
                            var groups = work.Where(color => color != 0).GroupBy(v => v).OrderByDescending(v => v.Count());
                            var count = groups.ElementAt(0).Count();
                            var group = groups.TakeWhile(v => v.Count() == count)
                                .OrderByDescending(v => v.Key).First();
                            result.mColors[i++] = group.Key;
                        }
                        else
                        {
                            result.mColors[i++] = 0;
                        }
                    }
                }
            }
            return result;
        }
    }
}
