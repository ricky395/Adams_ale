
// Author: Ricardo Roldán

using UnityEditor;
using UnityEngine;

/// <summary>
/// Ventana de Editor que aplica un material a un conjunto de objetos seleccionados
/// </summary>
public class MaterialApplyWindow : EditorWindow
{
    bool simpleDisabled;

    public Object material1;
    
    [MenuItem("Agama/Material assigner")]
    public static void AssignMaterial()
    {
        MaterialApplyWindow window = GetWindow<MaterialApplyWindow>("Material assigner");
        window.minSize = new Vector2(200, 245);
    }

    private void OnSelectionChange()
    {
        int selectionCount = Selection.gameObjects.Length;

        if (selectionCount == 0)
            simpleDisabled = true;
        else
            simpleDisabled = false;
    }

    private void OnGUI()
    {
        
        GUILayout.Space(10);

        GUILayout.Label("Adds chosen material to:");

        GUILayout.Space(15);

        GUILayout.Label("Selected objects");
        material1 = EditorGUILayout.ObjectField(material1, typeof(Material), true);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(simpleDisabled);
        if (GUILayout.Button("Assign to selected", GUILayout.MaxWidth(150)))
        {
            foreach (GameObject GO in Selection.gameObjects)
            {
                Renderer rend = GO.GetComponent<Renderer>();
                if (rend == null)
                    GO.AddComponent(typeof(Material));
                else
                    rend.material = material1 as Material;
            }
        }

        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();

        Repaint();
    }
}