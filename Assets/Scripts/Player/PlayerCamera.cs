
// Author: Ricardo Roldán

using System.Collections;
using UnityEngine;

/// <summary>
/// Mecánica de movimiento de la cámara del personaje
/// </summary>
public class PlayerCamera : MonoBehaviour
{

    #region Camera variables

    [SerializeField]
    float lookHorizontalSensitivity = 2;

    [SerializeField]
    float lookVerticalSensitivity = 2;

    [SerializeField]
    float viewUpDownRange = 70.0f;

    [SerializeField]
    float viewLeftRightRange = 45;

    [SerializeField]
    float availableDepthDistance = 0.6f;

    [SerializeField]
    Transform standCameraPoint,
              crouchCameraPoint = null;

    KeyInputs input;

    bool rayEnabled = true;

    CharacterController controller;

    float lookVertical = 0, lookHorizontal = 0,
          lastLookVertical = 0, lastLookHorizontal = 0;
    const float k_Half = 0.5f;

    float crouchRayDistance;
    WaitForSeconds cameraLerpTime = new WaitForSeconds(0.01f);

    string playerLayer = "BallOnTable";
    
    Quaternion initInteractRotation;

    Vector3 originalPlayerRot;

    public Vector3 OriginalPlayerRot
    {
        get { return originalPlayerRot; }
        set { originalPlayerRot = value; }
    }

    Camera cam;

    public Camera Cam
    {
        get { return cam; }
    }

    bool isTilting;

    public bool IsTilting
    {
        get { return isTilting; }
        set { isTilting = value; }
    }

    bool isRotating;

    public bool IsRotating
    {
        get { return isRotating; }
        set { isRotating = value; }
    }

    bool interacting;

    bool disabled;

    public bool Disabled
    {
        get { return disabled; }
        set { disabled = value; }
    }

    #endregion

    public static PlayerCamera Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Start ()
    {
               cam = GetComponentInChildren<Camera>();
        controller = GetComponent<CharacterController>();
        
        crouchRayDistance = standCameraPoint.position.y - transform.position.y;

        input = KeyInputs.Instance;

        disabled = false;

        Cursor.visible = false;

        StartCoroutine(RayFromCam());
    }
	
	void Update ()
    {
        if (isRotating)
        {
            lookHorizontal = lastLookHorizontal;
            lookVertical = lastLookVertical;
        }
        else
        {
            lastLookHorizontal = lookHorizontal;
            lastLookVertical = lookVertical;
        }

        if (!disabled)
            Look();
    }

    /// <summary>
    /// Rotación de la cámara
    /// </summary>
    void Look()
    {
        // Horizontal
        lookHorizontal += input.LookHorizontal() * lookHorizontalSensitivity * 100 * Time.deltaTime;

        if (!isRotating && !isTilting) //!isTilting && 
        {
            transform.Rotate(0, lookHorizontal, 0);
            lookHorizontal = 0;
        }

        // Clamped vertical
        lookVertical -= input.LookVertical() * lookVerticalSensitivity * 100 * Time.deltaTime;
        lookVertical = Mathf.Clamp(lookVertical, -viewUpDownRange, viewUpDownRange);

        if (interacting && !isTilting)
        {
            float yRotation = Mathf.Clamp(transform.rotation.eulerAngles.y, initInteractRotation.eulerAngles.y - viewLeftRightRange, initInteractRotation.eulerAngles.y + viewLeftRightRange);

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, yRotation, transform.rotation.eulerAngles.z);
        }

        if (isTilting)
        {
            transform.rotation = Quaternion.Euler(originalPlayerRot);

            lookHorizontal = Mathf.Clamp(lookHorizontal, -viewLeftRightRange, viewLeftRightRange);

            cam.transform.localRotation = Quaternion.Euler(lookVertical, lookHorizontal, 0);
        }

        else if (!isRotating)
            cam.transform.localRotation = Quaternion.Euler(lookVertical, lookHorizontal, 0);
    }

    /// <summary>
    /// Se activa la interacción con un objeto especial
    /// </summary>
    /// <param name="point"></param>
    public void EnableInteract(Transform point)
    {
        initInteractRotation = point.rotation;
        interacting = true;
    }

    public void DisableInteract()
    {
        interacting = false;
    }

    /// <summary>
    /// Reestablece la rotación del personaje hacia el nuevo ángulo de visión
    /// </summary>
    public void ResetCameraTilting()
    {
        float yRot = cam.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, yRot, 0);
        cam.transform.localRotation = Quaternion.Euler(cam.transform.localRotation.eulerAngles.x, 0, 0);

        lookHorizontal = 0;
    }

    /// <summary>
    /// Cambia mediante lerp la posición de la cámara
    /// </summary>
    /// <param name="targetHeight"></param>
    /// <returns></returns>
    public IEnumerator ChangeCameraPos(int targetPos)
    {
        InteractableObject.Instance.CanAct = false;
        for (;;)
        {
            Vector3 newPos;

            if (targetPos == 0)
                newPos = crouchCameraPoint.position;
            else
                newPos = standCameraPoint.position;

            cam.transform.position = new Vector3(Mathf.Lerp(cam.transform.position.x, newPos.x, 0.2f),
                                                 Mathf.Lerp(cam.transform.position.y, newPos.y, 0.2f),
                                                 Mathf.Lerp(cam.transform.position.z, newPos.z, 0.2f));

            if (Approximately(cam.transform.position.x, newPos.x, 0.05f) &&
                Approximately(cam.transform.position.y, newPos.y, 0.05f) &&
                Approximately(cam.transform.position.z, newPos.z, 0.05f))
            {
                InteractableObject.Instance.CanAct = true;
                yield break;
            }

            yield return cameraLerpTime;
        }
    }

    /// <summary>
    /// Evita que el personaje se levante en lugares poco altos
    /// </summary>
    /// <returns></returns>
    public bool PreventStandingInLowHeadroom()
    {
        Ray ray = new Ray(transform.position, standCameraPoint.position - transform.position);
        return (Physics.SphereCast(ray, controller.radius * k_Half, crouchRayDistance, ~0, QueryTriggerInteraction.Ignore));
    }

    /// <summary>
    /// Función Approximately con offset
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="offsetRange"></param>
    /// <returns></returns>
    public bool Approximately(float a, float b, float offsetRange)
    {
        return b >= a - offsetRange && b <= a + offsetRange ||
               a >= b - offsetRange && a <= b + offsetRange;
    }

    /// <summary>
    /// Controla el flag del Raycast de cámara
    /// </summary>
    /// <param name="state"></param>
    public void SetRayState(bool state)
    {
        rayEnabled = state;
    }

    /// <summary>
    /// Raycast de cámara que evita la colisión con mallas (mecánica PlayerTilt)
    /// </summary>
    /// <returns></returns>
    IEnumerator RayFromCam()
    {
        for(; ; )
        {
            if (rayEnabled)
            {
                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, availableDepthDistance))
                {
                    if (!hit.collider.isTrigger && hit.transform.gameObject.layer != LayerMask.NameToLayer(playerLayer))
                    {
                        PostProcess.Instance.ChangeDepthFocusDist(hit.distance);
                        PostProcess.Instance.ChangeDepthAperture(3.5f);
                        PostProcess.Instance.ChangeDepthFocalLength(32);
                    }
                }

                else
                {
                    PostProcess.Instance.ResetDepth();
                }
            }

            yield return cameraLerpTime;
        }
    }
}
