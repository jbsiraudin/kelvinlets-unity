using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGizmos : MonoBehaviour
{
    public float radius = 0.2f;
    public Color color = Color.red;

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
