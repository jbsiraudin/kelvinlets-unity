using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Unity.Mathematics;

namespace Kelvinlet
{
    [RequireComponent(typeof(MeshFilter))]
    public class KelvinletMeshCPU : MonoBehaviour
    {
        #region Material properties
        [Header("Material properties"), InfoBox("Nu is the Poisson ratio that controls compressibility \n" +
            "Mu is the elastic shear modulus indicating material stiffness", EInfoBoxType.Normal)]

        [SerializeField, Range(0f, 0.5f)]
        public float nu = 0.45f;

        [SerializeField, Range(0.001f, 20f)]
        public float mu = 5.0f;

        // to set with material calibration step
        float _alpha = 0.0f;    // P-Wave speed
        float _beta = 0.0f;     // S-wave speed
        #endregion

        #region Mesh properties
        Mesh _mesh;
        int _vertexCount;
        Vector3[] _originalVertexArray, _displacedVertexArray;
        #endregion

        #region Compute calibration values
        // Those values are used in shader for branching the calculations for extreme cases
        // In particular we consider that the max duration of a pulse is 5 seconds (MaxTime) but
        // it is easily changeable
        float _maxValue = 100f;
        float _minValue = 0.1f;
        float _maxTime = 3.0f;
        #endregion

        #region Brushes arrays
        PulseGrabCPU[] _grabBrushes;
        PulseAffineCPU[] _affineBrushes;
        Brush[] _pulseBrushes;
        AffineBrush[] _pulsesAffineBrushes;
        #endregion

        void SetMaterial()
        {
            Debug.Assert(mu > 0.0f);
            Debug.Assert(nu <= 0.5f);
            _beta = Mathf.Sqrt(mu);
            if (nu == 0.5f)
            {
                _alpha = _maxValue;
            }
            else
            {
                _alpha = _beta * Mathf.Sqrt(1.0f + 1.0f / (1.0f - 2.0f * nu));
            }
        }

        public Vector4 GetMaterial()
        {
            return new Vector4(mu, nu, _alpha, _beta);
        }

        private void OnValidate()
        {
            tag = "Deformable";
            SetMaterial();
        }

        public void SetMu(float value)
        {
            mu = value;
            SetMaterial();
        }

        public void SetNu(float value)
        {
            nu = value;
            SetMaterial();
        }

        public float GetAlpha()
        {
            return _alpha;
        }

        public float GetBeta()
        {
            return _beta;
        }

        // We initialize the Buffers here
        void Start()
        {
            // Init mesh properties
            _mesh = GetComponent<MeshFilter>().mesh;
            _vertexCount = _mesh.vertices.Length;
            _originalVertexArray = _mesh.vertices;
            _displacedVertexArray = new Vector3[_vertexCount];

            // Init _alpha, _beta
            SetMaterial();
        }

        // Update is called once per frame
        // Here we apply the Push modifiers
        void LateUpdate()
        {
            _grabBrushes = gameObject.GetComponents<PulseGrabCPU>();
            _affineBrushes = gameObject.GetComponents<PulseAffineCPU>();

            if (_grabBrushes.Length + _affineBrushes.Length != 0)
            {
                for (int i = 0; i < _displacedVertexArray.Length; i++)
                {
                    UpdateVertex(i);
                }

                // The actual update of the mesh is here
                _mesh.vertices = _displacedVertexArray;
                _mesh.RecalculateNormals();
            }
        }

        void UpdateVertex(int i)
        {
            _displacedVertexArray[i] = _originalVertexArray[i];
            foreach (PulseGrabCPU pulse in _grabBrushes)
            {
                _displacedVertexArray[i] += pulse.EvalDispRK4(_originalVertexArray[i], Time.time);
            }

            foreach (PulseAffineCPU pulse in _affineBrushes)
            {
                _displacedVertexArray[i] += pulse.EvalDispRK4(_originalVertexArray[i], Time.time);
            }
        }

        public void AddBrush(Mode mode, Type type, Vector3 p, Vector3 f, float eps)
        {
            PulseGrabCPU tmpGrab = gameObject.AddComponent<PulseGrabCPU>();
            tmpGrab.SetBrush(p, f, eps, _maxTime, _minValue, _maxValue);
        }

        public void AddAffineBrush(Mode mode, Type type, Vector3 p, float3x3 f, float eps)
        {
            PulseAffineCPU tmpGrab = gameObject.AddComponent<PulseAffineCPU>();
            tmpGrab.SetBrush(type, p, f, eps, _maxTime, _minValue, _maxValue);
        }
    }
}
