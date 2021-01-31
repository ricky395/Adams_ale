
// Author: Ricardo Roldán

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controla el spawn de los barcos y la jerarquía de objetos que contiene
/// </summary>
public class ShipsManager : MonoBehaviour
{
    #region Serializable variables

    [SerializeField]
    GameObject commingShip;

    [SerializeField]
    GameObject goingShip;

    [SerializeField]
    Transform assetsPool;
    
    [SerializeField]
    BezierSpline commingRoute,
                    cityRoute = null,
                     outRoute = null;

    #endregion

    #region Private variables

    SplineWalker commingShipWalker;
    SplineWalker goingShipWalker;
    
    List<Transform> activeAssetsPlaceHolders = new List<Transform>();

    List<Transform> chosenAssets = new List<Transform>();

    List<Transform> inactiveAssets = new List<Transform>();
    
    List<KeyValuePair<string, Transform>> kvpAssetsList;

    //List<Component> disabledComponents = new List<Component>();
    List<MeshRenderer> deletedMeshWalls = new List<MeshRenderer>();
    List<BoxCollider> deletedColliders = new List<BoxCollider>();

    Transform activeHull;

    int lengthCount, widthCount;
    int bodyWidth = 1;
    int bodyLength = 1;

    int stairsPosition;

    SplineWalker commingWalker;
    SplineWalker goingWalker;

    GatesManager gates;
    
    bool niceContent;

    public BLINDED_AM_ME.Trail_Mesh go;

    public GameObject cubierta;

    ObjectsPositioning OP;

    #endregion


    public static ShipsManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance == this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    void Start()
    {
        commingShipWalker = commingShip.GetComponent<SplineWalker>();
        goingShipWalker = goingShip.GetComponent<SplineWalker>();
        gates = GetComponent<GatesManager>();

        OP = ObjectsPositioning.Instance;

        SetAssetsList();
    }

    /// <summary>
    /// Se guardan en listas los assets que podrá contener cada barco
    /// </summary>
    void SetAssetsList()
    {
        kvpAssetsList = new List<KeyValuePair<string, Transform>>();
        
        // Recorro los primeros hijos del pool
        foreach (Transform tr in assetsPool)
        {
            Transform[] children = tr.GetComponentsInChildren<Transform>();
            int childCount = children.Length;
            string trName = tr.name;

            // Recorro los hijos de cada grupo dentro del pool
            foreach(Transform trChild in tr)
            {
                trChild.gameObject.SetActive(false);
                kvpAssetsList.Add(new KeyValuePair<string, Transform>(trName, trChild));
            }
        }        
    }

    /// <summary>
    /// Devuelve una cantidad de assets de las listas según un tipo especificado
    /// </summary>
    /// <param name="key"></param>
    /// <param name="objectsToTake"></param>
    /// <param name="inOrder"></param>
    /// <returns></returns>
    public List<Transform> SelectAssets(string key, int objectsToTake, bool inOrder)
    {
        IEnumerable<Transform> selection;

        if (inOrder)
            selection = kvpAssetsList.Where(i => i.Key == key).Where(t => !t.Value.gameObject.activeSelf).Take(objectsToTake).Select(item => item.Value);

        else
            selection = kvpAssetsList.OrderBy(x => Guid.NewGuid()).Where(i => i.Key == key).Where(t => !t.Value.gameObject.activeSelf).Take(objectsToTake).Select(item => item.Value);

        return selection.ToList();
    }

    void Update()
    {
        //if ((Input.GetKeyUp(KeyCode.R) || Input.GetKeyUp(KeyCode.Joystick1Button3)) && Information.Instance.NoShipSpawned())
        //{
        //    NewShip();
        //}

        //if (Input.GetKeyUp(KeyCode.F))
        //{
        //    Information.Instance.SetShipDestination(SplineDestination.City);
        //    //Mesh shipMesh = cubierta.GetComponent<MeshFilter>().mesh;
        //    //List<Vector3> verts = new List<Vector3>();
        //    //shipMesh.GetVertices(verts);

        //    ////Habría que reescalar los puntos según la escala de la cubierta
        //    //foreach(Vector3 v in verts)
        //    //{
        //    //    GameObject GO = new GameObject();
        //    //    GO.transform.parent = go.transform;
        //    //    GO.transform.position = cubierta.transform.position - v;
        //    //}

        //    //go.ShapeIt();
        //}
    }

    /// <summary>
    /// Genera un nuevo barco
    /// </summary>
    public void NewShip()
    {
        InitValues();
        Hierarchy();
        commingShipWalker.SpawnShip(commingRoute);
    }

