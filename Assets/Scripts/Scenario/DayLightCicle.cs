
// Author: Ricardo Roldán

using UnityEngine;

/// <summary>
/// 
/// </summary>
public class DayLightCicle : MonoBehaviour
{

    /// <summary>
    /// Velocidad de giro de la luz direccional
    /// </summary>
    [SerializeField]
    float dayVelocity = 1;

    /// <summary>
    /// Flag de giro de la luz
    /// </summary>
    bool active = true;

    /// <summary>
    /// Flag de final de partida
    /// </summary>
    bool doOnce = true;
	
    void Update ()
    {
        if (active)
        {
            transform.Rotate(dayVelocity * Time.deltaTime, 0, 0);
        }
        else
        {
            if (doOnce)
            {
                EndOfDay.Instance.FadeOut();
                doOnce = false;
            }
        }

        if (transform.rotation.eulerAngles.x >= 183)
            active = false;
    }
}
