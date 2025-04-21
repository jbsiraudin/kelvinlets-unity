using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Kelvinlet
{
    public class KelvinletInputCPU : MonoBehaviour
    {
        public float strength = 10.0f;
        public float offset = 0.2f;
        public float radius = 0.1f;

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                HandleInput();
            }
        }

        void HandleInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(inputRay, out hit))
            {
                KelvinletMeshCPU deformer = hit.collider.GetComponent<KelvinletMeshCPU>();
                Debug.Log(hit.collider.gameObject.name);
                Debug.Log(deformer);

                if (deformer)
                {
                    Vector3 point = hit.point;
                    point += hit.normal * offset;
                    point = hit.transform.InverseTransformPoint(point);
                    Vector3 force = strength * hit.normal;

                    Debug.DrawLine(hit.point, hit.point, Color.red);
                    Debug.DrawLine(Camera.main.transform.position, hit.point, Color.green);

                    deformer.AddBrush(Mode.Pulse, Type.Grab, point, force, radius);
                }
            }
        }
    }

}