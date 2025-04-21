using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class ShowValue : MonoBehaviour
{
    Text text;

    [Dropdown("FormatValues")]
    public string format;

    private List<string> FormatValues { get { return new List<string>() { "F0", "F1", "F2" }; } }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();    
    }

    public void valueUpdate(float value)
    {
        text.text = value.ToString(format, CultureInfo.InvariantCulture);
    }
}
