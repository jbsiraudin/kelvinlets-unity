using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kelvinlet
{
    struct uniKel
    {
        Vector3 pos;
        float eps;

        Vector3 grab;
        float scale;
        float twist;
    }

    public class KelvinletPath : MonoBehaviour
    {
        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
