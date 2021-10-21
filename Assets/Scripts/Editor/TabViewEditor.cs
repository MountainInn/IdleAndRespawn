using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// [CustomEditor(typeof(TabView))]
public class TabViewEditor : Editor
{
    TabView tabView;
    [SerializeField] Transform viewport;

    SerializedProperty tabNames;
    List<Transform> tabs;
    List<Button> buttons;


    void OnEnable()
    {
        tabView = (TabView)target;

        tabNames = serializedObject.FindProperty("tabNames");

        tabs = new List<Transform>();
        buttons = new List<Button>();

        viewport ??= EnsurePanelExist("Viewport", tabView.transform);
    }

    Transform CreatePanel(string name, Transform parent)
    {
        var panel = new GameObject(name, typeof(RectTransform)).transform;
        panel.SetParent(parent);
        panel.localScale = Vector3.one;

        return panel;
    }

    override public void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(tabNames);

        // for (int i =0; i < tabNames.arraySize; i++)
        // {
        //     EditorGUILayout.PropertyField(tabNames.GetArrayElementAtIndex(i));
        // }

        if (EditorGUI.EndChangeCheck())
        {
            if (tabs.Count != tabNames.arraySize)
                tabs.FitToSize(tabNames.arraySize);

            if (buttons.Count != tabNames.arraySize)
                buttons.FitToSize(tabNames.arraySize);


            for (int i = 0; i < tabNames.arraySize; i++)
            {
                string name = tabNames.GetArrayElementAtIndex(i).stringValue;

                tabs[i] ??= EnsurePanelExist(name, viewport);

                tabs[i].name = name;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    Transform EnsurePanelExist(string name, Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name.ToLower() == name.ToLower())
                return child;
        }

        return CreatePanel(name, parent);
    }


    void OnSceneGUI()
    {
    }
}
