using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CursorChanger))]
public class CursorChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CursorChanger script = (CursorChanger)target;

        if (script != null && script.enabled && script.gameObject.activeInHierarchy)
        {
            Texture2D cursorTexture = serializedObject.FindProperty("customCursor").objectReferenceValue as Texture2D;

            if (cursorTexture != null)
            {
                GUILayout.Label("Cursor Preview:");
                Rect previewRect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(previewRect, cursorTexture);
            }
        }
    }
}
