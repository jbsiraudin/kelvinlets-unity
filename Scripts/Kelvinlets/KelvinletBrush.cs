using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using NaughtyAttributes;
using Unity.Mathematics;

namespace Kelvinlet
{
    public enum Type
    {
        Grab,
        Scale,
        Twist,
        Pinch
    }

    public enum Mode
    {
        Pulse,
        Push
    }

    [RequireComponent(typeof(KelvinletMesh))]
    public class KelvinletBrush : MonoBehaviour
    {
        public Type _type;
        Mode _mode;

        Vector3 _pos;
        Vector3 _force;
        float3x3 _affineForce;

        // Epsilon is the radial scale of the brush
        private float _eps;

        // Time values
        float _triggerTime = 0.0f;
        float _maxTime = 5.0f;

        //[ProgressBar("Completion", 100, EColor.Blue)]
        int life = 0;

        public void SetBrush(Mode mode, Type type, Vector3 point, Vector3 f, float eps, float maxTime)
        {
            _type = type;
            _mode = mode;
            _pos = point;
            _force = f;
            _maxTime = maxTime;
            _triggerTime = Time.time;
            _eps = eps;
            CalibrateForce();
        }

        public void SetAffineBrush(Mode mode, Type type, Vector3 point, float3x3 f, float eps, float maxTime)
        {
            _type = type;
            _mode = mode;
            _pos = point;
            _affineForce = f;
            _maxTime = maxTime;
            _triggerTime = Time.time;
            _eps = eps;
            CalibrateForce();
        }

        public Brush GetBrush()
        {
            return new Brush(_pos, _force, _eps, Time.time - _triggerTime);
            // return new Brush(_pos, _force, _eps, 0.01f);
        }

        public AffineBrush GetAffineBrush()
        {
            return new AffineBrush(_pos, _affineForce, _eps, Time.time - _triggerTime);
        }

        public bool isPulse()
        {
            return _mode == Mode.Pulse;
        }

        public bool isAffine()
        {
            return _type != Type.Grab;
        }

        void CalibrateForce()
        {
            // vec4 : x: mu / y: nu / z: alpha / w: beta
            Vector4 material = GetComponent<KelvinletMesh>().GetMaterial();
            float alpha = material.z;
            float beta = material.w;

            if (_mode == Mode.Pulse && _type == Type.Grab)
            {
                _force *= (10f * material.w * _eps * _eps);
            }

            if (_mode == Mode.Push && _type == Type.Grab)
            {
                float mu = material.x;
                float nu = material.y;
                float a = 1f / (4f * Mathf.PI * mu);
                float b = a / (4f * (1f - nu));
                _force *= 2f * _eps / (3f * a - 2f * b);
            }

            if (_mode == Mode.Pulse && _type == Type.Scale)
            {
                // Scale
                float s = (_affineForce[0][0] + _affineForce[1][1] + _affineForce[2][2]) / 3f;

                float e4 = Mathf.Pow(_eps, 4);

                // Calibration
                if (alpha >= float.MaxValue)
                {
                    s = 0f;
                }
                else
                {
                    float sFactor = -10f * alpha * e4;
                    s *= sFactor / 5f;
                }

                // Reconstruct
                _affineForce = s * float3x3.identity;
            }

            if (_mode == Mode.Pulse && _type == Type.Pinch)
            {
                // Pinch
                float3x3 P = float3x3.zero;
                float3x3 tr = math.transpose(_affineForce);
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        P[i][j] = 0.5f * (_affineForce[i][j] + tr[i][j]);
                    }
                }

                float e4 = Mathf.Pow(_eps, 4);
                float tFactor = -10f * beta * e4;

                // Calibration
                if (alpha >= float.MaxValue)
                {
                    float sTmp = tFactor / 2f;
                    P *= sTmp;
                }
                else
                {
                    float sFactor = -10f * alpha * e4;
                    float sTmp = 1f / (2f / tFactor + 3f / sFactor);
                    P *= sTmp;
                }

                // Reconstruct
                _affineForce = P;
            }

            if (_mode == Mode.Pulse && _type == Type.Twist)
            {
                // Twist
                Vector3 q;
                q.x = 0.5f * (_affineForce[2][1] - _affineForce[1][2]);
                q.y = 0.5f * (_affineForce[0][2] - _affineForce[2][0]);
                q.z = 0.5f * (_affineForce[1][0] - _affineForce[0][1]);

                float e4 = Mathf.Pow(_eps, 4);
                float tFactor = -10f * beta * e4;

                // Calibration
                q *= tFactor / 5f;

                // Reconstruct
                _affineForce = AssembleSkewSymMatrix(q);
            }
        }

        static public float3x3 AssembleSkewSymMatrix(Vector3 x)
        {
            float3x3 A = float3x3.zero;
            /* A[0, 0] =  0f; */ A[0][1] = -x.z; A[0][2] = x.y;
            A[1][0] = x.z; /* A[1, 1] =   0f; */ A[1][2] = -x.x;
            A[2][0] = -x.y; A[2][1] = x.x; /* A[2, 2] =   0f;*/
            return A;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (_mode == Mode.Pulse)
            {
                Destroy(this, _maxTime);
            }
        }

        private void FixedUpdate()
        {
            life = (int)(100.0f * ((Time.time - _triggerTime) / _maxTime));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(_pos), Mathf.Lerp(0.1f, 0.0f, life/100.0f));
        }
    }
}
