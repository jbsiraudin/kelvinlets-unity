using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TubeMesh : MonoBehaviour
{
	public float height = 1f;
	public int nbSides = 24;

	// Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
	public float bottomRadius1 = .5f;
	public float bottomRadius2 = .15f;
	public float topRadius1 = .5f;
	public float topRadius2 = .15f;

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
		mesh.name = "Procedural Tube";

		CreateVertices();
		CreateTriangles();
		CreateColliders();

		mesh.RecalculateBounds();
		mesh.Optimize();
	}

	public void GenerateVue()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Tube";

		CreateVertices();
		CreateTriangles();

		mesh.RecalculateBounds();
		mesh.Optimize();
	}

	private void CreateVertices()
	{
		int nbVerticesCap = nbSides * 2 + 2;
		int nbVerticesSides = nbSides * 2 + 2;

		// bottom + top + sides
		vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
		normals = new Vector3[vertices.Length];
		int vert = 0;
		float _2pi = Mathf.PI * 2f;

		// Bottom cap
		int sideCounter = 0;
		while (vert < nbVerticesCap)
		{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			float r1 = (float)(sideCounter++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);
			vertices[vert] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
			vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
			normals[vert] = Vector3.down;
			normals[vert + 1] = Vector3.down;
			vert += 2;
		}

		// Top cap
		sideCounter = 0;
		while (vert < nbVerticesCap * 2)
		{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			float r1 = (float)(sideCounter++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);
			vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
			vertices[vert + 1] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
			normals[vert] = Vector3.up;
			normals[vert + 1] = Vector3.up;
			vert += 2;
		}

		// Sides (out)
		sideCounter = 0;
		while (vert < nbVerticesCap * 2 + nbVerticesSides)
		{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			float r1 = (float)(sideCounter++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);

			vertices[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
			vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
			normals[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
			normals[vert + 1] = normals[vert];
			vert += 2;
		}

		// Sides (in)
		sideCounter = 0;
		while (vert < vertices.Length)
		{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			float r1 = (float)(sideCounter++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);

			vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
			vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
			normals[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
			normals[vert + 1] = normals[vert];
			vert += 2;
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
	}

	private void CreateTriangles()
	{
		int nbFace = nbSides * 4;
		int nbTriangles = nbFace * 2;
		int nbIndexes = nbTriangles * 3;
		int[] triangles = new int[nbIndexes];

		// Bottom cap
		int i = 0;
		int sideCounter = 0;
		while (sideCounter < nbSides)
		{
			int current = sideCounter * 2;
			int next = sideCounter * 2 + 2;

			triangles[i++] = next + 1;
			triangles[i++] = next;
			triangles[i++] = current;

			triangles[i++] = current + 1;
			triangles[i++] = next + 1;
			triangles[i++] = current;

			sideCounter++;
		}

		// Top cap
		while (sideCounter < nbSides * 2)
		{
			int current = sideCounter * 2 + 2;
			int next = sideCounter * 2 + 4;

			triangles[i++] = current;
			triangles[i++] = next;
			triangles[i++] = next + 1;

			triangles[i++] = current;
			triangles[i++] = next + 1;
			triangles[i++] = current + 1;

			sideCounter++;
		}

		// Sides (out)
		while (sideCounter < nbSides * 3)
		{
			int current = sideCounter * 2 + 4;
			int next = sideCounter * 2 + 6;

			triangles[i++] = current;
			triangles[i++] = next;
			triangles[i++] = next + 1;

			triangles[i++] = current;
			triangles[i++] = next + 1;
			triangles[i++] = current + 1;

			sideCounter++;
		}


		// Sides (in)
		while (sideCounter < nbSides * 4)
		{
			int current = sideCounter * 2 + 6;
			int next = sideCounter * 2 + 8;

			triangles[i++] = next + 1;
			triangles[i++] = next;
			triangles[i++] = current;

			triangles[i++] = current + 1;
			triangles[i++] = next + 1;
			triangles[i++] = current;

			sideCounter++;
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
