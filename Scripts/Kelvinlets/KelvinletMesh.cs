using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using NaughtyAttributes;
using Unity.Mathematics;

namespace Kelvinlet
{
    public struct Brush
    {
        Vector3 pos;   // 3 * 4 bytes
        float eps;     // 1 * 4 bytes
        Vector3 force; // 3 * 4 bytes
        float dt;      // 1 * 4 bytes

        public Brush(Vector3 p, Vector3 f, float e, float t)
        {
            pos = p;
            force = f;
            eps = e;
            dt = t;
        }

        public bool IsActive()
        {
            return dt > 0;
        }
    };

    public struct AffineBrush
    {
        Vector3 pos;          // 3  * 4 bytes
        float eps;            // 1  * 4 bytes
        float3x3 force;       // 9 * 4 bytes
        float dt;             // 1  * 4 bytes

        public AffineBrush(Vector3 p, float3x3 f, float e, float t)
        {
            pos = p;
            force = f;
            eps = e;
            dt = t;
        }

        public bool IsActive()
        {
            return dt > 0;
        }
    };

    [RequireComponent(typeof(MeshFilter))]
    public class KelvinletMesh : MonoBehaviour
    {
        #region Material properties
        [Header("Material properties"), InfoBox("Nu is the Poisson ratio that controls compressibility \n" +
            "Mu is the elastic shear modulus indicating material stiffness \n" +
            "Eps is the radial scale of the Kelvinlet input", EInfoBoxType.Normal)]

        [SerializeField, Range(0f, 0.5f)]
        public float nu = 0.30f;

        [SerializeField, Range(0.001f, 200f)]
        public float mu = 60.0f;

        /* [SerializeField, Label("Epsilon"), Range(0f, 20f)]
                public float eps = 1.5f;*/

        // to set with material calibration step
        float _alpha = 0.0f;
        float _beta = 0.0f;
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
        float _maxValue = 100000000f;
        float _minValue = 0.0001f;
        float _maxTime = 4.0f;
        #endregion

        #region Compute shader attributes
        [Header("Shaders")]
        public ComputeShader PulseGrab;
        public ComputeShader PulseAffine;

        ComputeBuffer _originalVertexBuffer, _displacedVertexBuffer;

        int pulse_kernel, pulse_affine_kernel, push_kernel;
        const int kThreadCount = 16;
        int ThreadXGroupCount { get { return _vertexCount / kThreadCount; } }
        #endregion

        #region Brushes arrays
        KelvinletBrush[] _brushes;
        List<Brush> _pulseBrushes;
        List<AffineBrush> _pulseAffineBrushes;
        ComputeBuffer _pulseBrushesBuffer, _pulseAffineBrushesBuffer, _preBrushBuffer;
        Brush _preBrush;
        AffineBrush _preAffineBrush;
        #endregion

        // We initialize the Buffers here
        void Start()
        {
            // Init mesh properties
            _mesh = GetComponent<MeshFilter>().mesh;
            _vertexCount = _mesh.vertices.Length;
            _originalVertexArray = _mesh.vertices;
            _displacedVertexArray = new Vector3[_vertexCount];

            // Init vertex buffers
            _originalVertexBuffer = new ComputeBuffer(_vertexCount, 3 * sizeof(float));
            _originalVertexBuffer.SetData(_originalVertexArray);
            _displacedVertexBuffer = new ComputeBuffer(_vertexCount, 3 * sizeof(float));
            _displacedVertexBuffer.SetData(_displacedVertexArray);

            // Init brush buffers
            // _pulseBrushesBuffer = new ComputeBuffer(10, 8 * sizeof(float));

            // Init _alpha, _beta
            PulseGrab = (ComputeShader)Instantiate(PulseGrab);
            PulseAffine = (ComputeShader)Instantiate(PulseAffine);

            SetMaterial();

            // Set const calibration values
            PulseGrab.SetFloat("MinValue", _minValue);
            PulseGrab.SetFloat("MaxValue", _maxValue);
            PulseGrab.SetFloat("MaxTime", _maxTime);
            PulseAffine.SetFloat("MinValue", _minValue);
            PulseAffine.SetFloat("MaxValue", _maxValue);
            PulseAffine.SetFloat("MaxTime", _maxTime);

            pulse_kernel = PulseGrab.FindKernel("Deform");
            PulseGrab.SetBuffer(pulse_kernel, "originalVertexBuffer", _originalVertexBuffer);
            PulseGrab.SetBuffer(pulse_kernel, "displacedVertexBuffer", _displacedVertexBuffer);

            pulse_affine_kernel = PulseAffine.FindKernel("Deform");
            PulseAffine.SetBuffer(pulse_affine_kernel, "originalVertexBuffer", _originalVertexBuffer);
            PulseAffine.SetBuffer(pulse_affine_kernel, "displacedVertexBuffer", _displacedVertexBuffer);
        }

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

