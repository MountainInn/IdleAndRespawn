using UnityEditor;

[CustomEditor(typeof(DoubleEndedProgress))]
public class DoubleEndedProgressEditor : Editor
{
    DoubleEndedProgress prog;

    float fillAmount = 1;

    void Awake()
    {
        prog = (DoubleEndedProgress )target;

        prog.Init();
    }

    override public void OnInspectorGUI()
    {
        fillAmount = EditorGUILayout.Slider(fillAmount, 0, 1f);

        prog.SetValue(fillAmount);

        
    }
}
