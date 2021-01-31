
// Author: Ricardo Roldán

using System.Collections;
using UnityEngine;

/// <summary>
/// Clase controladora de la mecánica de cabeceo del personaje
/// </summary>
[System.Serializable]
public class PlayerTilt : MonoBehaviour 
{
    /// <summary>
    /// Valores de movimiento en x para todas las posiciones posibles
    /// </summary>
    [HideInInspector, SerializeField]
    public float[] xValues;

    /// <summary>
    /// Valores de movimiento en z para todas las posiciones posibles
    /// </summary>
    [HideInInspector, SerializeField]
    public float[] zValues;

    /// <summary>
    /// Referencia a la clase manager de inputs
    /// </summary>
    KeyInputs input;

    /// <summary>
    /// Referencia a la clase de movimiento del personaje
    /// </summary>
    PlayerMovement playerMov;

    /// <summary>
    /// Referencia a la clase de cámara del personaje
    /// </summary>
    PlayerCamera playerCam;

    /// <summary>
    /// Cámara activa del personaje
    /// </summary>
    Camera cam;

    [Tooltip("Velocidad a la inversa de movimiento de cámara (más rápido mientras menor sea el valor)")]
    [SerializeField]
    [Range(0.001f, 0.025f)]
    float cameraMovementTimeStep = 0.01f;

    /// <summary>
    /// Tiempo entre repeticiones de la corutina de movimiento de cámara
    /// </summary>
    WaitForSeconds cameraLerpTime;

    Vector3 originalCamPos;

    bool canTilt;

    /// <summary>
    /// Propiedad para cambiar externamente la posibilidad del cabeceo
    /// </summary>
    public bool CanTilt
    {
        get { return canTilt; }
        set { canTilt = value; }
    }


    public static PlayerTilt Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Start () 
	{
        input = KeyInputs.Instance;
        canTilt = true;

        playerMov = PlayerMovement.Instance;
        playerCam = PlayerCamera.Instance;
        cam = playerCam.Cam;

        
        if (xValues == null)
            xValues = new float[8];

        if (zValues == null)
            zValues = new float[8];
        
        originalCamPos = new Vector3(transform.position.x, playerCam.Cam.transform.position.y, transform.position.z);
    }
	
	void LateUpdate ()
    {
        cameraLerpTime = new WaitForSeconds(cameraMovementTimeStep);

        if (canTilt)
        {
            // Mientras la tecla del cabeceo esté presionado...
	        if (input.Tilt())
            {
                EvaluateTilt();
            }

            // Se actualiza la posición siempre que no se pulse la tecla para evitar bugs al activar el cabeceo
            else
            {
                originalCamPos = new Vector3(transform.position.x, playerCam.Cam.transform.position.y, transform.position.z);

                AssignCamPosition(originalCamPos);
            }

            // Primer frame al pulsar la tecla de cabeceo
            if (input.TiltStart())
            {
                playerCam.OriginalPlayerRot = transform.rotation.eulerAngles;

                playerMov.CanMove = false;
                playerCam.IsTilting = true;
            }

            // Frame en el que se deja de pulsar la tecla
            else if (input.TiltEnd())
            {
                playerCam.ResetCameraTilting();

                playerMov.CanMove = true;
                playerCam.IsTilting = false;
            }
        }
    }

    /// <summary>
    /// Evalúa la dirección de movimiento de la cámara según el input
    /// </summary>
    void EvaluateTilt()
    {
        float horizontal = input.MoveHorizontal();
        float   vertical = input.MoveVertical();

        #region Evaluation

        // No input A + D
        if (horizontal == 0)
        {
            // Backward
            if (vertical < 0)
            {
                CheckPosition(xValues[6], zValues[6]);
            }

            // Forward
            else if (vertical > 0)
            {
                CheckPosition(xValues[1], zValues[1]);
            }

            // No input
            else
            {
                AssignCamPosition(originalCamPos);
            }
        }

        // A Input
        else if (horizontal < 0)
        {
            // Backward
            if (vertical < 0)
            {
                CheckPosition(xValues[5], zValues[5]);
            }

            // Forward
            else if (vertical > 0)
            {
                CheckPosition(xValues[0], zValues[0]);
            }

            // No vert input
            else
            {
                CheckPosition(xValues[3], zValues[3]);
            }
        }

        // D Input
        else
        {
            // Backward
            if (vertical < 0)
            {
                CheckPosition(xValues[7], zValues[7]);
            }

            // Forward
            else if (vertical > 0)
            {
                CheckPosition(xValues[2], zValues[2]);
            }

            // No vert input
            else
            {
                CheckPosition(xValues[4], zValues[4]);
            }
        }

        #endregion
    }
    
    /// <summary>
    /// Lanza un rayo a la nueva posición para controlar las colisiones
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="currentVal"></param>
    void CheckPosition(float x, float z)
    {
        // Se lanza un rayo con algo más de tamaño que el movimiento original
        Vector3 direction = transform.forward * z * 1.2f + transform.right * x * 1.2f;

        Ray ray = new Ray(originalCamPos, direction);
        RaycastHit hit;

        // Se recalcula la posición en caso de que el raycast colisione
        if (Physics.Raycast(ray, out hit, ray.direction.magnitude))
        {
            Vector3 hitDir = originalCamPos - hit.point;

            Vector3 newPos = new Vector3(hit.point.x + hitDir.x * 0.3f, originalCamPos.y, hit.point.z + hitDir.z * 0.3f);

            StopAllCoroutines();
            StartCoroutine(ChangeCameraPos(newPos));
        }

        // En caso de no haber colisión, la dirección es la recibida en el input
        else
        {
            Vector3 newPos = new Vector3(originalCamPos.x + direction.x, originalCamPos.y, originalCamPos.z + direction.z);

            AssignCamPosition(newPos);
        }
    }

    /// <summary>
    /// Se para cualquier movimiento de cámara y se establece la nueva posición a la que moverse
    /// </summary>
    void AssignCamPosition(Vector3 newPos)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeCameraPos(newPos));
    }

    /// <summary>
    /// Cambia mediante lerp la posición de la cámara
    /// </summary>
    /// <param name="targetHeight"></param>
    /// <returns></returns>
    public IEnumerator ChangeCameraPos(Vector3 newPos)
    {
        //InteractableObject.Instance.CanAct = false;
        for (;;)
        {
            cam.transform.position = new Vector3(Mathf.Lerp(cam.transform.position.x, newPos.x, 0.2f),
                                                 Mathf.Lerp(cam.transform.position.y, newPos.y, 0.2f),
                                                 Mathf.Lerp(cam.transform.position.z, newPos.z, 0.2f));

            if (playerCam.Approximately(cam.transform.position.x, newPos.x, 0.05f) &&
                playerCam.Approximately(cam.transform.position.y, newPos.y, 0.05f) &&
                playerCam.Approximately(cam.transform.position.z, newPos.z, 0.05f))
            {
                //InteractableObject.Instance.CanAct = true;
                yield break;
            }

            yield return cameraLerpTime;
        }
    }
}
