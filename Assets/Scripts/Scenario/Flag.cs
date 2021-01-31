
// Author: Ricardo Roldán

using System.Collections;
using UnityEngine;

/// <summary>
/// Interactuable de tipo bandera
/// </summary>
public class Flag : MonoBehaviour, IInteractable
{
    [Tooltip("Velocidad de interpolación de posición de cámara al interactuar")]
    [SerializeField]
    float posInterpolationStep;

    [Tooltip("Velocidad de interpolación de rotación de cámara al interactuar")]
    [SerializeField]
    float rotInterpolationStep;

    [Tooltip("Velocidad de la animación de la bandera")]
    [SerializeField]
    float animationSpeed;

    [Tooltip("Posición del jugador al interactuar con la bandera")]
    [SerializeField]
    GameObject playerPosition;

    /// <summary>
    /// Transform del personaje
    /// </summary>
    Transform player;

    /// <summary>
    /// Velocidad de interpolación de movimiento y rotación de cámara al interactuar
    /// </summary>
    WaitForSeconds lerpTime = new WaitForSeconds(0.01f);
    
    /// <summary>
    /// Referencia al script de cámara del jugador
    /// </summary>
    PlayerCamera playerCam;

    /// <summary>
    /// Referencia al script de inputs
    /// </summary>
    KeyInputs input;

    /// <summary>
    /// Referencia al animator
    /// </summary>
    Animator animator;

    /// <summary>
    /// Información del clip de animación que se está reproduciendo
    /// </summary>
    AnimatorClipInfo[] currentClipInfo;

    bool doOnce;

    /// <summary>
    /// Flag que controla si el personaje está o no interactuando con la bandera
    /// </summary>
    bool isInteracting;

    /// <summary>
    /// Estado de la bandera
    /// </summary>
    enum AnimState
    {
        None, RedCompleted, GreenCompleted
    }

    AnimState animState = AnimState.None;

    /// <summary>
    /// Objeto activo para poder interactuar con él
    /// </summary>
    bool isActive;

    public bool IsActive
    {
        get { return isActive; }
    }

    public static Flag Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        playerCam = PlayerCamera.Instance;
        player = playerCam.gameObject.transform;
        animator = GetComponent<Animator>();

        isActive = true;
        isInteracting = false;

