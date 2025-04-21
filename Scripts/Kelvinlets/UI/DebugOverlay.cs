using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugOverlay : MonoBehaviour
{
    const float fpsMeasurePeriod = 0.5f;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    string display = "{0} FPS";
    public Text m_Text;

    private void Awake()
    {
        m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;

        GameObject[] gOs = GameObject.FindGameObjectsWithTag("Deformable");
        int total = 0;
        for (int i = 0; i < gOs.Length; i++)
        {
            total += gOs[i].GetComponent<MeshFilter>().sharedMesh.vertices.Length;
        }
        string tmp;

        if (gOs.Length > 1)
        {
            tmp = "\n {0} meshes, {1} vertices";
        }
        else
        {
            tmp = "\n {0} mesh, {1} vertices";
        }
        
        tmp = string.Format(tmp, gOs.Length, total);
        display += tmp;
    }

    private void Update()
    {
        // measure average frames per second
        m_FpsAccumulator++;
        GameObject gO = GameObject.FindGameObjectWithTag("Deformable");
        Vector2 ab = gO.GetComponent<Kelvinlet.KelvinletMesh>().GetAlphaBeta();
        string tmp = string.Format("\n alpha = {0} \n beta = {1}", ab.x, ab.y);

        if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        {
            m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;
            m_Text.text = string.Format(display, m_CurrentFps) + tmp;
        }
    }
}
