
// Author: Ricardo Roldán

using UnityEngine;

/// <summary>
/// Controlador del portón
/// </summary>
public class GatesManager : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    /// <summary>
    /// Activa la animación de abrir la puerta
    /// </summary>
    public void OpenGates()
    {
        animator.SetBool("Open", true);
    }

    /// <summary>
    /// Activa la animación de cerrar la puerta
    /// </summary>
    public void CloseGates()
    {
        animator.SetBool("Open", false);
    }
}
