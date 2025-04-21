using System;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

[ExecuteInEditMode]
public class BrushPlacer : PathSceneTool {

    public GameObject prefab;
    public GameObject arrow;
    public GameObject holder;
    public GameObject holder2;
    public float spacing = 3;

    const float minSpacing = .1f;
    private List<Vector3> points;

    void Generate () {
        if (pathCreator != null && prefab != null && holder != null && holder2 != null) {
            DestroyObjects ();
            points = new List<Vector3>();

            VertexPath path = pathCreator.path;

            spacing = Mathf.Max(minSpacing, spacing);
            float dst = 0;

            while (dst < path.length) {
                Vector3 point = path.GetPointAtDistance (dst);
                Quaternion rot = path.GetRotationAtDistance (dst);
                // Instantiate(prefab, point, rot, holder.transform);
                Instantiate(arrow, point, rot, holder2.transform);
                dst += spacing;
                points.Add(point);
            }

            int numArrows = holder2.transform.childCount;
            for (int i = 0; i < numArrows - 1; i++)
            {
                holder2.transform.GetChild(i).gameObject.GetComponent<ArrowMesh>().FromTo(points[i], points[Math.Min(i + 1, points.Count - 1)]);
            }
            DestroyImmediate(holder2.transform.GetChild(numArrows - 1).gameObject, false);
        }
    }

    void DestroyObjects () {
        int numChildren = holder.transform.childCount;
        for (int i = numChildren - 1; i >= 0; i--) {
            DestroyImmediate (holder.transform.GetChild (i).gameObject, false);
        }

        int numArrows = holder2.transform.childCount;
        for (int i = numArrows - 1; i >= 0; i--)
        {
            DestroyImmediate(holder2.transform.GetChild(i).gameObject, false);
        }
    }

    protected override void PathUpdated () {
        if (pathCreator != null) {
            Generate ();
        }
    }
}
