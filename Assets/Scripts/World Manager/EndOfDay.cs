using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndOfDay : MonoBehaviour
{
    [SerializeField]
    AnimationCurve fadeOutCurve;

    [SerializeField]
    Image blackImage;

    [SerializeField]
    Text finalText;

    public static EndOfDay Instance;

    float evaluation = 0;
    
	void Start ()
    {
        if (Instance != null)
            Destroy(this.gameObject);

        Instance = this;
	}
	
	public void FadeOut()
    {
        StartCoroutine(FadeCoroutine());
    }

    IEnumerator FadeCoroutine()
    {
        while (evaluation <= 1)
        {
            float alpha = fadeOutCurve.Evaluate(evaluation);
            blackImage.color = new Color(0, 0, 0, alpha);

            evaluation += Time.deltaTime * 0.5f;

            yield return new WaitForEndOfFrame();
        }
        
        blackImage.color = new Color(0, 0, 0, 1);

        yield return new WaitForSeconds(1.5f);

        int successes = Information.Instance.SuccessCount;
        int  failures = Information.Instance.FailureCount;

        finalText.text = "Successes: " + Information.Instance.SuccessCount.ToString() + "\n\n" +
                          "Failures: " + Information.Instance.FailureCount.ToString() + "\n\n";

        if (successes > 1 && failures == 0)
        {
            finalText.text += "\nThe city is happy with your work.\nEverything is as great as always";
        }

        else if (failures > 1 && successes == 0)
        {
            finalText.text += "\nYou have been a disappointment to the city.\nYou have been fired.";
        }

        else
        {
            finalText.text += "\nDifferent oppinions start to merge into the society.\nThe view of the city sovereignty is starting to tremble.";
        }

        finalText.enabled = true;
    }
    
}
