using UnityEngine;
using UnityEditor;

/// <summary>
/// Posicionamiento de hamaca
/// </summary>
public class PlaceHammock : MonoBehaviour, IPlaceable
{
    [SerializeField]
    Transform anchorTransform;

    public Vector3 Place()
    {
        //Ray ray = new Ray(anchorTransform.position, new Vector3(anchorTransform.position.x + 5, anchorTransform.position.y, anchorTransform.position.z));
        //RaycastHit hitInfo;
        
        //Selection.activeTransform = transform;
        //EditorApplication.isPaused = true;

        //float moveDistance = 0;

        //if (Physics.Raycast(ray, out hitInfo))
        //{
        //    moveDistance = Vector3.Distance(hitInfo.point, anchorTransform.position);
        //}
        
        return new Vector3(0, 0, 0.9f);
    }

    public Vector3 Rotate()
    {
        return new Vector3(0, -90, 0);
    }
}
