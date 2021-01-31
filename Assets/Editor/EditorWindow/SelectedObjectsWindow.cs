
// Author: Ricardo Roldán

using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Recuenta el número de objetos seleccionados y la jerarquía de los hijos
/// </summary>
public class SelectedObjectsWindow : EditorWindow
{
    bool disabled;
    int selectionCount;
    int selectionAndChildrenCount;
    
    List<Transform> prevChildren = new List<Transform>();

    [MenuItem("Agama/Selection count")]
    public static void CountSelected()
    {
        SelectedObjectsWindow window = GetWindow<SelectedObjectsWindow>("Selection count");
        window.minSize = new Vector2(100, 60);
    }

    private void OnSelectionChange()
    {
        GameObject[] selected = Selection.gameObjects;

        selectionCount = selected.Length;
        if (selectionCount == 0)
        {
            disabled = true;
            selectionAndChildrenCount = 0;
        }
        else
            disabled = false;

        selectionAndChildrenCount = 0;
        foreach (GameObject o in selected)
        {
            Transform tr = o.GetComponent<Transform>();
            if (tr.childCount > 0)
            {
                prevChildren = tr.GetComponentsInChildren<Transform>().ToList();
                selectionAndChildrenCount += prevChildren.Count;
            }
            else
            {
                ++selectionAndChildrenCount;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        GUILayout.Label("Shows number of objects selected");

        GUILayout.Space(15);
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.HelpBox("Selected objects:                       ", MessageType.None);
        EditorGUILayout.HelpBox(selectionCount.ToString(), MessageType.None);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.HelpBox("Selected objects and children: ", MessageType.None);
        EditorGUILayout.HelpBox(selectionAndChildrenCount.ToString(), MessageType.None);
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        if (disabled)
            EditorGUILayout.HelpBox("No objects selected", MessageType.Info);

        GUILayout.Space(25);

        Repaint();
    }
}