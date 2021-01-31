
// Author: Ricardo Roldán

using UnityEditor;
using UnityEngine;

/// <summary>
/// Clase que modifica el inspector del personaje para modificar la mecánica Tilt
/// </summary>
[ExecuteInEditMode]
[CustomEditor(typeof(PlayerTilt))]
public class PlayerTiltEditor : Editor
{
    int widthSize = 120;

    PlayerTilt playerT;

    SerializedObject so;

    SerializedProperty xValues;
    SerializedProperty zValues;
    

    /// <summary>
    /// Inicializa los arrays de datos en caso de estar vacíos
    /// </summary>
    void InitValues()
    {
        playerT = (PlayerTilt)target;

        if (playerT.xValues == null)
            playerT.xValues = new float[8];

        if (playerT.zValues == null)
            playerT.zValues = new float[8];
    }

    private void OnEnable()
    {
        InitValues();

        so = new SerializedObject(playerT);
        
        xValues = so.FindProperty("xValues");
        zValues = so.FindProperty("zValues");
    }

    public override void OnInspectorGUI()
    {
        // Actualiza los datos siempre que se actualiza el inspector
        so.Update();

        // Dibuja el inspector por defecto (muestra las variables serializadas en PlayerTilt.cs)
        DrawDefaultInspector();

        InitValues();

        GUILayout.Space(10);        
                
        // Primera fila de datos

        GUILayout.BeginHorizontal("box");

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));
                TextBox("Adelante izquierda", 0);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));
                TextBox("Adelante", 1);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));
                TextBox("Adelante derecha", 2);
            GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Segunda fila de datos
        GUILayout.BeginHorizontal("box");

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));
            TextBox("Izquierda", 3);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));

                GUILayout.Box("+Z", GUILayout.ExpandWidth(true)); // Expande la caja hasta el final del ancho

                GUILayout.BeginHorizontal();

                    GUILayout.Label("-X");
                    GUILayout.Label("Jugador");
                    GUILayout.Label("+X");

                GUILayout.EndHorizontal();

                GUILayout.Box("-Z", GUILayout.ExpandWidth(true)); // Expande la caja hasta el final del ancho

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));
            TextBox("Derecha", 4);
            GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Tercera fila de datos
        GUILayout.BeginHorizontal("box");

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));
                TextBox("Atrás izquierda", 5);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));
                TextBox("Atrás", 6);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", GUILayout.MinWidth(widthSize));
                TextBox("Atrás derecha", 7);
            GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        // Se "ensucia" la escena. Si se cierra el proyecto, Unity pregunta si se quieren guardar los cambios
        if (GUI.changed)
            EditorUtility.SetDirty(playerT);

        // Se aplican los cambios a las variables serializadas
        so.ApplyModifiedProperties();
    }

    /// <summary>
    /// Crea dos cajas de float con un título y una etiqueta que acompaña a cada caja
    /// </summary>
    /// <param name="label"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    float TextBox(string label, int val)
    {
        GUILayout.Label(label);

        GUILayout.BeginHorizontal("box", GUILayout.MaxWidth(20));

            GUILayout.Label("X ");

            // Actualiza las variables serializadas en ambos sentidos

            playerT.xValues[val] = EditorGUILayout.FloatField(playerT.xValues[val]);
        
            xValues.GetArrayElementAtIndex(val).floatValue = playerT.xValues[val];

        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal("box", GUILayout.MaxWidth(20));

            GUILayout.Label("Z ");

            // Actualiza las variables serializadas en ambos sentidos

            playerT.zValues[val] = EditorGUILayout.FloatField(playerT.zValues[val]);

            zValues.GetArrayElementAtIndex(val).floatValue = playerT.zValues[val];

        GUILayout.EndHorizontal();

        return val;
    }
}
