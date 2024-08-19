using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace EvolutionGame
{
	public class HeightMesh
	{
		public static MeshInstance3D MakeMesh()
		{
			var vertices = new Vector3[]
			{
				new(0, 1, 0),
				new(1, 0, 0),
				new(0, 0, 1),
			};

			// Initialize the ArrayMesh.
			var arrMesh = new ArrayMesh();
			var arrays = new Godot.Collections.Array();
			arrays.Resize((int)Mesh.ArrayType.Max);
			arrays[(int)Mesh.ArrayType.Vertex] = vertices;

			// Create the Mesh.
			arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
			var m = new MeshInstance3D();
			m.Mesh = arrMesh;
			m.MaterialOverride = GD.Load<Material>("res://Materials/3d_mat.tres");

			Base.Root.instance.AddChild(m);
			m.Position = new Vector3(0.0f, 0.0f, 1.0f);

			return m;
		}
	}
}
