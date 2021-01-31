
// Author: Ricardo Roldán

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Posicionamiento de objetos del barco
/// </summary>
public class ObjectsPositioning : MonoBehaviour
{
    /// <summary>
    /// Offset de posición de la ventana respecto a la tile en la que se encuentra
    /// </summary>
    [SerializeField]
    float windowHeightOffset = 1.6f,
          windowToWallOffset = 0.8f;

    List<Transform> assetsFill;

    // CAMBIAR
    List<Transform> assetsSmallBox;
    List<Transform> assetsLongBox;
    List<Transform> assetsBigBox;
    List<Transform> assetsJar;

    /// <summary>
    /// Lista de objetos del barco
    /// </summary>
    List<Transform> chosenAssets;

    /// <summary>
    /// Total de tiles del barco
    /// </summary>
    int widthCount, lengthCount;

    /// <summary>
    /// Anchura y largo del barco
    /// </summary>
    int bodyWidth, bodyLength;

    /// <summary>
    /// Referencia al objeto padre que desplaza la jerarquía de objetos del barco
    /// </summary>
    GameObject commingShip;

    List<Transform> floors;
    List<Transform> walls;
    List<Transform> rotated_walls;
    List<Transform> final_walls;
    List<Transform> thin_column_walls;
    List<Transform> thick_column_walls;
    List<Transform> corners;
    List<Transform> cross_beams;
    List<Transform> width_beams;
    List<Transform> length_beams;
    List<Transform> columns;
    List<Transform> mast;

