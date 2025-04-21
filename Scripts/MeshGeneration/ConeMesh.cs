using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConeMesh : MonoBehaviour
{
	public float height = 1f;
	public float bottomRadius = .25f;
	public float topRadius = .05f;
	public int nbSides = 18;

	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;

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
		mesh.name = "Procedural Cone";

		CreateVertices();
		CreateTriangles();
		CreateColliders();
	}

	public void GenerateVue()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Cone";

		CreateVertices();
		CreateTriangles();

		mesh.RecalculateBounds();
		mesh.Optimize();
	}

	private void CreateVertices()
	{
		int nbVerticesCap = nbSides + 1;

		// bottom + top + sides
		vertices = new Vector3[nbVerticesCap + nbVerticesCap + nbSides * 2 + 2];
		normals = new Vector3[vertices.Length];
		int vert = 0;
		float _2pi = Mathf.PI * 2f;

		// Bottom cap
		vertices[vert++] = new Vector3(0f, 0f, 0f);
		while (vert <= nbSides)
		{
			float rad = (float)vert / nbSides * _2pi;
			vertices[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
			normals[vert] = Vector3.down;
			vert++;
		}

		// Top cap
		vertices[vert++] = new Vector3(0f, height, 0f);
		while (vert <= nbSides * 2 + 1)
		{
			float rad = (float)(vert - nbSides - 1) / nbSides * _2pi;
			vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
			normals[vert] = Vector3.up;
			vert++;
		}

		// Sides
		int v = 0;
		while (vert <= vertices.Length - 4)
		{
			float rad = (float)v / nbSides * _2pi;
			vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
			vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
			normals[vert] = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
			normals[vert + 1] = normals[vert];
			vert += 2;
			v++;
		}
		vertices[vert] = vertices[nbSides * 2 + 2];
		vertices[vert + 1] = vertices[nbSides * 2 + 3];
		normals[vert] = normals[nbSides * 2 + 2];
		normals[vert + 1] = normals[nbSides * 2 + 3];

		mesh.vertices = vertices;
		mesh.normals = normals;
	}

	private void CreateTriangles()
	{
		int nbVerticesCap = nbSides + 1;
		int nbTriangles = nbSides + nbSides + nbSides * 2;
		int[] triangles = new int[nbTriangles * 3 + 3];

		// Bottom cap
		int tri = 0;
		int i = 0;
		while (tri < nbSides - 1)
		{
			triangles[i] = 0;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = tri + 2;
			tri++;
			i += 3;
		}
		triangles[i] = 0;
		triangles[i + 1] = tri + 1;
		triangles[i + 2] = 1;
		tri++;
		i += 3;

		// Top cap
		//tri++;
		while (tri < nbSides * 2)
		{
			triangles[i] = tri + 2;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = nbVerticesCap;
			tri++;
			i += 3;
		}

		triangles[i] = nbVerticesCap + 1;
		triangles[i + 1] = tri + 1;
		triangles[i + 2] = nbVerticesCap;
		tri++;
		i += 3;
		tri++;

		// Sides
		while (tri <= nbTriangles)
		{
			triangles[i] = tri + 2;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = tri + 0;
			tri++;
			i += 3;

			triangles[i] = tri + 1;
			triangles[i + 1] = tri + 2;
			triangles[i + 2] = tri + 0;
			tri++;
			i += 3;
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

		gameObject.AddComponent<SphereCollider>();
	}
}
