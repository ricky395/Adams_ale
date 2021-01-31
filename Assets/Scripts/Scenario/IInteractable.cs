
// Author: Ricardo Roldán

/// <summary>
/// Interfaz de objeto interactuable genérico
/// </summary>
public interface IInteractable
{
    bool IsActive
    {
        get;
    }

    void DoAction();
}
