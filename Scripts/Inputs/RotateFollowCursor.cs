using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateFollowCursor : MonoBehaviour
{
    public float calibration = 10.0f;

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = calibration;

        Vector3 pos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector3 forward = pos - transform.position;

        transform.rotation = Quaternion.LookRotation(forward);
    }
}
