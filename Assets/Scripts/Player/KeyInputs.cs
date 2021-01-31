
// Author: Ricardo Roldán

using UnityEngine;

/// <summary>
/// Clase encargada de recibir y controlar el input del jugador mediante teclado, ratón y mando
/// </summary>
[ExecuteInEditMode]
public class KeyInputs : MonoBehaviour
{
    /// <summary>
    /// Control de teclado
    /// </summary>
    [System.Serializable]
    public struct KeyboardKeys
    {
        public KeyCode Forward;
        public KeyCode Left;
        public KeyCode Backward;
        public KeyCode Right;
        public KeyCode Run;
        public KeyCode Menu;
        public KeyCode Action;
        public KeyCode Crouch;
        public KeyCode Tilt;
        public KeyCode Tilt2;
    }

    /// <summary>
    /// Ejes del mando
    /// </summary>
    [System.Serializable]
    public struct Axis
    {
        [Tooltip("Nombre del eje de la ventana Edit/Project Settings/Input")]
        public string axisName;
        float axisValue;

        public float AxisValue
        {
            get { return axisValue; }
            set { axisValue = value; }
        }
    }

    /// <summary>
    /// Ejes del ratón
    /// </summary>
    [System.Serializable]
    public struct MouseKeys
    {
        public Axis LookHorizontal;
        public Axis LookVertical;
    }

    /// <summary>
    /// Controles de mando
    /// </summary>
    [System.Serializable]
    public struct ControllerKeys
    {
        public Axis VerticalAxis;
        public Axis HorizontalAxis;
        public Axis LookXAxis;
        public Axis LookYAxis;
        public KeyCode Menu;
        public KeyCode Run;
        public KeyCode Action;
        public KeyCode Crouch;
        public KeyCode Tilt;
        public KeyCode Tilt2;
    }

    /// <summary>
    /// Zona mínima en la que se tiene en cuenta el movimiento del stick de mando [0,1]
    /// </summary>
    public float deadZone = 0.2f;

    [SerializeField]
    KeyboardKeys _keyboardKeys;

    [SerializeField]
    MouseKeys _mouseKeys;

    [SerializeField]
    ControllerKeys _controllerKeys;

    #region Properties
    public KeyboardKeys Keyboardkeys
    {
        get { return _keyboardKeys; }
    }

    public MouseKeys Mousekeys
    {
        get { return _mouseKeys; }
    }

    public ControllerKeys Controllerkeys
    {
        get { return _controllerKeys; }
    }
    #endregion

    public static KeyInputs Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;

        InitDefaultKeys();
    }
	
    /// <summary>
    /// Controles por defecto
    /// </summary>
    void InitDefaultKeys()
    {
        _keyboardKeys.Forward = KeyCode.W;
        _keyboardKeys.Backward = KeyCode.S;
        _keyboardKeys.Right = KeyCode.D;
        _keyboardKeys.Left = KeyCode.A;
        _keyboardKeys.Run = KeyCode.LeftShift;
        _keyboardKeys.Menu = KeyCode.Escape;
        _keyboardKeys.Action = KeyCode.E;
        _keyboardKeys.Crouch = KeyCode.C;
        _keyboardKeys.Tilt = KeyCode.LeftControl;
        _keyboardKeys.Tilt2 = KeyCode.Space;

        _mouseKeys.LookHorizontal.axisName = "Mouse X";
        _mouseKeys.LookVertical.axisName = "Mouse Y";

        _controllerKeys.VerticalAxis.axisName = "Movement Vertical";
        _controllerKeys.HorizontalAxis.axisName = "Movement Horizontal";
        _controllerKeys.LookXAxis.axisName = "Look Horizontal";
        _controllerKeys.LookYAxis.axisName = "Look Vertical";
        _controllerKeys.Run = KeyCode.Joystick1Button1;
        _controllerKeys.Action = KeyCode.Joystick1Button0;
        _controllerKeys.Crouch = KeyCode.Joystick1Button2;
        _controllerKeys.Tilt = KeyCode.JoystickButton5;
    }

    private void Update()
    {
        _mouseKeys.LookHorizontal.AxisValue = Input.GetAxis(_mouseKeys.LookHorizontal.axisName);
        _mouseKeys.LookVertical.AxisValue = Input.GetAxis(_mouseKeys.LookVertical.axisName);

        _controllerKeys.HorizontalAxis.AxisValue = Input.GetAxisRaw(_controllerKeys.HorizontalAxis.axisName);
        _controllerKeys.VerticalAxis.AxisValue = Input.GetAxisRaw(_controllerKeys.VerticalAxis.axisName);
        _controllerKeys.LookXAxis.AxisValue = Input.GetAxisRaw(_controllerKeys.LookXAxis.axisName);
        _controllerKeys.LookYAxis.AxisValue = Input.GetAxisRaw(_controllerKeys.LookYAxis.axisName);

        //if (_controllerKeys.RightLeftAxis.AxisValue < -0.3 || _controllerKeys.RightLeftAxis.AxisValue > 0.3)
        //    print("L X");
        //if (_controllerKeys.ForwardBackwardAxis.AxisValue < -0.3 || _controllerKeys.ForwardBackwardAxis.AxisValue > 0.3)
        //    print("L Y");

        //if (_controllerKeys.LookXAxis.AxisValue < -0.3 || _controllerKeys.LookXAxis.AxisValue > 0.3)
        //    print("R X");
        //if (_controllerKeys.LookYAxis.AxisValue < -0.3 || _controllerKeys.LookYAxis.AxisValue > 0.3)
        //    print("R Y");
    }

    //public void ChangeKey()
    //{

    //}

    public float MoveHorizontal()
    {
        float r = 0f;
        r += Input.GetAxis("Movement Horizontal");
        r += Input.GetAxis("Horizontal");

        return Mathf.Clamp(r, -1, 1);
    }

    public float MoveVertical()
    {
        float r = 0f;
        r += Input.GetAxis("Movement Vertical");
        r += Input.GetAxis("Vertical");

        return Mathf.Clamp(r, -1, 1);
    }

    public float LookHorizontal()
    {
        float r = 0f;
        r += Input.GetAxis("Look Horizontal");
        r += Input.GetAxis("Mouse X");

        return r;
    }

    public float LookVertical()
    {
        float r = 0f;
        r += Input.GetAxis("Look Vertical");
        r += Input.GetAxis("Mouse Y");

        return r;
    }

    public bool Crouch()
    {
        return Input.GetKey(_keyboardKeys.Crouch) || Input.GetKey(_controllerKeys.Crouch);
    }

    public bool Action()
    {
        return Input.GetKeyUp(_keyboardKeys.Action) || Input.GetKeyUp(_controllerKeys.Action);
    }

    public bool Run()
    {
        return Input.GetKey(_keyboardKeys.Run) || Input.GetKey(_controllerKeys.Run);
    }

    public bool Tilt()
    {
        return Input.GetKey(_keyboardKeys.Tilt) || Input.GetKey(_keyboardKeys.Tilt2) || Input.GetKey(_controllerKeys.Tilt);
    }

    public bool TiltStart()
    {
        return Input.GetKeyDown(_keyboardKeys.Tilt) || Input.GetKeyDown(_keyboardKeys.Tilt2) || Input.GetKeyDown(_controllerKeys.Tilt);
    }

    public bool TiltEnd()
    {
        return Input.GetKeyUp(_keyboardKeys.Tilt) || Input.GetKeyUp(_keyboardKeys.Tilt2) || Input.GetKeyUp(_controllerKeys.Tilt);
    }
}
