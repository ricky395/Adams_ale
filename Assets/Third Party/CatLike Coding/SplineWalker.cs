using UnityEngine;

public class SplineWalker : MonoBehaviour
{

    [SerializeField]
    float speed;

    [SerializeField]
    bool commingShip;

    [SerializeField]
    bool lookForward;

    [SerializeField]
    SplineWalker otherShip;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClip goodSound;

    [SerializeField]
    AudioClip badSound;

    BezierSpline spline;

    protected float progress;

    bool isActive;

    Vector3 entranceOffset;

    Vector3 entrance;

    Vector3 docksPoint;

    public struct ShipInfo
    {
        public ShipInfo(int _lCount, int _wCount, float _partLength, float _partWidth)
        {
            lCount = _lCount;
            wCount = _wCount;
            partLength = _partLength;
            partWidth = _partWidth;
        }

        public int lCount, wCount;
        public float partLength, partWidth;
    }

    ShipInfo ship;

    public ShipInfo Ship
    {
        get { return ship; }
        set { ship = value; }
    }

    bool niceContent;

    public bool NiceContent
    {
        get { return niceContent; }
        set { niceContent = value; }
    }


    private void Start()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    protected void Update()
    {
        if (isActive)
        {
            progress += Time.deltaTime * speed;

            Vector3 position = spline.GetPoint(progress);
            transform.position = position + entranceOffset;

            if (lookForward)
            {
                transform.LookAt(position + spline.GetDirection(progress) + entranceOffset);
            }

            if (progress >= 1)
            {
                isActive = false;
                progress = 0;

                if (!commingShip)
                {
                    SetScore();
                    ShipsManager.Instance.DespawnGoingShip();
                    Flag.Instance.AnimationEnd();

                    if (Information.Instance.ActiveShip == this)
                        Information.Instance.ActiveShip = null;
                }
            }
        }
    }

    void SetScore()
    {
        if (niceContent)
        {
            if (spline.name.Contains("City"))
            {
                Information.Instance.SuccessText.text = Information.Instance.SuccessLabel + (++Information.Instance.SuccessCount);
                audioSource.clip = goodSound;
            }
            else
            {
                Information.Instance.FailureText.text = Information.Instance.FailureLabel + (++Information.Instance.FailureCount);
                audioSource.clip = badSound;
            }
        }
        else
        {
            if (spline.name.Contains("Out"))
            {
                Information.Instance.SuccessText.text = Information.Instance.SuccessLabel + (++Information.Instance.SuccessCount);
                audioSource.clip = goodSound;
            }
            else
            {
                Information.Instance.FailureText.text = Information.Instance.FailureLabel + (++Information.Instance.FailureCount);
                audioSource.clip = badSound;
            }
        }

        audioSource.Play();
    }

    void Init()
    {
        docksPoint = spline.GetControlPoint(spline.ControlPointCount - 1);

        entrance = new Vector3(transform.position.x + ship.lCount * .5f * ship.partLength,
                                docksPoint.y,
                                transform.position.z + ship.wCount * ship.partWidth);

        otherShip.docksPoint = spline.GetControlPoint(spline.ControlPointCount - 1);
        otherShip.entrance = entrance;
    }

    public void SpawnShip(BezierSpline _splineRoute)
    {
        Information.Instance.ActiveShip = this;
        spline = _splineRoute;

        if (commingShip)
            Init();

        entranceOffset = entrance;

        isActive = true;
    }

    public void GoToDestination(SplineDestination _destination)
    {
        if (commingShip && !isActive)
        {
            ShipsManager.Instance.SetShipToGoing(_destination);
        }
    }
}