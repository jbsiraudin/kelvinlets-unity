using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlaneMesh : MonoBehaviour
{

	public float length = 1f;
	public float width = 1f;
	public int res = 2; // 2 minimum

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
		//CreateColliders();

		mesh.RecalculateBounds();
		mesh.Optimize();
	}

	private void GenerateVue()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Sphere";

		CreateVertices();
		CreateTriangles();
		//CreateColliders();

		mesh.RecalculateBounds();
		mesh.Optimize();
	}

	private void CreateVertices()
	{
		vertices = new Vector3[res * res];
		normals = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];

		for (int z = 0; z < res; z++)
		{
			// [ -length / 2, length / 2 ]
			float zPos = ((float)z / (res - 1) - .5f) * length;
			for (int x = 0; x < res; x++)
			{
				// [ -width / 2, width / 2 ]
				float xPos = ((float)x / (res - 1) - .5f) * width;
				vertices[x + z * res] = new Vector3(xPos, 0f, zPos);
				normals[x + z * res] = Vector3.up;
				uvs[x + z * res] = new Vector2((float)x / res, (float)z/res);
			}
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
	}

	private void CreateTriangles()
	{
		int nbFaces = (res - 1) * (res - 1);
		int[] triangles = new int[nbFaces * 6];
		int t = 0;
		for (int face = 0; face < nbFaces; face++)
		{
			// Retrieve lower left corner from face ind
			int i = face % (res - 1) + (face / (res - 1) * res);

			triangles[t++] = i + res;
			triangles[t++] = i + 1;
			triangles[t++] = i;

			triangles[t++] = i + res;
			triangles[t++] = i + res + 1;
			triangles[t++] = i + 1;
		}

		mesh.triangles = triangles;
	}


	private void CreateColliders()
	{
		BoxCollider[] boxColliders = gameObject.GetComponents<BoxCollider>();
		foreach (BoxCollider box in boxColliders)
		{
			if (box != null)
			{
				DestroyImmediate(box);
			}
		}

		gameObject.AddComponent<BoxCollider>();
	}
}
