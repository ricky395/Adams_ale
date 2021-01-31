
// Author: Ricardo Roldán

using System.Collections;
using UnityEngine;

/// <summary>
/// Interactuable de tipo objeto rotable
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class RotableObject : MonoBehaviour , IInteractable
{
    /// <summary>
    /// Velocidad de giro
    /// </summary>
    [SerializeField]
    float rotationSpeed;

    /// <summary>
    /// Estado del objeto
    /// </summary>
    bool isActive;

    public bool IsActive
    {
        get { return isActive; }
    }

    Transform mainCameraTr;

    PlayerMovement playerMovement;
    PlayerCamera playerCamera;

    float greaterMeasure;

    /// <summary>
    /// Posición inicial
    /// </summary>
    Vector3 initPos;

    /// <summary>
    /// Rotación inicial
    /// </summary>
    Quaternion initRot;

    SphereCollider sphere;

    bool notInitiated;

    KeyInputs input;

	void Start ()
    {
        isActive     = true;
        notInitiated = true;

        mainCameraTr = PlayerCamera.Instance.Cam.transform;

        input = KeyInputs.Instance;

        playerMovement = PlayerMovement.Instance;
        playerCamera = PlayerCamera.Instance;

        sphere = GetComponent<SphereCollider>();
    }

    /// <summary>
    /// Inicializa el objeto
    /// </summary>
    void Initiate()
    {
        notInitiated = false;
        
        Vector3 meshSizes = GetComponentInChildren<MeshFilter>().mesh.bounds.size;
        float [] sizes = { meshSizes.x, meshSizes.y, meshSizes.z};

        // Obtains the greatest measure of all object axes
        greaterMeasure = 0;
        for (int i = 0; i < 3; ++i)
        {
            if (sizes[i] > greaterMeasure)
                greaterMeasure = sizes[i];
        }

        initPos = transform.position;
        initRot = transform.rotation;
    }
	
	void Update ()
    {
        if (playerCamera.IsRotating && !isActive)
        {
            float x = - input.LookHorizontal() * rotationSpeed;
            float y =   input.LookVertical()   * rotationSpeed;

            //transform.Rotate(mainCameraTr.right, y, Space.Self);
            //transform.Rotate(mainCameraTr.up, x, Space.Self);

            transform.RotateAround(sphere.transform.position, mainCameraTr.right, y);
            transform.RotateAround(sphere.transform.position, mainCameraTr.up,    x);

            if (input.Action())
                DisableRotation();
        }
    }

    /// <summary>
    /// Función llamada desde el manager de interacción al interactuar
    /// </summary>
    public void DoAction()
    {
        if (notInitiated)
            Initiate();
            
        StartCoroutine(EnableRotation());
    }

    /// <summary>
    /// Activa la rotación del objeto y lo coloca frente a la cámara
    /// </summary>
    /// <returns></returns>
    IEnumerator EnableRotation()
    {
        // Prevents instant enable and disable
        yield return new WaitForEndOfFrame();
        
        isActive = false;
        //playerController.enabled = false;

        playerMovement.CanMove = false;
        playerCamera.IsRotating = true;
        transform.position = mainCameraTr.position + mainCameraTr.forward;
        transform.LookAt(mainCameraTr);
        Vector3 position = transform.position - (transform.forward.normalized * 0.2f); //* greaterMeasure
        transform.position = position;
        InteractableObject.Instance.CanAct = false;

        // Raycast of depth of field
        playerCamera.SetRayState(false);
        PostProcess.Instance.ChangeDepthFocusDist(0.8f);
        PostProcess.Instance.ChangeDepthAperture(3.5f);
        PostProcess.Instance.ChangeDepthFocalLength(32);
    }

    /// <summary>
    /// Desactiva la rotación del objeto y lo devuelve a su transform original
    /// </summary>
    void DisableRotation()
    {
        //playerController.enabled = true;

        playerMovement.CanMove = true;
        playerCamera.IsRotating = false;
        transform.position = initPos;
        transform.rotation = initRot;
        isActive = true;
        InteractableObject.Instance.CanAct = true;

        // Raycast of depth of field
        playerCamera.SetRayState(true);
        PostProcess.Instance.ResetDepth();
    }
}
