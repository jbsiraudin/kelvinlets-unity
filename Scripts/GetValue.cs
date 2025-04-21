using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using Kelvinlet;

public class GetValue : MonoBehaviour
{
    Text text;

    public GameObject Kelvinlet;
    public bool cpu = false;

    [Dropdown("FormatValues")]
    public string format;

    [Dropdown("DisplayedValues")]
    public string value;

    private List<string> FormatValues { get { return new List<string>() { "F0", "F1", "F2" }; } }
    private List<string> DisplayedValues { get { return new List<string>() { "Alpha", "Beta", "Epsilon", "Force", "Offset" }; } }

    private KelvinletMesh KelvinletMesh;
    private KelvinletMeshCPU KelvinletMeshCPU;
    private KelvinletMouseInput KelvinletMouseInput;
    private KelvinletMouseInputCPU KelvinletMouseInputCPU;

    void Start()
    {
        text = GetComponent<Text>();

        if (value == "Alpha" || value == "Beta" && cpu)
        {
            KelvinletMeshCPU = Kelvinlet.GetComponent<KelvinletMeshCPU>();
        }
        if (value == "Alpha" || value == "Beta" && !cpu)
        {
            KelvinletMesh = Kelvinlet.GetComponent<KelvinletMesh>();
        }
        if (value == "Force" || value == "Epsilon" || value == "Offset" && cpu)
        {
            KelvinletMouseInputCPU = Kelvinlet.GetComponent<KelvinletMouseInputCPU>();
        }
        if (value == "Force" || value == "Epsilon" || value == "Offset" && !cpu)
        {
            KelvinletMouseInput = Kelvinlet.GetComponent<KelvinletMouseInput>();
        }
    }

    public void valueUpdate()
    {
        switch (value)
        {
            case "Alpha":
                if (cpu)
                {
                    text.text = KelvinletMeshCPU.GetAlpha().ToString(format, CultureInfo.InvariantCulture);
                }
                else
                {
                    text.text = KelvinletMesh.GetAlpha().ToString(format, CultureInfo.InvariantCulture);
                }
                break;
            case "Beta":
                if (cpu)
                {
                    text.text = KelvinletMeshCPU.GetBeta().ToString(format, CultureInfo.InvariantCulture);
                }
                else
                {
                    text.text = KelvinletMesh.GetBeta().ToString(format, CultureInfo.InvariantCulture);
                }
                break;
            case "Epsilon":
                if (cpu)
                {
                    text.text = KelvinletMouseInputCPU.GetEpsilon().ToString(format, CultureInfo.InvariantCulture);
                }
                else
                {
                    text.text = KelvinletMouseInput.GetEpsilon().ToString(format, CultureInfo.InvariantCulture);
                }
                break;
            case "Force":
                if (cpu)
                {
                    text.text = KelvinletMouseInputCPU.GetForce().ToString(format, CultureInfo.InvariantCulture);
                }
                else
                {
                    text.text = KelvinletMouseInput.GetForce().ToString(format, CultureInfo.InvariantCulture);
                }
                break;
            case "Offset":
                if (cpu)
                {
                    text.text = KelvinletMouseInputCPU.GetOffset().ToString(format, CultureInfo.InvariantCulture);
                }
                else
                {
                    text.text = KelvinletMouseInput.GetOffset().ToString(format, CultureInfo.InvariantCulture);
                }
                break;
            default:
                if (cpu)
                {
                    text.text = KelvinletMeshCPU.GetAlpha().ToString(format, CultureInfo.InvariantCulture);
                }
                else
                {
                    text.text = KelvinletMesh.GetAlpha().ToString(format, CultureInfo.InvariantCulture);
                }
                break;
        }
    }
}
