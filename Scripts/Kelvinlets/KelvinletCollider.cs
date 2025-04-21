using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Kelvinlet
{
    public class KelvinletCollider : MonoBehaviour
    {
        float strength = 5.0f;
        float offset = 0.01f;
        float radius = 1.0f;

        private void OnCollisionEnter(Collision collision)
        {
            KelvinletMesh deformer = collision.collider.GetComponent<KelvinletMesh>();

            if(deformer)
            {
                ContactPoint hit = collision.GetContact(0);
                Vector3 hitPoint = hit.point + offset * hit.normal;
                hitPoint = collision.transform.InverseTransformPoint(hitPoint);

                Vector3 force = Mathf.Lerp(0.0f, strength, collision.relativeVelocity.magnitude / 20f) * (hit.normal);
                float eps = Mathf.Lerp(0.0f, radius, collision.relativeVelocity.magnitude / 10f);
                deformer.AddBrush(Mode.Pulse, Type.Grab, hitPoint, force, eps);
            }
        }
    }
}