        input = KeyInputs.Instance;
    }

    void Update()
    {
        currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);

        if (isInteracting)
        {
            float horizontal = input.MoveHorizontal();

            // Movimiento a la izquierda (bandera roja)
            if (horizontal < - input.deadZone || animState == AnimState.RedCompleted)
            {
                animator.SetBool("Backwards", false);
                animator.SetTrigger("Red");
                animator.SetFloat("Speed", animationSpeed);

                string clipName = currentClipInfo[0].clip.name;

                // Devuelve la animación al estado inicial si se pretende izar la otra bandera
                if (clipName != "Red Flag lift" && clipName != "Idle deactivated")
                {
                    AnimBackwards();
                }
            }

            // Movimiento a la derecha (bandera verde)
            else if (horizontal > input.deadZone || animState == AnimState.GreenCompleted)
            {
                animator.SetBool("Backwards", false);
                animator.SetTrigger("Green");
                animator.SetFloat("Speed", animationSpeed);

                string clipName = currentClipInfo[0].clip.name;

                // Devuelve la animación al estado inicial si se pretende izar la otra bandera
                if (clipName != "Green Flag lift" && clipName != "Idle deactivated")
                {
                    AnimBackwards();
                }
            }

            // No input
            else
            {
                AnimBackwards();
            }           
        }

        if (doOnce)
        {
            // Se da un rumbo al barco del puerto según la bandera

            if (currentClipInfo[0].clip.name == "Idle green activated")
            {
                doOnce = false;

                StartCoroutine(Reset());

                if (Information.Instance.ActiveShip != null)
                {
                    Information.Instance.SetShipDestination(SplineDestination.City);
                }
            }

            else if (currentClipInfo[0].clip.name == "Idle red activated")
            {
                doOnce = false;

                StartCoroutine(Reset());

                if (Information.Instance.ActiveShip != null)
                {
                    Information.Instance.SetShipDestination(SplineDestination.Out);
                }
            }
        }

        // Se resetea el flag que da pie a los destinos del barco
        if (currentClipInfo.Length > 0 && currentClipInfo[0].clip.name == "Idle deactivated")
        {
            doOnce = true;
        }
    }

    /// <summary>
    /// Devuelve la animación a su estado inicial y desactiva la interacción del jugador con la bandera
    /// </summary>
    /// <returns></returns>
    IEnumerator Reset()
    {
        yield return new WaitForEndOfFrame();

        PlayerMovement.Instance.CanMove = true;
        InteractableObject.Instance.CanAct = true;
        PlayerTilt.Instance.CanTilt = true;

        isInteracting = false;

        playerCam.DisableInteract();
    }

    /// <summary>
    /// Se cambia la velocidad de animación a negativa
    /// </summary>
    void AnimBackwards()
    {
        animator.SetFloat("Speed", -animationSpeed * 2);
        animator.SetBool("Backwards", true);
    }

    /// <summary>
    /// Se establece el estado de animación por evento en los propios clips de animación
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimationState(int id)
    {
        animState = (AnimState)id;
    }

    /// <summary>
    /// Inicio de animación establecido por evento en los clips de animación
    /// </summary>
    public void AnimationStart()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0 && animator.GetBool("Backwards") && (currentClipInfo[0].clip.name == "Green Flag lift" || currentClipInfo[0].clip.name == "Red Flag lift"))
        {
            animator.Play("Idle deactivated");
            animState = AnimState.None;
        }
    }

    /// <summary>
    /// Fin de animación en caso de dejar de interactuar en medio del izado
    /// </summary>
    public void AnimationEnd()
    {
        animState = AnimState.None;
        animator.SetBool("Backwards", true);
        animator.SetFloat("Speed", - animationSpeed * 2);
        
        if (currentClipInfo[0].clip.name == "Idle green activated")
        {
            animator.Play("Green Flag lift", 0, 1);
            
        }
        else if (currentClipInfo[0].clip.name == "Idle red activated")
        {
            animator.Play("Red Flag lift", 0, 1);
        }

    }

    /// <summary>
    /// Lo que ocurre al interactuar
    /// </summary>
    public void DoAction()
    {
        // En caso de pulsar la tecla de interacción al estar ya interactuando...
        if (isInteracting)
        {
            StartCoroutine(Reset());

            // Si no se ha izado una bandera en su totalidad... (animación sin completar)
            if ((currentClipInfo[0].clip.name == "Green Flag lift" || currentClipInfo[0].clip.name == "Red Flag lift") && animState == AnimState.None)
            {
                AnimationEnd();
            }
        }        

        else if (currentClipInfo[0].clip.name == "Idle deactivated" && Information.Instance.ActiveShip != null)
        {
            isInteracting = true;
            PlayerMovement.Instance.Stand();

            playerCam.EnableInteract(playerPosition.transform);
            PlayerMovement.Instance.CanMove = false;
            InteractableObject.Instance.CanAct = false;
            PlayerTilt.Instance.CanTilt = false;

            StopAllCoroutines();

            StartCoroutine(LerpRotation(player, playerPosition.transform.rotation, true));
            StartCoroutine(LerpPosition(player, playerPosition.transform.position, true));
            
        }
    }

    /// <summary>
    /// Cambia la posición del player mediante lerp
    /// </summary>
    /// <param name="targetHeight"></param>
    /// <returns></returns>
    IEnumerator LerpPosition(Transform obj, Vector3 targetPos, bool restrictMovement)
    {
        InteractableObject.Instance.CanAct = false;
        

        for (;;)
        {
            obj.position = new Vector3(Mathf.Lerp(obj.position.x, targetPos.x, posInterpolationStep),
                                       Mathf.Lerp(obj.position.y, targetPos.y, posInterpolationStep),
                                       Mathf.Lerp(obj.position.z, targetPos.z, posInterpolationStep));

            if ((playerCam.Approximately(obj.position.x, targetPos.x, 0.01f) &&
                playerCam.Approximately(obj.position.y, targetPos.y, 0.01f) &&
                playerCam.Approximately(obj.position.z, targetPos.z, 0.01f)))
            {
                InteractableObject.Instance.CanAct = true;
                yield break;
            }

            yield return lerpTime;
        }
    }

    /// <summary>
    /// Cambia la rotación de cámara mediante lerp
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="targetRot"></param>
    /// <param name="restrictRotation"></param>
    /// <returns></returns>
    IEnumerator LerpRotation(Transform obj, Quaternion targetRot, bool restrictRotation)
    {
        InteractableObject.Instance.CanAct = false;

        if (restrictRotation)
            PlayerCamera.Instance.Disabled = true;

        for (;;)
        {
            obj.rotation = Quaternion.Lerp(obj.rotation, targetRot, rotInterpolationStep);

            if (playerCam.Approximately(obj.rotation.eulerAngles.x, targetRot.eulerAngles.x, 0.5f) &&
                playerCam.Approximately(obj.rotation.eulerAngles.y, targetRot.eulerAngles.y, 0.5f) &&
                playerCam.Approximately(obj.rotation.eulerAngles.z, targetRot.eulerAngles.z, 0.5f))
            {
                InteractableObject.Instance.CanAct = true;
                PlayerCamera.Instance.Disabled = false;
                yield break;
            }

            yield return lerpTime;
        }
    }
}
