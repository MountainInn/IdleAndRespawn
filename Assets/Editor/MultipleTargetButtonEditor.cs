using UnityEditor;

[CustomEditor(typeof(MultipleTargetGraphicButton))]
public class MultipleTargetButtonEditor : UnityEditor.UI.ButtonEditor
{
    public override void OnInspectorGUI()
    {
        MultipleTargetGraphicButton button = (MultipleTargetGraphicButton)target;

        SerializedProperty graphics = serializedObject.FindProperty("targetGraphics");

        EditorGUILayout.PropertyField(graphics);

        base.OnInspectorGUI();
    }
}
