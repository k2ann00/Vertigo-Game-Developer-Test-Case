#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MultiScreenshot))]
public class MultiScreenshotEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MultiScreenshot script = (MultiScreenshot)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Capture Screenshots"))
        {
            script.CaptureAll();
        }
    }
}
#endif
