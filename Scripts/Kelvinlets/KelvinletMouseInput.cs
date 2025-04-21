using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using MathNet.Numerics.LinearAlgebra;
using NaughtyAttributes;
using Unity.Mathematics;

namespace Kelvinlet
{
    [System.Serializable]
    public class ToggleEvent : UnityEvent<Type>
    {

    }

    public class KelvinletMouseInput : MonoBehaviour
    {
        public float strength = 1.0f;
        public float offset = 0.2f;
        public float radius = 0.5f;
        Mode mode = Mode.Pulse;
        public Type type = Type.Grab;

        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        bool pushInputMode = false;

        [Header("Outline of the brush for radius viz")]
        public RectTransform outline;

        [Header("On scroll roll")]
        public UnityEvent scroll;

        [Header("On scroll push")]
        public UnityEvent middleBtn;

        [Header("On toggle push")]
        public ToggleEvent toggled;

        private void Start()
        {
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = mouse.position.ReadValue();

            if (mouse.scroll.IsActuated())
            {
                float bump = mouse.scroll.ReadValue().y / 6f;
                outline.sizeDelta += new Vector2(bump, bump);
                radius = outline.sizeDelta.x / 400f;

                scroll.Invoke();
            }

            if (mouse.middleButton.IsPressed())
            {
                Vector2 pos = mouse.position.ReadValue();
                strength = 2f * pos.y / Screen.height;
                offset = 0.1f + pos.x / Screen.width;

                middleBtn.Invoke();
            }

            if (keyboard.cKey.wasPressedThisFrame)
            {
                pushInputMode = !pushInputMode;
            }

            // Preinput
            if (pushInputMode && mouse.rightButton.isPressed)
            {
                HandlePreInput();
            }
            if (pushInputMode && mouse.rightButton.wasReleasedThisFrame)
            {
                HandleInput();
            }

            // Normal input
            if (!pushInputMode && mouse.rightButton.wasPressedThisFrame)
            {
                if (type == Type.Grab)
                {
                    HandleInput();
                }
                else
                {
                    HandleAffineInput();
                }
            }
        }

        void HandlePreInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(inputRay, out hit))
            {
                KelvinletMesh deformer = hit.collider.GetComponent<KelvinletMesh>();

                if (deformer)
                {
                    Vector3 point = hit.point;
                    point += hit.normal * offset;

                    point = hit.transform.InverseTransformPoint(point);
                    Vector3 force = strength * hit.normal;

                    deformer.PreBrush(mode, type, point, force, radius);
                }
            }
        }

        void HandleInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(inputRay, out hit))
            {
                KelvinletMesh deformer = hit.collider.GetComponent<KelvinletMesh>();

                if (deformer)
                {
                    Vector3 point = hit.point;
                    point += hit.normal * offset;
                    Debug.DrawLine(hit.point, point, Color.red);

                    point = hit.transform.InverseTransformPoint(point);
                    Debug.DrawLine(Camera.main.transform.position, hit.point, Color.green);

                    Vector3 force = strength * hit.normal;
                    
                    deformer.AddBrush(mode, type, point, force, radius);
                }
            }
        }

        void HandleAffineInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(inputRay, out hit))
            {
                KelvinletMesh deformer = hit.collider.GetComponent<KelvinletMesh>();

                if (deformer)
                {
                    Vector3 point = hit.point;
                    point += hit.normal * offset;
                    Debug.DrawLine(hit.point, point, Color.red);

                    point = hit.transform.InverseTransformPoint(point);
                    Debug.DrawLine(Camera.main.transform.position, hit.point, Color.green);

                    float3x3 force;
                    if (type == Type.Scale)
                    {
                        force = strength * float3x3.identity;

                        deformer.AddAffineBrush(mode, type, point, force, radius);
                    }
                    else if (type == Type.Pinch)
                    {
                        force = float3x3.zero;
                        force[0][0] = strength * 2f;
                        force[1][1] = strength * -2f;

                        deformer.AddAffineBrush(mode, type, point, force, radius);
                    }
                    else if (type == Type.Twist)
                    {
                        Vector3 axisAngle = strength * 0.5f * Mathf.PI * hit.normal;
                        force = KelvinletBrush.AssembleSkewSymMatrix(axisAngle);

                        deformer.AddAffineBrush(mode, type, point, force, radius);
                    }
                }
            }
        }

        public float GetEpsilon()
        {
            return radius;
        }

        public float GetForce()
        {
            return strength;
        }

        public float GetOffset()
        {
            return offset;
        }

        public void SetTranslate(bool val)
        {
            if (val)
            {
                type = Type.Grab;
            }
        }

        public void SetTwist(bool val)
        {
            if (val)
            {
                type = Type.Twist;
            }
        }

        public void SetScale(bool val)
        {
            if (val)
            {
                type = Type.Scale;
            }
        }

        public void SetPinch(bool val)
        {
            if (val)
            {
                type = Type.Pinch;
            }
        }
    }
}

