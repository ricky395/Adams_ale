using UnityEngine;
using UnityEngine.UI;

public class Information : MonoBehaviour
{
    [SerializeField]
    GameObject commingShip;

    #region Texts
    [SerializeField]
    Text successText;

    [SerializeField]
    Text failureText;

    public Text SuccessText
    {
        get { return successText; }
        set { successText = value; }
    }

    public Text FailureText
    {
        get { return failureText; }
        set { failureText = value; }
    }

    string successLabel = "Success: ",
           failureLabel = "Failure: ";

    public string SuccessLabel
    {
        get { return successLabel; }
    }

    public string FailureLabel
    {
        get { return failureLabel; }
    }

    int successCount = 0,
        failureCount = 0;

    public int SuccessCount
    {
        get { return successCount; }
        set { successCount = value; }
    }

    public int FailureCount
    {
        get { return failureCount; }
        set { failureCount = value; }
    }

    #endregion

    SplineWalker activeShip;

    public SplineWalker ActiveShip
    {
        get { return activeShip; }
        set { activeShip = value; }
    }

    public static Information Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    public void SetShipDestination(SplineDestination dest)
    {
        activeShip.GoToDestination(dest);
    }

    public bool NoShipSpawned()
    {
        if (commingShip.transform.childCount == 0)
            return true;

        return false;
    }
}
