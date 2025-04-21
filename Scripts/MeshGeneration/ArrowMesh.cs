using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ArrowMesh : MonoBehaviour
{
    public ConeMesh tip;
    public TubeMesh tube;

    public float height = 1f;
    public float radius = .25f;
    public int nbSides = 18;

	public float tipHeight = 0.1f;
	public float tipRadius = 0.3f;

    public Color color;

    private Material tipMaterial;
    private Material tubeMaterial;

    private void Awake()
    {
        PreBuildArrow();
    }
    private void OnValidate()
    {
        // tipMaterial = tip.gameObject.GetComponent<MeshRenderer>().material;
        // tubeMaterial = tube.gameObject.GetComponent<MeshRenderer>().material;
        // tipMaterial.EnableKeyword("Main Color");
        // tubeMaterial.EnableKeyword("Main Color");
        BuildArrow();
    }

    void PreBuildArrow()
    {
        tip.topRadius = 0f;
        tube.topRadius1 = 0f;
        tube.bottomRadius1 = 0f;
    }

    void BuildArrow()
    {
        tip.topRadius = 0f;
        tip.bottomRadius = tipRadius;
        tip.nbSides = nbSides;
        tip.height = tipHeight;

        tube.topRadius1 = 0f;
        tube.bottomRadius1 = 0f;
        tube.bottomRadius2 = radius;
        tube.topRadius2 = radius;
        tube.nbSides = nbSides;
        tube.height = height - tipHeight;

        tip.GenerateVue();
        tube.GenerateVue();

        tip.transform.localPosition = new Vector3(0, 0.5f * height - tipHeight, 0);
        tube.transform.localPosition = new Vector3(0, - height / 2, 0);

        // tipMaterial.SetColor("_Color", color);
        // tubeMaterial.SetColor("_Color", color);
    }

    public void FromTo(Vector3 from, Vector3 to)
    {
        Vector3 pos = (from + to) / 2;
        Quaternion q = Quaternion.FromToRotation(new Vector3(0, 1, 0), from - to);
        height = (from - to).magnitude;
        BuildArrow();
        
        transform.SetPositionAndRotation(pos, q);
    }
}
