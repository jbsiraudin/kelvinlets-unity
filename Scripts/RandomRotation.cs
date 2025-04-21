using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RandomRotation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public GameObject ToRotate;
    public Toggle toggle;

    public float rotation = 0.0f;
    // Start is called before the first frame update

    float refTime = 0.0f;
    bool selected = false;
    bool hovered = false;

    // Update is called once per frame
    void Update()
    {
        float t = Time.time - refTime;

        if (selected)
        {
            if (t > 6.0f)
            {
                refTime += 6.0f;
                rotation = Random.Range(-360.0f, 360.0f);
                ToRotate.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            }
        }
        else
        {
            if (hovered)
            {
                if (t > 1.0f)
                {
                    refTime += 1.0f;
                    rotation = Random.Range(-360.0f, 360.0f);
                    ToRotate.transform.localRotation = Quaternion.Euler(0, 0, rotation);
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        refTime = Time.time;
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (toggle.isOn)
        {
            refTime = Time.time;
            selected = true;
        }
        else
        {
            selected = false;
        }
    }
}
