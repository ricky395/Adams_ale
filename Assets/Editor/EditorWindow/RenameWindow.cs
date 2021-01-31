
// Author: Ricardo Roldán

using UnityEngine;
using UnityEditor;

/// <summary>
/// Ventana de editor que asigna un nombre y un prefijo a los objetos seleccionados
/// </summary>
public class RenameWindow : EditorWindow
{
    public string prefix;
    public string objName;
    public Object[] objects;

    [MenuItem("Agama/Objects Renamer")]
    public static void Renamer()
    {
        RenameWindow window = GetWindow<RenameWindow>("Objects Renamer");
        window.minSize = new Vector2(200, 245);
    }


    private void OnGUI()
    {

        GUILayout.Space(10);

        GUILayout.Label("Rename selected objects");

        GUILayout.Space(15);


        GUILayout.Label("Prefix");
        prefix = EditorGUILayout.TextField(prefix);

        GUILayout.Label("Name");
        objName = EditorGUILayout.TextField(objName);
        
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Assign to selected", GUILayout.MaxWidth(150)))
        {
            foreach (Object o in Selection.objects)
            {
                string oldName = o.name;
                string newName = "";

                // Reemplaza nombres variados de texturas con un sufijo común

                if (oldName.Contains("Albedo") || oldName.Contains("albedo") || oldName.Contains("Base_Color") || oldName.Contains("base_Color") || oldName.Contains("basecolor"))
                {
                    newName = "_Albedo";
                }

                else if (oldName.Contains("Metallic") || oldName.Contains("metallic"))
                {
                    newName = "_Metallic";
                }

                else if(oldName.Contains("Normal") || oldName.Contains("normal"))
                {
                    newName = "_Normal";
                }

                else if(oldName.Contains("Ambient Occlusion") || oldName.Contains("AO") || oldName.Contains("ao") || oldName.Contains("Ao") || oldName.Contains("ambient occlusion") || oldName.Contains("ambient_occlusion"))
                {
                    newName = "_AO";
                }

                else if (oldName.Contains("Emission") || oldName.Contains("emission"))
                {
                    newName = "_Emission";
                }
                string assetPath = AssetDatabase.GetAssetPath(o);
                AssetDatabase.RenameAsset(assetPath, prefix + "_" + objName + newName);

                o.name = prefix + objName;
            }
        }


        GUILayout.EndHorizontal();

        GUILayout.Space(25);

        Repaint();
    }
}