            PulseGrab.SetFloat("alpha", _alpha);
            PulseGrab.SetFloat("beta", _beta);
            PulseAffine.SetFloat("alpha", _alpha);
            PulseAffine.SetFloat("beta", _beta);
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

        // Update is called once per frame
        // Here we apply the Push modifiers
        void Update()
        {
            _displacedVertexBuffer.SetData(_originalVertexArray);
            
            _brushes = gameObject.GetComponents<KelvinletBrush>();
            _pulseBrushes = new List<Brush>();
            _pulseAffineBrushes = new List<AffineBrush>();
            foreach (KelvinletBrush k in _brushes)
            {
                if (k.isAffine())
                {
                    _pulseAffineBrushes.Add(k.GetAffineBrush());
                }
                else
                {
                    _pulseBrushes.Add(k.GetBrush());
                }
            }

            if (_preBrush.IsActive())
            {
                _preBrushBuffer = new ComputeBuffer(1, 8 * sizeof(float));
                Brush[] tmp = new Brush[1];
                tmp[0] = _preBrush;
                _preBrushBuffer.SetData(tmp);
                PulseGrab.SetBuffer(pulse_kernel, "brushes", _preBrushBuffer);

                PulseGrab.Dispatch(pulse_kernel, ThreadXGroupCount, 1, 1);

                _displacedVertexBuffer.GetData(_displacedVertexArray);
                _preBrushBuffer.Release();
            }
        }

        // LateUpdate is called once per frame after Update
        // Here we apply the Pulse modifiers
        void LateUpdate()
        {
            // intégrer un test si la taille a changé ? dans ce cas changer la logique sur le time
            if (_pulseBrushes.Count != 0)
            {
                _pulseBrushesBuffer = new ComputeBuffer(_pulseBrushes.Count, 8 * sizeof(float));
                _pulseBrushesBuffer.SetData(_pulseBrushes);
                PulseGrab.SetBuffer(pulse_kernel, "brushes", _pulseBrushesBuffer);

                PulseGrab.Dispatch(pulse_kernel, ThreadXGroupCount, _pulseBrushes.Count, 1);

                _displacedVertexBuffer.GetData(_displacedVertexArray);
                _pulseBrushesBuffer.Release();
            }

            if (_pulseAffineBrushes.Count != 0)
            {
                _pulseAffineBrushesBuffer = new ComputeBuffer(_pulseAffineBrushes.Count, 14 * sizeof(float));
                _pulseAffineBrushesBuffer.SetData(_pulseAffineBrushes);
                PulseAffine.SetBuffer(pulse_affine_kernel, "brushes", _pulseAffineBrushesBuffer);

                PulseAffine.Dispatch(pulse_affine_kernel, ThreadXGroupCount, _pulseAffineBrushes.Count, 1);

                _displacedVertexBuffer.GetData(_displacedVertexArray);
                _pulseAffineBrushesBuffer.Release();
            }

            if (_brushes.Length != 0)
            {
                // The actual update of the mesh is here
                _mesh.vertices = _displacedVertexArray;
                _mesh.RecalculateNormals();

                // GetComponent<MeshCollider>().sharedMesh = null;
                // GetComponent<MeshCollider>().sharedMesh = _mesh;
            }
        }

        public Vector2 GetAlphaBeta()
        {
            return new Vector2(_alpha, _beta);
        }

        public void AddBrush(Mode mode, Type type, Vector3 p, Vector3 f, float eps)
        {
            KelvinletBrush brush = gameObject.AddComponent<KelvinletBrush>();
            brush.SetBrush(mode, type, p, f, eps, _maxTime);
            _preBrush = new Brush(p, f, eps, -1f);
        }

        public void AddAffineBrush(Mode mode, Type type, Vector3 p, float3x3 f, float eps)
        {
            KelvinletBrush brush = gameObject.AddComponent<KelvinletBrush>();
            brush.SetAffineBrush(mode, type, p, f, eps, _maxTime);
        }

        public void PreBrush(Mode mode, Type type, Vector3 p, Vector3 f, float eps)
        {
            Vector3 force = f * (5f * _alpha * eps * eps);
            _preBrush = new Brush(p, force, eps, 0.01f);
        }

        private void OnDestroy()
        {
            _originalVertexBuffer.Dispose();
            _displacedVertexBuffer.Dispose();

            if (_originalVertexBuffer != null)
            {
                _originalVertexBuffer.Release();
            }

            if (_displacedVertexBuffer != null)
            {
                _displacedVertexBuffer.Release();
            }
        }
    }
}
