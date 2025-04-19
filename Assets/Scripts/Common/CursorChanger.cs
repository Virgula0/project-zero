using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    [SerializeField] private Texture2D customCursor; // Assign in Inspector
    [SerializeField] private Vector2 hotspot = new Vector2(16, 16);
    private CursorMode cursorMode = CursorMode.Auto;

    public void ChangeToTargetCursor(){
        Cursor.SetCursor(customCursor, hotspot, cursorMode);
    }

    public void ChangeToDefaultCursor()
    {
        // Reset to the default cursor
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    // helper that auto-centers the hotspot when a new texture is assigned
    void OnValidate()
    {
        if (customCursor != null)
        {
            hotspot = new Vector2(customCursor.width / 2f, customCursor.height / 2f);
        }
    }

}
