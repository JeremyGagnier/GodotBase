using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace EvolutionGame
{
	public class HeightMesh
	{
		const byte FLAT = 0;
		const byte UP = 1;
		const byte DOWN = 2;
		private struct TileIndex {
			public byte upType;
			public byte leftType;
			public int originHeightDiff;
		}

		private struct HeightData {
			public int height;
			public int bottomEdge;
			public int bottomEdgeHeightDiff;
			public int rightEdge;
			public int rightEdgeHeightDiff;
		}

		// Square data is used to tell the mesh generator how to render the current tile (points
		// and heightAdjustment) and what the shape of the bottom and right tiles will need to be
		// (bottomEdge and rightEdge).
		private struct SquareData {
			public Vector3[] points;	// The points of the current tile
			public int heightAdjustment;	// The height adjustment for the mesh
			public byte bottomEdge;	// The slope type at the bottom edge
			public byte rightEdge;	// The slope type at the right edge
		}

		private struct TileData {
			public byte bottomType;
			public byte rightType;
			public int nextOriginHeight;
		}

		static readonly Vector3[] flatPoints = {
			new(0, 0, 0),
			new(1, 0, 0),
			new(0, 0, 1),
			new(0, 0, 1),
			new(1, 0, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] bottomLeftBumpPoints = {
			new(0, 0, 0),
			new(1, 0, 1),
			new(0, 1, 1),
			new(0, 0, 0),
			new(1, 0, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] bottomRightBumpPoints = {
			new(0, 0, 0),
			new(1, 0, 0),
			new(0, 0, 1),
			new(0, 0, 1),
			new(1, 0, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] topLeftBumpPoints = {
			new(0, 1, 0),
			new(1, 0, 0),
			new(0, 0, 1),
			new(0, 0, 1),
			new(1, 0, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] topRightBumpPoints = {
			new(0, 0, 0),
			new(1, 0, 1),
			new(0, 0, 1),
			new(0, 0, 0),
			new(1, 1, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] bottomLeftDipPoints = {
			new(0, 1, 0),
			new(1, 1, 1),
			new(0, 0, 1),
			new(0, 1, 0),
			new(1, 1, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] bottomRightDipPoints = {
			new(0, 1, 0),
			new(1, 1, 0),
			new(0, 1, 1),
			new(0, 1, 1),
			new(1, 1, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] topLeftDipPoints = {
			new(0, 0, 0),
			new(1, 1, 0),
			new(0, 1, 1),
			new(0, 1, 1),
			new(1, 1, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] topRightDipPoints = {
			new(0, 1, 0),
			new(1, 1, 1),
			new(0, 1, 1),
			new(0, 1, 0),
			new(1, 0, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] topSlopePoints = {
			new(0, 1, 0),
			new(1, 0, 1),
			new(0, 0, 1),
			new(0, 1, 0),
			new(1, 1, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] rightSlopePoints = {
			new(0, 0, 0),
			new(1, 1, 1),
			new(0, 0, 1),
			new(0, 0, 0),
			new(1, 1, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] bottomSlopePoints = {
			new(0, 0, 0),
			new(1, 0, 0),
			new(0, 1, 1),
			new(0, 1, 1),
			new(1, 0, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] leftSlopePoints = {
			new(0, 1, 0),
			new(1, 0, 0),
			new(0, 1, 1),
			new(0, 1, 1),
			new(1, 0, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] skirtPoints = {
			new(0, 0, 0),
			new(0, -1, 0),
			new(1, 0, 0),
			new(0, -1, 0),
			new(1, -1, 0),
			new(1, 0, 0),
			new(1, 0, 0),
			new(1, 0, 1),
			new(1, -1, 0),
			new(1, -1, 0),
			new(1, 0, 1),
			new(1, -1, 1),
			new(0, 0, 0),
			new(0, 0, 1),
			new(0, -1, 0),
			new(0, -1, 0),
			new(0, 0, 1),
			new(0, -1, 1),
			new(0, 0, 1),
			new(1, 0, 1),
			new(0, -1, 1),
			new(0, -1, 1),
			new(1, 0, 1),
			new(1, -1, 1),
		};

		// Line shapes (starting from origin):
		// 0 - flat
		// 1 - line going up
		// -1 - line going down
		// The first key is (x line from tile above, z line from tile to the left),
		// the second key is the height difference. The value is which points to use. If there is no
		// suitable value then there needs to be a level break.
		static readonly Dictionary<TileIndex, SquareData> tileIndexToTileData = new() {
			{new() { // Done
				upType = FLAT,
				leftType = FLAT,
				originHeightDiff = 1,
			}, new() {
				points = bottomRightDipPoints,
				heightAdjustment = 0,
				bottomEdge = DOWN,
				rightEdge = DOWN,
			}},
			{new() { // Done
				upType = FLAT,
				leftType = FLAT,
				originHeightDiff = 0,
			}, new() {
				points = flatPoints,
				heightAdjustment = 0,
				bottomEdge = FLAT,
				rightEdge = FLAT,
			}},
			{new() { // Done
				upType = FLAT,
				leftType = FLAT,
				originHeightDiff = -1,
			}, new() {
				points = bottomRightBumpPoints,
				heightAdjustment = 0,
				bottomEdge = UP,
				rightEdge = UP,
			}},
			{new() { // Done
				upType = FLAT,
				leftType = DOWN,
				originHeightDiff = 1,
			}, new() {
				points = topSlopePoints,
				heightAdjustment = 0,
				bottomEdge = FLAT,
				rightEdge = DOWN,
			}},
			{new() { // Done
				upType = FLAT,
				leftType = DOWN,
				originHeightDiff = 0,
			}, new() {
				points = bottomLeftDipPoints,
				heightAdjustment = -1,
				bottomEdge = UP,
				rightEdge = FLAT,
			}},
			{new() { // Done
				upType = FLAT,
				leftType = UP,
				originHeightDiff = 0,
			}, new() {
				points = bottomLeftBumpPoints,
				heightAdjustment = 0,
				bottomEdge = DOWN,
				rightEdge = FLAT,
			}},
			{new() { // Done
				upType = FLAT,
				leftType = UP,
				originHeightDiff = -1,
			}, new() {
				points = bottomSlopePoints,
				heightAdjustment = -1,
				bottomEdge = FLAT,
				rightEdge = UP,
			}},
			{new() { // Done
				upType = DOWN,
				leftType = FLAT,
				originHeightDiff = 1,
			}, new() {
				points = leftSlopePoints,
				heightAdjustment = 0,
				bottomEdge = DOWN,
				rightEdge = FLAT,
			}},
			{new() { // Done
				upType = DOWN,
				leftType = FLAT,
				originHeightDiff = 0,
			}, new() {
				points = topRightDipPoints,
				heightAdjustment = -1,
				bottomEdge = FLAT,
				rightEdge = UP,
			}},
			{new() { // Done
				upType = DOWN,
				leftType = DOWN,
				originHeightDiff = 1,
			}, new() {
				points = topLeftBumpPoints,
				heightAdjustment = -1,
				bottomEdge = FLAT,
				rightEdge = FLAT,
			}},
			{new() { // Done
				upType = UP,
				leftType = FLAT,
				originHeightDiff = 0,
			}, new() {
				points = topRightBumpPoints,
				heightAdjustment = -1,
				bottomEdge = FLAT,
				rightEdge = DOWN,
			}},
			{new() { // Done
				upType = UP,
				leftType = FLAT,
				originHeightDiff = -1,
			}, new() {
				points = rightSlopePoints,
				heightAdjustment = -1,
				bottomEdge = UP,
				rightEdge = FLAT,
			}},
			{new() { // Done
				upType = UP,
				leftType = UP,
				originHeightDiff = -1,
			}, new() {
				points = topLeftDipPoints,
				heightAdjustment = -1,
				bottomEdge = UP,
				rightEdge = FLAT,
			}},
		};

		public static MeshInstance3D MakeMesh()
		{
			// Initialize the ArrayMesh.
			var arrMesh = new ArrayMesh();
			var arrays = new Godot.Collections.Array();
			arrays.Resize((int)Mesh.ArrayType.Max);
			List<Vector3> points = new();

			FastNoiseLite noise = new();
			int smoothness = 2;
			int maxHeight = 10;
			float[] heightmap = new float[10 * 10];
			/*for (int y = 0; y < 10; y += 1)
			{
				for (int x = 0; x < 10; x += 1)
				{
					heightmap[x + y * 10] = noise.GetNoise2D((float)x / smoothness, (float)y / smoothness) * maxHeight;
				}
			}*/
			heightmap = new float[]{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, -1, -1, 0, 0, 0, 0, -1, -1, 0,
				0, -1, -1, 0, 0, 0, 0, -1, -1, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, -1, -1, 0, 0, 0, 0, -1, -1, 0,
				0, -1, -1, 0, 0, 0, 0, -1, -1, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			};

			TileData[] tileData = new TileData[11 * 11];
			for (int i = 0; i < 10; ++i)
			{
				tileData[i + 1] = new TileData() { bottomType = FLAT, rightType = FLAT, nextOriginHeight = 0 };
				tileData[(i + 1) * 11] = new TileData() { bottomType = FLAT, rightType = FLAT, nextOriginHeight = 0 };
			}
			{
				int x = 0, y = 0;
				while (y < 10)
				{
					int height = Mathf.FloorToInt(heightmap[x + y * 10]);
					TileData top = tileData[(x + 1) + y * 11];
					TileData left = tileData[x + (y + 1) * 11];
					int originHeight = tileData[x + y * 11].nextOriginHeight;
					Vector3[] newPoints;
					int heightAdjustment = 0;
					GD.Print(string.Format("x:{0} y:{1} hd:{2} top:{3} left:{4}", x, y, (originHeight - height).ToString(), top.bottomType, left.rightType));
					
					TileIndex tileIndex = new() {
						upType = top.bottomType,
						leftType = left.rightType,
						originHeightDiff = originHeight - height,
					};
					if (tileIndexToTileData.ContainsKey(tileIndex))
					{
						SquareData squareData = tileIndexToTileData[tileIndex];
						tileData[(x + 1) + (y + 1) * 11] = new TileData() {
							bottomType = squareData.bottomEdge,
							rightType = squareData.rightEdge,
							nextOriginHeight = height,
						};
						newPoints = squareData.points;
						heightAdjustment = squareData.heightAdjustment;
					}
					else
					{
						tileData[(x + 1) + (y + 1) * 11] = new TileData() {
							bottomType = FLAT,
							rightType = FLAT,
							nextOriginHeight = height
						};
						newPoints = flatPoints;
					}
					for (int i = 0; i < 6; ++i)
					{
						points.Add(new Vector3(
							newPoints[i].X + x,
							(newPoints[i].Y + height + heightAdjustment) / 2.0f,
							newPoints[i].Z + y
						));
					}
					if (y == 9) {
						y = x + 1;
						x = 9;
					}
					else if (x == 0)
					{
						x = y + 1;
						y = 0;
					}
					else
					{
						x -= 1;
						y += 1;
					}
				}
			}

			arrays[(int)Mesh.ArrayType.Vertex] = points.ToArray();

			// Create the Mesh.
			arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
			var m = new MeshInstance3D();
			m.Mesh = arrMesh;
			m.MaterialOverride = GD.Load<Material>("res://Materials/3d_mat.tres");

			m.Position = new Vector3(-5.0f, -2.0f, 4.0f);
			Base.Root.instance.AddChild(m);

			return m;
		}
	}
}
