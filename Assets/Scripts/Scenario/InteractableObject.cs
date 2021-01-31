
// Author: Ricardo Roldán

using UnityEngine;

/// <summary>
/// Controla la acción con un objeto de tipo IInteractable
/// </summary>
public class InteractableObject : MonoBehaviour
{

    /// <summary>
    /// Distancia de interacción con objetos
    /// </summary>
    [SerializeField]
    [Range(0.01f, 10)]
    float interactableDistance;

    /// <summary>
    /// Radio de la esfera que es lanzada para interactuar (Spherecast)
    /// </summary>
    [SerializeField]
    [Range(0.1f, 2)]
    float interactableSphereRadius;

    /// <summary>
    /// Distancia máxima de interacción desde la cápsula del personaje (hacia delante)
    /// </summary>
    [SerializeField]
    [Range(0.1f, 8)]
    float interactableMaxDistanceForward;

    /// <summary>
    /// Distancia máxima de interacción desde la cápsula del personaje (hacia los laterales)
    /// </summary>
    [SerializeField]
    [Range(0.1f, 8)]
    float interactableMaxDistanceSides;

    /// <summary>
    /// Flag del personaje si puede o no interactuar
    /// </summary>
    bool canAct;

    Camera cam;

    /// <summary>
    /// Tag de objeto interactuable
    /// </summary>
    string i_tag = "Interactable";

    KeyInputs input;

    public bool CanAct
    {
        get { return canAct; }
        set { canAct = value; }
    }

    public static InteractableObject Instance;

    private void Awake()
    {
        if (Instance != null && Instance == this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    void Start ()
    {
        canAct = true;

        input = KeyInputs.Instance;

        cam = PlayerCamera.Instance.Cam;
    }

    void Update ()
    {
        if (canAct)
            if (input.Action())
            {
                // Genera una spherecast hacia donde apunta la cámara en busca de objetos interactuables

                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                RaycastHit[] hits = Physics.SphereCastAll(ray, interactableSphereRadius, interactableDistance, ~0, QueryTriggerInteraction.Collide);

                foreach(RaycastHit hit in hits)
                {
                    // filtra los objetos de tipo interactuables

                    if (hit.collider.isTrigger && hit.transform.CompareTag(i_tag))
                    {
                        float x_dist = Mathf.Abs(hit.point.x) - Mathf.Abs(cam.transform.position.x);
                        float z_dist = Mathf.Abs(hit.point.z) - Mathf.Abs(cam.transform.position.z);
                        
                        if (Mathf.Abs(x_dist) < interactableMaxDistanceSides && Mathf.Abs(z_dist) < interactableMaxDistanceForward)
                        {
                            Interact(hit.transform);
                            break;
                        }
                    }
                }
            }
    }

    /// <summary>
    /// Acción de interacción
    /// </summary>
    /// <param name="obj"></param>
    void Interact(Transform obj)
    {
        MonoBehaviour objMonoBehaviour = obj.GetComponent<MonoBehaviour>();
        if (objMonoBehaviour is IInteractable && (objMonoBehaviour as IInteractable).IsActive)
        {
            (objMonoBehaviour as IInteractable).DoAction();
        }
    }
}
