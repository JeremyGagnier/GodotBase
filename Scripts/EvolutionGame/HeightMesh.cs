using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace EvolutionGame
{
	public class HeightMesh
	{
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
			public int bottomEdge;	// The slope type at the bottom edge
			public int rightEdge;	// The slope type at the right edge
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
			new(0, 1, 0),
			new(1, 0, 0),
			new(0, 0, 1),
			new(0, 0, 1),
			new(1, 0, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] bottomRightBumpPoints = {
			new(0, 0, 0),
			new(1, 0, 1),
			new(0, 0, 1),
			new(0, 0, 0),
			new(1, 1, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] topLeftBumpPoints = {
			new(0, 0, 0),
			new(1, 0, 1),
			new(0, 1, 1),
			new(0, 0, 0),
			new(1, 0, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] topRightBumpPoints = {
			new(0, 0, 0),
			new(1, 0, 0),
			new(0, 0, 1),
			new(0, 0, 1),
			new(1, 0, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] bottomLeftDipPoints = {
			new(0, 0, 0),
			new(1, 1, 0),
			new(0, 1, 1),
			new(0, 1, 1),
			new(1, 1, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] bottomRightDipPoints = {
			new(0, 1, 0),
			new(1, 1, 1),
			new(0, 1, 1),
			new(0, 1, 0),
			new(1, 0, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] topLeftDipPoints = {
			new(0, 1, 0),
			new(1, 1, 1),
			new(0, 0, 1),
			new(0, 1, 0),
			new(1, 1, 0),
			new(1, 1, 1),
		};
		static readonly Vector3[] topRightDipPoints = {
			new(0, 1, 0),
			new(1, 1, 0),
			new(0, 1, 1),
			new(0, 1, 1),
			new(1, 1, 0),
			new(1, 0, 1),
		};
		static readonly Vector3[] topSlopePoints = {
			new(0, 0, 0),
			new(1, 0, 0),
			new(0, 1, 1),
			new(0, 1, 1),
			new(1, 0, 0),
			new(1, 1, 1),
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
			new(0, 1, 0),
			new(1, 0, 1),
			new(0, 0, 1),
			new(0, 1, 0),
			new(1, 1, 0),
			new(1, 0, 1),
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
		static readonly Dictionary<Tuple<int, int>, Dictionary<int, SquareData>> lineToHeightToPoints = new() {
			{new(0, 0), new() {
				{0, new() {
					points = flatPoints,
					heightAdjustment = 0,
					bottomEdge = 0,
					rightEdge = 0,
				}},
				{-1, new() {
					points = bottomRightDipPoints,
					heightAdjustment = -1,
					bottomEdge = -1,
					rightEdge = -1,
				}},
				{1, new() {
					points = bottomRightBumpPoints,
					heightAdjustment = 0,
					bottomEdge = 1,
					rightEdge = 1,
				}},
			}},
			{new(0, 1), new() {
				{0, new() {
					points = bottomLeftBumpPoints,
					heightAdjustment = 0,
					bottomEdge = -1,
					rightEdge = 0,
				}},
				{1, new() {
					points = bottomSlopePoints,
					heightAdjustment = 0,
					bottomEdge = 0,
					rightEdge = 1,
				}},
			}},
			{new(0, -1), new() {
				{0, new() {
					points = bottomLeftDipPoints,
					heightAdjustment = -1,
					bottomEdge = 1,
					rightEdge = 0,
				}},
				{-1, new() {
					points = topSlopePoints,
					heightAdjustment = -1,
					bottomEdge = 0,
					rightEdge = -1,
				}},
			}},
			{new(-1, 0), new() {
				{0, new() {
					points = topRightDipPoints,
					heightAdjustment = -1,
					bottomEdge = 0,
					rightEdge = 1,
				}},
				{-1, new() {
					points = leftSlopePoints,
					heightAdjustment = -1,
					bottomEdge = -1,
					rightEdge = 0,
				}},
			}},
			{new(-1, -1), new() {
				{-1, new() {
					points = topLeftBumpPoints,
					heightAdjustment = -1,
					bottomEdge = 0,
					rightEdge = 0,
				}},
			}},
			{new(1, 0), new() {
				{0, new() {
					points = topRightBumpPoints,
					heightAdjustment = 0,
					bottomEdge = 0,
					rightEdge = -1,
				}},
				{1, new() {
					points = rightSlopePoints,
					heightAdjustment = 0,
					bottomEdge = 1,
					rightEdge = 0,
				}},
			}},
			{new(1, 1), new() {
				{1, new() {
					points = topLeftDipPoints,
					heightAdjustment = 0,
					bottomEdge = 0,
					rightEdge = 0,
				}},
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

			HeightData[] heightData = new HeightData[11 * 11];
			for (int i = 0; i < 10; ++i)
			{
				heightData[i + 1] = new HeightData() { height = 0, bottomEdge = 0, rightEdge = 0 };
				heightData[(i + 1) * 11] = new HeightData() { height = 0, bottomEdge = 0, rightEdge = 0 };
			}
			{
				int x = 0, y = 0;
				while (y < 10)
				{
					HeightData top = heightData[(x + 1) + y * 11];
					HeightData left = heightData[x + (y + 1) * 11];
					int height = Mathf.FloorToInt(heightmap[x + y * 10] * 2);
					Tuple<int, int> edges = new Tuple<int, int>(top.bottomEdge, left.rightEdge);
					Vector3[] newPoints;
					int heightAdjustment = 0;
					if (lineToHeightToPoints.ContainsKey(edges) &&
						lineToHeightToPoints[edges].ContainsKey(top.height - height))
					{
						SquareData squareData = lineToHeightToPoints[edges][top.height - height];
						heightData[(x + 1) + (y + 1) * 11] = new HeightData() {
							height = height,
							bottomEdge = squareData.bottomEdge,
							rightEdge = squareData.rightEdge,
						};
						newPoints = squareData.points;
						heightAdjustment = squareData.heightAdjustment;
					}
					else
					{
						heightData[(x + 1) + (y + 1) * 11] = new HeightData() {
							height = height,
							bottomEdge = 0,
							rightEdge = 0,
						};
						newPoints = flatPoints;
					}
					for (int i = 0; i < 6; ++i)
					{
						points.Add(new Vector3(
							newPoints[i].X + x,
							(newPoints[i].Y + heightAdjustment) / 2.0f + height / 4.0f,
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
