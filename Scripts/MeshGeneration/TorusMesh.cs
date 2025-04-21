using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TorusMesh : MonoBehaviour
{

	public float radius1 = 1f;
	public float radius2 = .3f;
	public int nbRadSeg = 24;
	public int nbSides = 18;

	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color32[] cubeUV;

	private void Awake()
	{
		Generate();
	}

	private void OnValidate()
	{
		GenerateVue();
	}

	private void Generate()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Sphere";

		CreateVertices();
		CreateTriangles();
		CreateColliders();
	}

	private void GenerateVue()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Sphere";

		CreateVertices();
		CreateTriangles();
	}

	private void CreateVertices()
	{
		vertices = new Vector3[(nbRadSeg + 1) * (nbSides + 1)];
		normals = new Vector3[vertices.Length];
		float _2pi = Mathf.PI * 2f;

		for (int seg = 0; seg <= nbRadSeg; seg++)
		{
			int currSeg = seg == nbRadSeg ? 0 : seg;

			float t1 = (float)currSeg / nbRadSeg * _2pi;
			Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

			for (int side = 0; side <= nbSides; side++)
			{
				int currSide = side == nbSides ? 0 : side;

				Vector3 normale = Vector3.Cross(r1, Vector3.up);
				float t2 = (float)currSide / nbSides * _2pi;
				Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) * new Vector3(Mathf.Sin(t2) * radius2, Mathf.Cos(t2) * radius2);

				vertices[side + seg * (nbSides + 1)] = r1 + r2;
				normals[side + seg * (nbSides + 1)] = (vertices[side + seg * (nbSides + 1)] - r1).normalized;
			}
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
	}

	private void CreateTriangles()
	{
		int nbFaces = vertices.Length;
		int nbTriangles = nbFaces * 2;
		int nbIndexes = nbTriangles * 3;
		int[] triangles = new int[nbIndexes];

		int i = 0;
		for (int seg = 0; seg <= nbRadSeg; seg++)
		{
			for (int side = 0; side <= nbSides - 1; side++)
			{
				int current = side + seg * (nbSides + 1);
				int next = side + (seg < (nbRadSeg) ? (seg + 1) * (nbSides + 1) : 0);

				if (i < triangles.Length - 6)
				{
					triangles[i++] = current;
					triangles[i++] = next;
					triangles[i++] = next + 1;

					triangles[i++] = current;
					triangles[i++] = next + 1;
					triangles[i++] = current + 1;
				}
			}
		}

		mesh.triangles = triangles;
	}

	private void CreateColliders()
	{
		SphereCollider[] sphereColliders = gameObject.GetComponents<SphereCollider>();
		foreach (SphereCollider sphere in sphereColliders)
		{
			if (sphere != null)
			{
				DestroyImmediate(sphere);
			}
		}

		float _2pi = Mathf.PI * 2f;

		for (int seg = 0; seg <= nbRadSeg; seg+=4)
		{
			int currSeg = seg == nbRadSeg ? 0 : seg;

			float t1 = (float)currSeg / nbRadSeg * _2pi;
			AddSphereCollider(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1, radius2);
		}
	}

	private void AddSphereCollider(float x, float y, float z, float radius)
	{
		SphereCollider c = gameObject.AddComponent<SphereCollider>();
		c.center = new Vector3(x, y, z);
		c.radius = radius;
	}
}
