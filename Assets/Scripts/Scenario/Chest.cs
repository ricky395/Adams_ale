
// Author: Ricardo Roldán

using UnityEngine;

/// <summary>
/// Interactuable de tipo cofre o caja
/// </summary>
public class Chest : MonoBehaviour, IInteractable
{
	bool isActive;

	public bool IsActive
	{
		get { return isActive; }
	}

	bool isOpened;
    
	Animator anim;

	private void Start()
	{
		isActive = true;
		anim = GetComponent<Animator>();
	}

    /// <summary>
    /// Acción del objeto al interactuar
    /// </summary>
	public void DoAction()
    {
		isOpened = !isOpened;
		anim.SetBool("isOpened", isOpened);
	}
}