    /// <summary>
    /// Valores de los barcos (ancho, largo, número de celdas...)
    /// </summary>
    void InitValues()
    {
        RandomAmount();

        niceContent = UnityEngine.Random.Range(0f, 1f) > .5f;
    }

    /// <summary>
    /// Generación aleatoria de valores del barco
    /// </summary>
    void RandomAmount()
    {
        lengthCount = UnityEngine.Random.Range(9, 18);
         widthCount = UnityEngine.Random.Range(5, 10);

        // Width siempre es impar
        if (widthCount % 2 == 0)
            --widthCount;
    }
    
    void Hierarchy()
    {
        SplineWalker.ShipInfo ship = new SplineWalker.ShipInfo(lengthCount + 2, widthCount, bodyLength, bodyWidth);
        commingShipWalker.Ship = ship;
        goingShipWalker.Ship = ship;
                
        PositionShipParts();
    }
    
    /// <summary>
    /// Establece el objeto padre a un elemento y asigna una posción y rotación
    /// </summary>
    public void SetParentAndPos(Transform obj, Transform parent, Vector3 position, Quaternion rotation)
    {
        obj.rotation = rotation;
        obj.position = position;
        obj.parent = parent;
    }

    /// <summary>
    /// Establece el objeto padre a un elemento y asigna una posción, rotación y escala
    /// </summary>
    public void SetParentAndPos(Transform obj, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        SetParentAndPos(obj, parent, position, rotation);
        obj.localScale += scale;
    }

    /// <summary>
    /// Llama a las funciones de posicionado de los objetos del barco
    /// </summary>
    void PositionShipParts()
    {
        OP.Initialize(chosenAssets, commingShip, widthCount, lengthCount, bodyWidth, bodyLength);
        OP.WarehousePosition();
        activeHull = OP.HullPosition();
        OP.SailPosition(activeHull.GetChild(0).GetChild(0));
        OP.StairsPosition();
        OP.Windows();
        OP.AssetsPosition();
        OP.ContentAssets(niceContent);
        AssetsFill();
    }

    /// <summary>
    /// Asigna el parent de los objetos del barco
    /// </summary>
    void AssetsFill()
    {
        // Hull at the end of list
        chosenAssets.Add(activeHull);

        foreach (Transform tr in chosenAssets)
        {
            tr.parent = commingShip.transform;
        }
    }

    /// <summary>
    /// Limpia las listas de objetos del barco actual
    /// </summary>
    void ClearObjectLists()
    {
        chosenAssets.Clear();
        activeAssetsPlaceHolders.Clear();

        foreach (MeshRenderer renderer in deletedMeshWalls)
        {
            renderer.enabled = true;
        }

        foreach (BoxCollider collider in deletedColliders)
        {
            collider.enabled = true;
        }

        deletedMeshWalls.Clear();
        deletedColliders.Clear();
    }

    /// <summary>
    /// Devuelve el objeto parent del barco a su posición y rotación originales
    /// </summary>
    /// <param name="ship"></param>
    void ResetShip(GameObject ship)
    {
        ship.transform.position = Vector3.zero;
        ship.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// El barco cambia a tipo "Going", lo cual deja libre el tipo "Comming" a otro posible barco que se dirija al puerto
    /// </summary>
    /// <param name="_destination"></param>
    public void SetShipToGoing(SplineDestination _destination)
    {
        goingShipWalker.NiceContent = niceContent;

        goingShip.transform.position = commingShip.transform.position;
        goingShip.transform.rotation = commingShip.transform.rotation;

        inactiveAssets.AddRange(chosenAssets);

        // Se cambia el parent de todos los assets
        foreach (Transform tr in inactiveAssets)
        {
            tr.parent = goingShip.transform;
        }

        ResetShip(commingShip);
        
        ClearObjectLists();

        if (_destination == SplineDestination.City)
        {
            goingShipWalker.SpawnShip(cityRoute);
            gates.OpenGates();
        }
        else
            goingShipWalker.SpawnShip(outRoute);
    }

    /// <summary>
    /// Devuelve todos los assets del barco al pool y los desactiva
    /// </summary>
    public void DespawnGoingShip()
    {
        gates.CloseGates();

        foreach (Transform tr in inactiveAssets)
        {
            SetParentAndPos(tr, assetsPool, assetsPool.position, Quaternion.identity);
            tr.gameObject.SetActive(false);
        }

        inactiveAssets[inactiveAssets.Count - 1].localScale = Vector3.one;

        inactiveAssets.Clear();
        ResetShip(goingShip);
    }
}