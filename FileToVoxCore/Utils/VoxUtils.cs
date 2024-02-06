using FileToVoxCore.Schematics.Tools;
using FileToVoxCore.Vox;
using System;

namespace FileToVoxCore.Utils
{
	public static class VoxUtils
	{
		private static Vector3Int GetVoxPosition(VoxelData data, int x, int y, int z, Vector3 pivot, Vector3 fpivot, Matrix4x4 matrix4X4)
		{
			Vector3Int tmpVoxel = new Vector3Int(x, y, z);
			//origPos.x = data.VoxelsWide - 1 - tmpVoxel.x; //invert
			//origPos.y = data.VoxelsDeep - 1 - tmpVoxel.z; //swapYZ //invert
			//origPos.z = tmpVoxel.y;

			Vector3 pos = new Vector3(tmpVoxel.X + 0.5f, tmpVoxel.Y + 0.5f, tmpVoxel.Z + 0.5f);
			pos -= pivot;
			pos = matrix4X4.MultiplyPoint(pos);
			pos += pivot;

			pos.X += fpivot.X;
			pos.Y += fpivot.Y;
			pos.Z -= fpivot.Z;

			tmpVoxel.X = (int)Math.Floor(pos.X);
			tmpVoxel.Y = (int)Math.Floor(pos.Y);
			tmpVoxel.Z = (int)Math.Floor(pos.Z);

			//tmpVoxel.x = data.VoxelsWide - 1 - origPos.x; //invert
			//tmpVoxel.z = data.VoxelsDeep - 1 - origPos.y; //swapYZ  //invert
			//tmpVoxel.y = origPos.z;

			return tmpVoxel;
		}

		public static Matrix4x4 ReadMatrix4X4FromRotation(Rotation rotation, Vector3 transform)
		{
			Matrix4x4 result = Matrix4x4.identity;
			{
				byte r = Convert.ToByte(rotation);
				int indexRow0 = (r & 3);
				int indexRow1 = (r & 12) >> 2;
				bool signRow0 = (r & 16) == 0;
				bool signRow1 = (r & 32) == 0;
				bool signRow2 = (r & 64) == 0;

				result.SetRow(0, Vector4.zero);
				switch (indexRow0)
				{
					case 0: result[0, 0] = signRow0 ? 1f : -1f; break;
					case 1: result[0, 1] = signRow0 ? 1f : -1f; break;
					case 2: result[0, 2] = signRow0 ? 1f : -1f; break;
				}
				result.SetRow(1, Vector4.zero);
				switch (indexRow1)
				{
					case 0: result[1, 0] = signRow1 ? 1f : -1f; break;
					case 1: result[1, 1] = signRow1 ? 1f : -1f; break;
					case 2: result[1, 2] = signRow1 ? 1f : -1f; break;
				}
				result.SetRow(2, Vector4.zero);
				switch (indexRow0 + indexRow1)
				{
					case 1: result[2, 2] = signRow2 ? 1f : -1f; break;
					case 2: result[2, 1] = signRow2 ? 1f : -1f; break;
					case 3: result[2, 0] = signRow2 ? 1f : -1f; break;
				}

				result.SetColumn(3, new Vector4(transform.X, transform.Y, transform.Z, 1f));
			}
			return result;
		}
	}
}
