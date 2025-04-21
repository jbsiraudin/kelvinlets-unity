using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

namespace Kelvinlet
{
    public class PulseScaleCPU : MonoBehaviour
    {
        Vector3 _pos;
        public Matrix<float> _force = Matrix<float>.Build.Dense(3, 3);

        public float _triggerTime = 0.0f;

        float _eps = 1.0f;
        Vector4 material;
        float _alpha, _beta;

        float _maxValue = float.MaxValue;
        float _maxTime = 5f;
        float _minValue = 0.01f;

        struct kelField
        {
            public Vector3 Ua;
            public Vector3 Ub;
            public float r;
        }

        private void Start()
        {
            Destroy(this, _maxTime);
        }

        // Set wave speeds via the material parameters
        // Stiffness mu > 0 and Poisson ratio nu <= 0.5

        public void SetBrush(Vector3 point, Matrix4x4 f, float eps, float maxTime, float minValue, float maxValue)
        {
            _pos = point;
            _force = build(f);
            _maxTime = maxTime;
            _minValue = minValue;
            _maxValue = maxValue;
            _triggerTime = Time.time;
            _eps = eps;
            CalibrateForce();
        }

        Matrix<float> build(Matrix4x4 m)
        {
            Matrix<float> mr = Matrix<float>.Build.Dense(3, 3);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    mr[i, j] = m[i, j];
                }
            }
            return mr;
        }

        // Eq. 9a in [de Goes and James 2018]
        // Convention on Vec3 Ua & Ub:
        // [0] - function
        // [1] - gradient
        // [2] - hessian 
        float EvalA(kelField values)
        {
            return values.Ua.x + 2f * values.Ub.x + values.r * values.Ub.y;
        }

        // Eq. 9b in [de Goes and James 2018]
        float EvalB(kelField values)
        {
            if (values.r < _minValue)
            {
                return 0f;
            }
            return (values.Ua.y - values.Ub.y) / values.r;
        }

        float EvalGradA(kelField values)
        {
            // .Y gradient, .Z hessian
            return values.Ua.y + 3f * values.Ub.y + values.r * values.Ub.z;
        }

        float EvalGradB(kelField values)
        {
            return (values.Ua.z - values.Ub.z - EvalB(values)) / values.r;
        }

        float EvalDen(float r, float a)
        {
            return 16f * Mathf.PI * a * Mathf.Pow(r, 3);
        }

        float EvalW(float r, float R)
        {
            // Eq. 8b in [de Goes and James 2018]

            float e2 = Mathf.Pow(_eps, 2);
            float R2 = R * R;
            float Re2 = R2 + e2;
            float Re = Mathf.Sqrt(Re2);

            float num = e2 + 2f * R2 - r * R * (3f - R2 / Re2);
            return num / Re;
        }

        float EvalGradW(float r, float R)
        {
            // See Appendix C in [de Goes and James 2018]

            float e2 = Mathf.Pow(_eps, 2);
            float R2 = R * R;
            float Re2 = R2 + e2;
            float Re = Mathf.Sqrt(Re2);

            float num = -3f * e2 * e2 * r;
            float den = Re2 * Re2 * Re;
            return num / den;
        }

        float EvalHessW(float r, float R)
        {
            // See Appendix C in [de Goes and James 2018]

            float e2 = Mathf.Pow(_eps, 2);
            float R2 = R * R;
            float Re2 = R2 + e2;
            float Re = Mathf.Sqrt(Re2);

            float num = -3f * e2 * e2 * (Re2 - 5f * r * R);
            float den = Re2 * Re2 * Re2 * Re;
            return num / den;
        }

        float EvalGradU(float r, float t, float a)
        {
            if (a >= _maxValue || t >= _maxTime || r < _minValue)
            {
                return 0f;
            }

            float fP = EvalW(r, r + a * t);
            float fN = EvalW(r, r - a * t);
            float gP = EvalGradW(r, r + a * t);
            float gN = EvalGradW(r, r - a * t);
            return ((gP - gN) - 3f * (fP - fN) / r) / EvalDen(r, a);
        }

        float EvalHessU(float r, float t, float a)
        {
            if (a >= _maxValue || t >= _maxTime || r < _minValue)
            {
                return 0f;
            }

            float r2 = r * r;
            float fP = EvalW(r, r + a * t);
            float fN = EvalW(r, r - a * t);
            float gP = EvalGradW(r, r + a * t);
            float gN = EvalGradW(r, r - a * t);
            float hP = EvalHessW(r, r + a * t);
            float hN = EvalHessW(r, r - a * t);
            return ((hP - hN) - 6f * (gP - gN) / r + 12f * (fP - fN) / r2) / EvalDen(r, a);
        }

        void CalibrateForce()
        {
            // vec4 : x: mu / y: nu / z: alpha / w: beta
            material = GetComponent<KelvinletMeshCPU>().GetMaterial();
            _alpha = material.z;
            _beta = material.w;

            // Scale
            float s = _force.Trace() / 3f;

            float e4 = Mathf.Pow(_eps, 4);

            // Calibration
            if (_alpha >= float.MaxValue)
            {
                s = 0f;
            }
            else
            {
                float sFactor = -10f * _alpha * e4;
                s *= sFactor / 5f;
            }

            // Reconstruct
            _force = s * Matrix<float>.Build.DenseIdentity(3);
        }

        kelField Compute(float r, float t)
        {
            kelField kel;
            kel.Ua = Vector3.zero;
            kel.Ub = Vector3.zero;

            kel.r = r;
            kel.Ua.y = EvalGradU(r, t, _alpha);
            kel.Ua.z = EvalHessU(r, t, _alpha);

            kel.Ub.y = EvalGradU(r, t, _beta);
            kel.Ub.z = EvalHessU(r, t, _beta);
            return kel;
        }

        Vector3 EvalDisp(Vector3 query, float time)
        {
            float t = time - _triggerTime;
            if (t <= 0)
            {
                return Vector3.zero;
            }

            Vector3 x = (query - _pos);
            float[] xArray = new float[] { x.x, x.y, x.z };
            Vector<float> xMath = Vector<float>.Build.DenseOfArray(xArray);
            float r = x.sqrMagnitude;

            kelField values = Compute(r, t);

            Vector<float> FxMath = _force * xMath;
            Vector3 Fx = new Vector3(FxMath[0], FxMath[1], FxMath[2]);

            Vector<float> TrTrMath = _force.Transpose() * xMath + _force.Trace() * xMath;
            Vector3 TrTr = new Vector3(TrTrMath[0], TrTrMath[1], TrTrMath[2]);

            Vector3 result = Vector3.zero;
            result += EvalGradA(values) * (Fx / r);
            result += EvalGradB(values) * Vector3.Dot(x, Fx) * (x / r);
            result += EvalB(values) * TrTr;
            return result;
        }

        public Vector3 EvalDispRK4(Vector3 query, float time)
        {
            Vector3 v0 = EvalDisp(query, time);
            Vector3 v1 = EvalDisp(query + 0.5f * v0, time);
            Vector3 v2 = EvalDisp(query + 0.5f * v1, time);
            Vector3 v3 = EvalDisp(query + v2, time);
            Vector3 tmp = (v0 + 2f * v1 + 2f * v2 + v3) / 6f;
            return tmp;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.TransformPoint(_pos), 0.05f);
        }
    }

}
