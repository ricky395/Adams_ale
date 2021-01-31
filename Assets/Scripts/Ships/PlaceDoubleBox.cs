
// Author: Ricardo Roldán

using UnityEngine;

/// <summary>
/// Posicionamiento de caja doble
/// </summary>
public class PlaceDoubleBox : MonoBehaviour, IPlaceable
{
    public Vector3 Place()
    {
        return Vector3.zero;
    }

    public Vector3 Rotate()
    {
        if (Random.Range(0, 1) < 0.5f)
            return new Vector3(0, 90, 0);
        
        return new Vector3(0, -90, 0);
    }
}
