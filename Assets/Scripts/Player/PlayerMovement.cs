
// Author: Ricardo Roldán

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Mecánica de movimiento del personaje
/// </summary>
public class PlayerMovement : MonoBehaviour
{

#region Player movement variables

    /// <summary>
    /// Velocidad del personaje en los ejes XZ de movimiento
    /// </summary>
    float zSpeed = 0, 
          xSpeed = 0;

    /// <summary>
    /// Velocidad del personaje
    /// </summary>
    [SerializeField]
    float speed = 12;

    /// <summary>
    /// Multiplicador de velocidad al correr
    /// </summary>
    [SerializeField]
    float runMultiplier = 1.5f;
    
    /// <summary>
    /// Multiplicador de velocidad al agacharse
    /// </summary>
    [SerializeField]
    float crouchMultiplier = 1.5f;

    /// <summary>
    /// Variable que controla el multiplicador de velocidad activo
    /// </summary>
    float speedMultiplier = 1;
    
    /// <summary>
    /// Gravedad del personaje
    /// </summary>
    [SerializeField]
    float gravity = -18f;
    
    CharacterController controller;

    KeyInputs input;

    PlayerCamera playerCamera;

    /// <summary>
    /// Velocidad en el eje Y llevada a cabo por la gravedad
    /// </summary>
    float yVelocity = 0;

    /// <summary>
    /// Posición de la cápsula estando el personaje de pie y agachado
    /// </summary>
    float standCapsulleYCenter = 0, crouchCapsulleYCenter = -.5f;

    /// <summary>
    /// Alturas de la cápsula de colisión
    /// </summary>
    float standCapsulleHeight  = 1.8f, crouchCapsulleHeight = 1f;

    /// <summary>
    /// Flag de acción de correr
    /// </summary>
    bool runningFlag;

    /// <summary>
    /// Flag de acción de agacharse
    /// </summary>
    bool m_Crouching;

    /// <summary>
    /// Flag de acción de agacharse
    /// </summary>
    bool crouchFlag;

    /// <summary>
    /// Flag que controla el movimiento del personaje
    /// </summary>
    bool canMove;

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }

#endregion


    public static PlayerMovement Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        controller   = GetComponent<CharacterController>();
        playerCamera = GetComponent<PlayerCamera>();
        canMove = true;

        input = KeyInputs.Instance;
    }
    
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.L))
            SceneManager.LoadScene(0);
        
            

        if (canMove)
        {
            Run();
            Move();

            if (!playerCamera.IsRotating)
                Crouch();
        }

        else if (playerCamera.IsTilting)
        {
            Crouch();
        }
    }

    /// <summary>
    /// Acción de correr
    /// </summary>
    void Run()
    {
        if (input.Run())
        {
            if (!runningFlag && !m_Crouching)
            {
                speedMultiplier = runMultiplier;
                runningFlag = true;
            }
        }

        else if (runningFlag)
        {
            speedMultiplier = 1;
            runningFlag = false;
        }
    }

    /// <summary>
    /// Movimiento del personaje
    /// </summary>
    void Move()
    {
        xSpeed = input.MoveHorizontal();
        zSpeed = input.MoveVertical();        

        if (controller.isGrounded)
            yVelocity = -1;
        else
            yVelocity += Time.deltaTime * gravity;

        // Suma de componentes X Y Z
        Vector3 moveVector = transform.forward * zSpeed + transform.right * xSpeed + transform.up * yVelocity;
        
        // Componentes anteriores aplicados a la posición
        controller.Move(moveVector * Time.deltaTime * speed * speedMultiplier);
    }

    /// <summary>
    /// Acción de agarcharse
    /// </summary>
    void Crouch ()
    {
        if (input.Crouch())
        {
            if (!crouchFlag)
            {
                crouchFlag = true;
                ChangeCapsuleForCrouching(crouchFlag);
            }
        }

        else 
        {
            if (crouchFlag || (controller.height == crouchCapsulleHeight && !playerCamera.PreventStandingInLowHeadroom()))
            {
                crouchFlag = false;
                ChangeCapsuleForCrouching(crouchFlag);
            }
        }
    }

    /// <summary>
    /// Función encargada de tomar las medidas necesarias según el input de agarcharse
    /// </summary>
    /// <param name="crouch"></param>
    void ChangeCapsuleForCrouching(bool crouch)
    {
        if (crouch)
        {
            if (m_Crouching) return;
            
            SwapCapsulleHeight(crouchCapsulleHeight, crouchCapsulleYCenter);

            StopAllCoroutines();
            StartCoroutine(playerCamera.ChangeCameraPos(0));
            m_Crouching = true;
            speedMultiplier = crouchMultiplier;
        }

        else
        {
            if (playerCamera.PreventStandingInLowHeadroom())
                return;

            Stand();
        }
    }

    /// <summary>
    /// Acción de levantarse. Se cambia el tamaño de la cápsula,
    /// la posición de la cámara y la velocidad del personaje
    /// </summary>
    public void Stand()
    {
        SwapCapsulleHeight(standCapsulleHeight, standCapsulleYCenter);

        StopAllCoroutines();
        StartCoroutine(playerCamera.ChangeCameraPos(1));
        m_Crouching = false;
        speedMultiplier = 1;
    }

    /// <summary>
    /// Cambia el tamaño de la cápsula
    /// </summary>
    /// <param name="_height"></param>
    /// <param name="_yCenter"></param>
    void SwapCapsulleHeight(float _height, float _yCenter)
    {
        controller.height = _height;
        controller.center = new Vector3(0, _yCenter, 0);
    }
}