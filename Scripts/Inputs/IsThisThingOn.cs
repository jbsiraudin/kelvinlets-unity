using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kelvinlet;

public class IsThisThingOn : MonoBehaviour
{
    public Toggle toggle;
    public Type targetType;

    public void updateOn(Type type)
    {
        if (type == targetType)
        {
            toggle.isOn = true;
        }
        else
        {
            toggle.isOn = false;
        }
    }
}
