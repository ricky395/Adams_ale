using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FPSDebug : MonoBehaviour 
{

    [SerializeField]
    Text fpsText;

    [SerializeField]
    Text highestText;

    [SerializeField]
    Text lowestText;

    float deltaTime = 0, 
                fps = 0;

    float highestFps,
           lowestFps;

    private void Start()
    {
        StartCoroutine(Updt());
        StartCoroutine(SetHighestAndLowest());
    }

    IEnumerator Updt()
    {
        for (; ; )
        {
            deltaTime += Time.deltaTime;
            deltaTime /= 2.0f;
            fps = 1.0f / deltaTime;

            if (fps > highestFps)
                highestFps = fps;

            if (fps < lowestFps)
                lowestFps = fps;

            fpsText.text = "FPS: " + fps.ToString("#.##");
            highestText.text = "Highest: " + highestFps.ToString("#.##");
            lowestText.text = "Lowest: " + lowestFps.ToString("#.##");
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator SetHighestAndLowest()
    {
        yield return new WaitForSeconds(.5f);
        highestFps = 0;
         lowestFps = 100;
    }
}