    // Large, Small, Neutral, Double
    char[,] assetsTemplate = new char[,]
    {
        { 'N', 'N', 'L', 'L', 'L', 'L', 'L', 'N', 'N'},
        { 'N', 'N', 'S', 'S', 'S', 'S', 'S', 'N', 'N'},
        { 'L', 'S', ' ', ' ', ' ', ' ', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', 'S', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', 'S', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', ' ', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', 'D', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', ' ', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', ' ', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', 'D', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', ' ', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', ' ', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', 'S', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', 'S', 'S', 'S', ' ', 'S', 'L'},
        { 'L', 'S', ' ', ' ', ' ', ' ', ' ', 'S', 'L'},
        { 'N', 'N', ' ', ' ', ' ', ' ', ' ', 'N', 'N'},
        { 'N', 'N', ' ', ' ', ' ', ' ', ' ', 'N', 'N'},
    };

    int[] lengthPriority = { 3, 2, 4, 1, 0, 4, 4, 4, 4, 4, 4, 4, 0, 1, 4, 2, 3 };
    int[] widthPriority = { 1, 0, 4, 4, 4, 4, 4, 0, 1 };

    int maxLength,
        maxWidth;

    int l_center,
        w_center;

    char[,] currentAssetsTemplate;

    List<int> removedRows = new List<int>();
    List<int> removedColumns = new List<int>();

    ShipsManager shipsManager;

    /// Define el tipo de objeto y su transform de una determinada posición del barco
    struct Structure
    {
        char type;
        float y_rotation;
        Transform obj;

        public Structure(char _type, float _y_rotation, Transform o = null)
        {
            type = _type;
            y_rotation = _y_rotation;
            obj = o;
        }

        public char Type
        {
            get { return type; }
        }

        public float Y_rotation
        {
            get { return y_rotation; }
        }

        public Transform Obj
        {
            get { return obj; }
            set { obj = value; }
        }
    }

    Structure[,] structure;

    public static ObjectsPositioning Instance { get; private set; }


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
    
    /// <summary>
    /// Se inicializan todos los valores que diferencian a cada barco
    /// </summary>
    /// <param name="_chosenAssets"></param>
    /// <param name="_commingShip"></param>
    /// <param name="_widthCount"></param>
    /// <param name="_lengthCount"></param>
    /// <param name="_bodyWidth"></param>
    /// <param name="_bodyLength"></param>
    public void Initialize(List<Transform> _chosenAssets, GameObject _commingShip, int _widthCount, int _lengthCount, int _bodyWidth, int _bodyLength)
    {
        chosenAssets = _chosenAssets;
        commingShip = _commingShip;

        widthCount = _widthCount;
        lengthCount = _lengthCount;

        bodyWidth = _bodyWidth;
        bodyLength = _bodyLength;

        maxWidth = widthPriority.Length;
        maxLength = lengthPriority.Length;

        shipsManager = ShipsManager.Instance;
        assetsFill = new List<Transform>();

        assetsJar      = new List<Transform>();
        assetsBigBox   = new List<Transform>();
        assetsLongBox  = new List<Transform>();
        assetsSmallBox = new List<Transform>();

        l_center = Mathf.FloorToInt(lengthCount * .5f);
        w_center = (int)((widthCount - 1) * .5f);
    }

    /// <summary>
    /// Activa un objeto y lo añade a la lista de objetos del barco
    /// </summary>
    /// <param name="part"></param>
    void SetActiveAndRegister(Transform part)
    {
        chosenAssets.Add(part);
        part.gameObject.SetActive(true);
    }

    /// <summary>
    /// Define la estructura de la bodega del barco
    /// </summary>
    public void WarehousePosition()
    {
        structure = new Structure[lengthCount, widthCount];

        int total_basic_walls = 0, total_corners = 0, total_floors = 0,
            total_cross_beams = 0, total_vert_beams = 0, total_hor_beams = 0,
            window_count = 0, total_l_walls = 0, total_mid_columns = 0,
            total_final_walls = 0, total_rotated_walls = 0, total_L_walls = 4;

        int l_iterator = 0, w_iterator = 0;


        for (int i = l_center; i < lengthCount; ++i)
        {
            // Si el ancho empieza con pasillo o con viga
            if (w_center % 2 == 0)
                w_iterator = 0;
            else
                w_iterator = 1;

            for (int j = 0; j < widthCount; ++j)
            {
                // Filas de ventana o
                if (l_iterator % 2 == 0)
                {
                    // Principio de fila
                    if (j == 0)
                    {
                        // Si no es la última columna
                        if (i != lengthCount - 1) //////
                        {
                            ++total_basic_walls;
                            ++window_count;
                            structure[i, j] = new Structure('w', 90);
                        }

                        // Si es la última columna
                        else
                        {
                            ++total_corners;
                            structure[i, j] = new Structure('c', 0);
                        }
                    }

                    // Final de fila
                    else if (j == widthCount - 1)
                    {
                        // Si no es la última columna
                        if (i != lengthCount - 1) //////
                        {
                            ++total_basic_walls;
                            ++window_count;
                            structure[i, j] = new Structure('w', -90);
                        }

                        // Si es la última columna
                        else
                        {
                            ++total_corners;
                            structure[i, j] = new Structure('c', -90);
                        }
                    }

                    // Intermedio de fila
                    else
                    {
                        // Columnas sin viga
                        if (w_iterator % 2 == 0)
                        {
                            // Si no es la última columna
                            if (i != lengthCount - 1) //////
                            {
                                ++total_floors;
                                structure[i, j] = new Structure('f', 90);
                            }

                            // Si es la última columna
                            else
                            {
                                ++total_rotated_walls;
                                structure[i, j] = new Structure('W', 0);
                            }
                        }

                        // Columnas con viga
                        else
                        {
                            // Si no es la última columna
                            if (i != lengthCount - 1) //////
                            {
                                ++total_vert_beams;
                                structure[i, j] = new Structure('v', 90);
                            }

                            // Si es la última columna
                            else
                            {
                                ++total_final_walls;
                                structure[i, j] = new Structure('p', 0);
                            }
                        }
                    }
                }


                // Filas de pared l
                else
                {
                    // Principio de fila
                    if (j == 0)
                    {
                        // Si no es la última columna
                        if (i != lengthCount - 1) //////
                        {
                            ++total_l_walls;
                            structure[i, j] = new Structure('l', 90);
                        }

                        // Si es la última columna
                        else
                        {
                            ++total_corners;
                            structure[i, j] = new Structure('c', 0);
                        }

                        // Si es la fila de vigas a lo ancho
                        if (i == l_center + 3 && l_center + 3 != lengthCount - 1) //////
                        {
                            ++total_L_walls;
                            structure[i, j] = new Structure('L', 90);
                        }
                    }

                    // Final de fila
                    else if (j == widthCount - 1)
                    {
                        // Si no es la última columna
                        if (i != lengthCount - 1) //////
                        {
                            ++total_l_walls;
                            structure[i, j] = new Structure('l', -90);
                        }

                        // Si es la última columna
                        else
                        {
                            ++total_corners;
                            structure[i, j] = new Structure('c', -90);
                        }

                        // Si es la fila de vigas a lo ancho
                        if (i == l_center + 3 && l_center + 3 != lengthCount - 1) //////
                        {
                            ++total_L_walls;
                            structure[i, j] = new Structure('L', -90);
                        }
                    }

                    // Intermedio de fila
                    else
                    {
                        // Columnas sin viga
                        if (w_iterator % 2 == 0)
                        {
                            // Si es la última columna
                            if (i == lengthCount - 1) //////
                            {
                                ++total_rotated_walls;
                                structure[i, j] = new Structure('W', 0);
                            }

                            // Intermedias
                            else
                            {
                                ++total_floors;
                                structure[i, j] = new Structure('f', 90);
                            }

                            // Si es la fila de vigas a lo ancho
                            if (i == l_center + 3 && l_center + 3 != lengthCount - 1) //////
                            {
                                // Centro de vigas a lo ancho (Column)
                                if (j == w_center)
                                {
                                    ++total_mid_columns;
                                    structure[i, j] = new Structure('T', 90);
                                }
                                else
                                {
                                    ++total_hor_beams;
                                    structure[i, j] = new Structure('h', 90);
                                }
                            }
                        }

                        // Columnas con viga
                        else
                        {
                            // Si es la última columna
                            if (i == lengthCount - 1) //////
                            {
                                ++total_final_walls;
                                structure[i, j] = new Structure('p', 0);
                            }

                            // Intermedias
                            else
                            {
                                ++total_vert_beams;
                                structure[i, j] = new Structure('v', 90);
                            }

                            // Si es la fila de vigas a lo ancho
                            if (i == l_center + 3 && l_center + 3 != lengthCount - 1) //////
                            {
                                ++total_cross_beams;
                                structure[i, j] = new Structure('x', 90);
                            }
                        }
                    }
                }

                ++w_iterator;
            }

            ++l_iterator;
        }

        l_iterator = 1;

        for (int i = l_center - 1; i >= 0; --i)
        {
            // Si el ancho empieza con pasillo o viga
            if (w_center % 2 == 0)
                w_iterator = 0;
            else
                w_iterator = 1;

            for (int j = 0; j < widthCount; ++j)
            {
                // Filas de ventana o
                if (l_iterator % 2 == 0)
                {
                    // Principio de fila
                    if (j == 0)
                    {
                        // Si no es la última columna
                        if (i != 0) //////
                        {
                            ++total_basic_walls;
                            ++window_count;
                            structure[i, j] = new Structure('w', 90);
                        }

                        // Si es la última columna
                        else
                        {
                            ++total_corners;
                            structure[i, j] = new Structure('c', 90);
                        }
                    }

                    // Final de fila
                    else if (j == widthCount - 1)
                    {
                        // Si no es la última columna
                        if (i != 0) //////
                        {
                            ++total_basic_walls;
                            ++window_count;
                            structure[i, j] = new Structure('w', -90);
                        }

                        // Si es la última columna
                        else
                        {
                            ++total_corners;
                            structure[i, j] = new Structure('c', 180);
                        }
                    }

                    // Intermedio de fila
                    else
                    {
                        // Columnas sin viga
                        if (w_iterator % 2 == 0)
                        {
                            // Si no es la última columna
                            if (i != 0) //////
                            {
                                ++total_floors;
                                structure[i, j] = new Structure('f', 90);
                            }

                            // Si es la última columna
                            else
                            {
                                ++total_rotated_walls;
                                structure[i, j] = new Structure('W', 180);
                            }
                        }

                        // Columnas con viga
                        else
                        {
                            // Si no es la última columna
                            if (i != 0) //////
                            {
                                ++total_vert_beams;
                                structure[i, j] = new Structure('v', 90);
                            }

                            // Si es la última columna
                            else
                            {
                                ++total_final_walls;
                                structure[i, j] = new Structure('p', 180);
                            }
                        }
                    }
                }


                // Filas de pared l
                else
                {
                    // Principio de fila
                    if (j == 0)
                    {
                        // Si no es la última columna
                        if (i != 0) //////
                        {
                            ++total_l_walls;
                            structure[i, j] = new Structure('l', 90);
                        }

                        // Si es la última columna
                        else
                        {
                            ++total_corners;
                            structure[i, j] = new Structure('c', 90);
                        }

                        // Si es la fila de vigas a lo ancho
                        if (i == l_center - 3 && l_center - 3 != 0) //////
                        {
                            ++total_L_walls;
                            structure[i, j] = new Structure('L', 90);
                        }
                    }

                    // Final de fila
                    else if (j == widthCount - 1)
                    {
                        // Si no es la última columna
                        if (i != 0) //////
                        {
                            ++total_l_walls;
                            structure[i, j] = new Structure('l', -90);
                        }

                        // Si es la última columna
                        else
                        {
                            ++total_corners;
                            structure[i, j] = new Structure('c', -180);
                        }

                        // Si es la fila de vigas a lo ancho
                        if (i == l_center - 3 && l_center - 3 != 0) //////
                        {
                            ++total_L_walls;
                            structure[i, j] = new Structure('L', -90);
                        }
                    }

                    // Intermedio de fila
                    else
                    {
                        // Columnas sin viga
                        if (w_iterator % 2 == 0)
                        {
                            // Si es la última columna
                            if (i == 0) //////
                            {
                                ++total_rotated_walls;
                                structure[i, j] = new Structure('W', 180);
                            }

                            // Intermedias
                            else
                            {
                                ++total_floors;
                                structure[i, j] = new Structure('f', 90);
                            }

                            // Si es la fila de vigas a lo ancho
                            if (i == l_center - 3 && l_center - 3 != 0) //////
                            {
                                // Centro de vigas a lo ancho (Column)
                                if (j == w_center)
                                {
                                    ++total_mid_columns;
                                    structure[i, j] = new Structure('T', 90);
                                }
                                else
                                {
                                    ++total_hor_beams;
                                    structure[i, j] = new Structure('h', 90);
                                }
                            }
                        }

                        // Columnas con viga
                        else
                        {
                            // Si es la última columna
                            if (i == 0) //////
                            {
                                ++total_final_walls;
                                structure[i, j] = new Structure('p', 180);
                            }

                            // Intermedias
                            else
                            {
                                ++total_vert_beams;
                                structure[i, j] = new Structure('v', 90);
                            }

                            // Si es la fila de vigas a lo ancho
                            if (i == l_center - 3 && l_center - 3 != 0) //////
                            {
                                ++total_cross_beams;
                                structure[i, j] = new Structure('x', 90);
                            }
                        }
                    }
                }

                ++w_iterator;
            }

            ++l_iterator;
        }

        floors = shipsManager.SelectAssets("Ship Base Floor", total_floors, true);
        rotated_walls = shipsManager.SelectAssets("Ship Base Wall 90", total_rotated_walls, true);
        walls = shipsManager.SelectAssets("Ship Base Wall", total_basic_walls, true);
        thin_column_walls = shipsManager.SelectAssets("Ship Thin Wall", total_l_walls, true);
        thick_column_walls = shipsManager.SelectAssets("Ship Thick Wall", total_L_walls, true);
        corners = shipsManager.SelectAssets("Ship Corner", total_corners, true);
        final_walls = shipsManager.SelectAssets("Ship Final Wall", total_final_walls, true);
        cross_beams = shipsManager.SelectAssets("Ship Cross Beams", total_cross_beams, true);
        width_beams = shipsManager.SelectAssets("Ship Width Beams", total_vert_beams, true);
        length_beams = shipsManager.SelectAssets("Ship Length Beams", total_hor_beams, true);
        columns = shipsManager.SelectAssets("Ship Column", total_mid_columns, true);
        mast = shipsManager.SelectAssets("Mast", 1, true);

        WareHouseIteration();
    }

    /// <summary>
    /// Según la estructura definida en WarehousePosition, coloca las tiles de estructura
    /// </summary>
    void WareHouseIteration()
    {
        Vector3 parentPos = commingShip.transform.position;

        int l_index = 0, L_index = 0, w_index = 0, T_index = 0, p_index = 0,
            c_index = 0, f_index = 0, x_index = 0, h_index = 0, v_index = 0,
            W_index = 0;

        for (int i = 0; i < lengthCount; ++i)
        {
            for (int j = 0; j < widthCount; ++j)
            {
                if (i == l_center && j == w_center)
                {
                    shipsManager.SetParentAndPos(mast[0],
                                                 commingShip.transform,
                                                 new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                 Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                    SetActiveAndRegister(mast[0]);
                }

                switch (structure[i, j].Type)
                {
                    case 'l':
                        structure[i, j].Obj = thin_column_walls[l_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'L':
                        structure[i, j].Obj = thick_column_walls[L_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'w':
                        structure[i, j].Obj = walls[w_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'W':
                        structure[i, j].Obj = rotated_walls[W_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'T':
                        structure[i, j].Obj = columns[T_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'c':
                        structure[i, j].Obj = corners[c_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'f':
                        structure[i, j].Obj = floors[f_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'x':
                        structure[i, j].Obj = cross_beams[x_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'h':
                        structure[i, j].Obj = length_beams[h_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'v':
                        structure[i, j].Obj = width_beams[v_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;

                    case 'p':
                        structure[i, j].Obj = final_walls[p_index++];
                        shipsManager.SetParentAndPos(structure[i, j].Obj,
                                                              commingShip.transform,
                                                              new Vector3(parentPos.x + j * bodyWidth, parentPos.y + .5f, parentPos.z - i * bodyLength),
                                                              Quaternion.Euler(new Vector3(0, structure[i, j].Y_rotation, 0))); ///////
                        SetActiveAndRegister(structure[i, j].Obj);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Posiciona el casco del barco
    /// </summary>
    /// <param name="bodyWidth"></param>
    /// <param name="bodyLength"></param>
    /// <param name="widthCount"></param>
    /// <param name="lengthCount"></param>
    /// <returns></returns>
    public Transform HullPosition()
    {
        int _lengthCount = lengthCount + 2;

        List<Transform> hullTr = shipsManager.SelectAssets("Hull", 1, true);

        Vector3 hullPosition;
        hullPosition = new Vector3(commingShip.transform.position.x + (widthCount * .5f * bodyWidth) - bodyWidth * .5f, // Ship sides direction
                                   commingShip.transform.position.y - 2.9f, // Height direction
                                   commingShip.transform.position.z - _lengthCount * .1f); // Ship forward direction

        shipsManager.SetParentAndPos(hullTr[0],
                                     commingShip.transform,
                                     hullPosition,
                                     Quaternion.identity,
                                     new Vector3(hullTr[0].localScale.x * (bodyWidth * widthCount + 2),
                                                 hullTr[0].localScale.y + 6.2f,
                                                 hullTr[0].localScale.z * (bodyLength * _lengthCount + _lengthCount * .5f)));

        SetActiveAndRegister(hullTr[0]);

        return hullTr[0];
    }

    /// <summary>
    /// Posiciona las velas del barco
    /// </summary>
    /// <param name="selectedHull"></param>
    public void SailPosition(Transform selectedHull)
    {
        Transform[] partsPosition = selectedHull.GetComponentsInChildren<Transform>();

        int backCount, mainCount, wingCount;
        backCount = mainCount = wingCount = 0;

        foreach (Transform tr in partsPosition)
        {
            if (tr.name.Contains("Back"))
                ++backCount;
            if (tr.name.Contains("Main"))
                ++mainCount;
            if (tr.name.Contains("Wing"))
                ++wingCount;
        }

        List<Transform> partsTr = shipsManager.SelectAssets("Back Sail", backCount, true);
        partsTr.AddRange(shipsManager.SelectAssets("Main Sail", mainCount, true));
        partsTr.AddRange(shipsManager.SelectAssets("Wing Sail", wingCount, true));
        int count = partsPosition.Length - 1;

        for (int i = 0; i < count; ++i)
        {
            shipsManager.SetParentAndPos(partsTr[i],
                                         commingShip.transform,
                                         partsPosition[i + 1].position,
                                         Quaternion.Euler(new Vector3(partsPosition[i + 1].localRotation.eulerAngles.x, partsPosition[i + 1].localRotation.eulerAngles.z, -partsPosition[i + 1].localRotation.eulerAngles.y)));

            SetActiveAndRegister(partsTr[i]);
        }
    }

    /// <summary>
    /// Posiciona la escalera a la bodega
    /// </summary>
    public void StairsPosition()
    {
        int stairIndex = 0;
        int stairsPosition = 0;

        // Dependiendo del tamaño del barco, la escalera tendrá más o menos posiciones posibles
        if (lengthCount < 13)
            stairIndex = UnityEngine.Random.Range(0, 2);
        else
            stairIndex = UnityEngine.Random.Range(0, 3);

        // Posibles posiciones de la escalera
        switch (stairIndex)
        {
            case 0:
                stairsPosition = (int)Mathf.Floor(widthCount / 2 - 2);
                break;
            case 1:
                stairsPosition = (int)Mathf.Floor(widthCount / 2 + 2);
                break;
            case 2:
                stairsPosition = (int)Mathf.Floor(widthCount / 2);
                break;
        }

        int sLength = structure.GetLength(0) - 1;

        List<Transform> stairs = shipsManager.SelectAssets("Stairs", 1, true);

        shipsManager.SetParentAndPos(stairs[0],
                                     commingShip.transform,
                                     structure[sLength - 1, stairsPosition].Obj.localPosition + new Vector3(0, 0, 0.5f),
                                     Quaternion.identity);

        SetActiveAndRegister(stairs[0]);

        // Asset que puede ir debajo de la escalera
        if (UnityEngine.Random.Range(0, 1f) < 0.5f)
        {
            PlaceAsset("S", sLength, stairsPosition);
        }
    }

    /// <summary>
    /// Posiciona las escotillas del barco
    /// </summary>
    public void Windows()
    {
        List<Transform> window;

        // ESTO CACA
        int leftCount = 0;
        int rightCount = 0;

        for (int i = 0; i < lengthCount; ++i)
        {
            // Lateral izquierdo del barco
            if (structure[i, 0].Type == 'w')
            {
                if (leftCount % 2 == 0)
                {
                    window = shipsManager.SelectAssets("Windows", 1, true);

                    Vector3 pos = new Vector3(structure[i, 0].Obj.position.x - windowToWallOffset, structure[i, 0].Obj.position.y + windowHeightOffset, structure[i, 0].Obj.position.z);
                    shipsManager.SetParentAndPos(window[0], commingShip.transform, pos, Quaternion.Euler(structure[i, 0].Obj.rotation.eulerAngles.x + 5,
                                                                                                         structure[i, 0].Obj.rotation.eulerAngles.y + 180,
                                                                                                         structure[i, 0].Obj.rotation.eulerAngles.z));
                    SetActiveAndRegister(window[0]);
                }

                ++leftCount;
            }

            // Lateral derecho del barco
            if (structure[i, widthCount - 1].Type == 'w')
            {
                if (rightCount % 2 == 0)
                {
                    window = shipsManager.SelectAssets("Windows", 1, true);

                    Vector3 pos = new Vector3(structure[i, widthCount - 1].Obj.position.x + windowToWallOffset, structure[i, widthCount - 1].Obj.position.y + windowHeightOffset, structure[i, widthCount - 1].Obj.position.z);
                    shipsManager.SetParentAndPos(window[0], commingShip.transform, pos, Quaternion.Euler(structure[i, widthCount - 1].Obj.rotation.eulerAngles.x + 5,
                                                                                                         structure[i, widthCount - 1].Obj.rotation.eulerAngles.y + 180,
                                                                                                         structure[i, widthCount - 1].Obj.rotation.eulerAngles.z));
                    SetActiveAndRegister(window[0]);
                }

                ++rightCount;
            }
        }
    }

    /// <summary>
    /// Posiciona cada asset dentro del barco
    /// </summary>
    public void AssetsPosition()
    {
        GenerateNewAssetsTemplate();

        // Itera sobre la estructura del barco y coloca los assets
        for (int i = 0; i < structure.GetLength(0); ++i)
        {
            for (int j = 0; j < structure.GetLength(1); ++j)
            {
                char c = currentAssetsTemplate[i, j];
                if (c == 'N' || c == 'D' || c == 'S')
                {
                    PlaceAsset(c.ToString(), i, j);
                }
                else if (c == 'L')
                {
                    if (UnityEngine.Random.Range(0, 1f) < 0.5f)
                    {
                        PlaceAsset("L", i, j);
                    }
                    else
                    {
                        PlaceAsset("S", i, j);
                    }
                }
            }
        }

    }

    /// <summary>
    /// Se encarga de colocar los assets en sus posiciones y con la rotación correcta
    /// </summary>
    /// <param name="type"></param>
    /// <param name="i"></param>
    /// <param name="j"></param>
    void PlaceAsset(string type, int i, int j)
    {
        if (structure[i, j].Obj.childCount > 1 && Random.Range(0, 1f) < 0.8f)
        {
            Transform newAsset = shipsManager.SelectAssets(type, 1, false)[0].transform;

            float randX = Random.Range(-0.05f, 0.05f);
            float randZ = Random.Range(-0.05f, 0.05f);
            Vector3 newPos = structure[i, j].Obj.GetChild(1).position + new Vector3(randX, 0, randZ);

            Vector3 rotation = Vector3.zero;

            #region Object own placing function

            MonoBehaviour objMonoBehaviour = newAsset.GetComponent<MonoBehaviour>();

            // Objeto con script de posición y rotación propio
            if (objMonoBehaviour is IPlaceable)
            {
                rotation = (objMonoBehaviour as IPlaceable).Rotate();
                newPos += (objMonoBehaviour as IPlaceable).Place();
            }

            // Objetos que se rotan respecto al centro del barco
            else
            {
                rotation = new Vector3(0, Mathf.Atan((l_center - i) / (w_center - j + 0.001f)) * (180 / Mathf.PI) - 90, 0);

                if ((i < 2 && j <= w_center) || (i >= 2 && j < 2) || (j == w_center + 1 && i > 2))
                    rotation += new Vector3(0, 180, 0);
            }

            #endregion

            // Objetos rellenables
            //if (newAsset.GetChild(0).childCount > 1)
            //    assetsFill.Add(newAsset);

            // EN SUCIO
            if (type == "L")
            {
                if (newAsset.GetChild(0).name == "CajaGrande")
                    assetsBigBox.Add(newAsset);

                else
                    assetsJar.Add(newAsset);
            }

            else if (type == "S")
            {
                if (newAsset.GetChild(0).name == "CajaMedio")
                    assetsSmallBox.Add(newAsset);

                else
                    assetsJar.Add(newAsset);

            }

            else if (type == "D")
                assetsLongBox.Add(newAsset);

            else
                assetsFill.Add(newAsset);


            shipsManager.SetParentAndPos(newAsset, commingShip.transform, newPos, Quaternion.Euler(rotation));
            SetActiveAndRegister(newAsset);
        }
    }

    /// <summary>
    /// Se crea una nueva plantilla dependiendo del tamaño del barco actual
    /// </summary>
    void GenerateNewAssetsTemplate()
    {
        // Filas y columnas a eliminar

        int    rowsToRemove = assetsTemplate.GetLength(0) - lengthCount;
        int columnsToRemove = assetsTemplate.GetLength(1) -  widthCount;

           removedRows.Clear();
        removedColumns.Clear();

        // Se guardan en un array las filas y columnas más susceptibles a ser eliminadas

        if (rowsToRemove > 0)
        {
            SelectByPriority(ref removedRows, lengthPriority, rowsToRemove);
        }

        if (columnsToRemove > 0)
        {
            SelectByPriority(ref removedColumns, widthPriority, columnsToRemove);
        }

        // Se crea un array para los objetos con el tamaño del barco

        currentAssetsTemplate = new char[lengthCount, widthCount];
        
        int wIndex = widthCount - 1,
            lIndex = lengthCount - 1;
        
        // Se rellena el array de posiciones de objetos sin tener en cuenta las filas y columnas de menos prioridad

        for (int i = maxLength - 1; i >= 0 && lIndex >= 0; --i)
        {
            wIndex = widthCount - 1;

            if (removedRows.Contains(i))
                continue;

            for (int j = maxWidth - 1; j >= 0 && wIndex >= 0; --j)
            {
                if (removedColumns.Contains(j))
                    continue;
                
                currentAssetsTemplate[lIndex, wIndex] = assetsTemplate[i, j];
                --wIndex;
            }
            --lIndex;
        }
    }

    /// <summary>
    /// Elimina un cierto número de filas o columnas del array según la prioridad
    /// </summary>
    /// <param name="removed"></param>
    /// <param name="priority"></param>
    void SelectByPriority(ref List<int> removed, int[] priority, int removeCount)
    {
        int currentPriority = 0;

        while (removeCount != removed.Count)
        {
            for (int i = 0; i < priority.Length; ++i)
            {
                if (priority[i] == currentPriority)
                    removed.Add(i);

                if (removeCount == removed.Count)
                    break;
            }

            ++currentPriority;
        }
    }

    /// <summary>
    /// Assets que van dentro de los contenedores del barco
    /// </summary>
    /// <param name="niceContent"></param>
    public void ContentAssets(bool niceContent)
    {
        //int assetsCount = UnityEngine.Random.Range((int)(assetsFill.Count * 0.5f), assetsFill.Count);
        MyList.Shuffle(assetsFill);

        int sBoxCount = Random.Range((int)(assetsSmallBox.Count * 0.8f), (int)(assetsSmallBox.Count - 1));
        int bBoxCount = Random.Range((int)(assetsBigBox.Count * 0.8f), (int)(assetsBigBox.Count));
        int jarsCount = Random.Range((int)(assetsJar.Count * 0.8f), (int)(assetsJar.Count));

        //List<Transform> assets = shipsManager.SelectAssets("Assets Fill Good", assetsCount, false);

        List<Transform> sBoxGoodAssets = shipsManager.SelectAssets("Good assets S Box", sBoxCount, false);
        MyList.Shuffle(sBoxGoodAssets);

        List<Transform> bBoxGoodAssets = shipsManager.SelectAssets("Good assets B Box", bBoxCount, false);
        MyList.Shuffle(bBoxGoodAssets);

        List<Transform> jarsGoodAssets = shipsManager.SelectAssets("Good assets Jars", jarsCount, false);
        MyList.Shuffle(jarsGoodAssets);

        int i = 0;
        int index = -1;

        if (niceContent)
        {
            print("Contenido de barco: OK");
            //print("Assets BUENOS: " + assetsCount);
        }

        else
        {
            print("Contenido de barco: peligroso");
            //print("Assets BUENOS: " + assetsCount);
            //print("Assets MALOS: " + 1);
            List<Transform> sBoxBadAssets = shipsManager.SelectAssets("Bad assets S Box", 1, false);

            index = Random.Range(0, assetsSmallBox.Count);

            sBoxBadAssets[0].position = assetsSmallBox[index].GetChild(0).GetChild(0).position;
            sBoxBadAssets[0].rotation = Quaternion.Euler(0, assetsSmallBox[index].GetChild(0).GetChild(0).rotation.eulerAngles.y, 0);
            SetActiveAndRegister(sBoxBadAssets[0]);
        }

        for (; i < sBoxCount; ++i)
        {
            if (i == index)
                continue;

            sBoxGoodAssets[i].position = assetsSmallBox[i].GetChild(0).GetChild(0).position;
            sBoxGoodAssets[i].rotation = Quaternion.Euler(0, assetsSmallBox[i].GetChild(0).GetChild(0).rotation.eulerAngles.y, 0);
            SetActiveAndRegister(sBoxGoodAssets[i]);
        }

        for (int j = 0; j < bBoxCount; ++j)
        {
            bBoxGoodAssets[j].position = assetsBigBox[j].GetChild(0).GetChild(0).position;
            bBoxGoodAssets[j].rotation = Quaternion.Euler(0, assetsBigBox[j].GetChild(0).GetChild(0).rotation.eulerAngles.y, 0);
            SetActiveAndRegister(bBoxGoodAssets[j]);
        }

        for (int j = 0; j < jarsCount; ++j)
        {
            jarsGoodAssets[j].position = assetsJar[j].GetChild(0).GetChild(0).position;
            jarsGoodAssets[j].rotation = Quaternion.Euler(0, assetsJar[j].GetChild(0).GetChild(0).rotation.eulerAngles.y, 0);
            SetActiveAndRegister(jarsGoodAssets[j]);
        }

        //int nAssets = assets.Count;
        //for (int i = 0; i < nAssets; ++i)
        //{
        //    assets[i].position = assetsFill[i].GetChild(0).GetChild(0).position;
        //    assets[i].rotation = Quaternion.Euler(0, assetsFill[i].GetChild(0).GetChild(0).rotation.eulerAngles.y, 0);
        //    SetActiveAndRegister(assets[i]);
        //}
    }
}
