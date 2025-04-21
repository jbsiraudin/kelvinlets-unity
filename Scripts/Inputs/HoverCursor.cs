using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCursor : MonoBehaviour
{
    public Texture2D defaultTexture;
    public Texture2D targetTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        Cursor.SetCursor(targetTexture, hotSpot, cursorMode);
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(defaultTexture, hotSpot, cursorMode);
    }
}
