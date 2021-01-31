using UnityEngine;

/// <summary>
/// Interfaz de objetos con función de colocación propia
/// </summary>
public interface IPlaceable
{
    Vector3 Place();
    Vector3 Rotate();
}
