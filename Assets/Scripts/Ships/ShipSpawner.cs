
// Author: Ricardo Roldán

using System.Collections;
using UnityEngine;

/// <summary>
/// Se encarga de llamar al spawner de ShipsManager cuando es necesario
/// </summary>
public class ShipSpawner : MonoBehaviour
{
    [SerializeField]
    float minSpawnTime = 2;

    [SerializeField]
    float maxSpawnTime = 8;
    
    bool doOnce = true;


	void Update ()
    {
		if (Information.Instance.NoShipSpawned())
        {
            if (doOnce)
            {
                float timer = Random.Range(minSpawnTime, maxSpawnTime);
                StartCoroutine(Timer(timer));
                doOnce = false;
            }
        }
        else
        {
            doOnce = true;
        }
	}

    /// <summary>
    /// Llama a un nuevo barco tras una espera de tiempo
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    IEnumerator Timer(float timer)
    {
        yield return new WaitForSeconds(timer);

        if (Information.Instance.NoShipSpawned())
            ShipsManager.Instance.NewShip();
    }
}
