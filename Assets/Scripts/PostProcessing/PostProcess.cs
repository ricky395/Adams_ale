
// Author: Ricardo Roldán

using UnityEngine;
using UnityEngine.PostProcessing;

/// <summary>
/// Clase que controla valores de postprocesado de la cámara
/// </summary>
public class PostProcess : MonoBehaviour
{
    /// <summary>
    /// Objeto de postprocesado
    /// </summary>
    PostProcessingProfile postPB;

    /// <summary>
    /// Opciones iniciales
    /// </summary>
    DepthOfFieldModel.Settings init_depth_sett;

    /// <summary>
    /// Opciones de profundidad de campo de la cámara
    /// </summary>
    DepthOfFieldModel.Settings depth_sett;

    //WaitForSeconds seconds = new WaitForSeconds(0.01f);

    public static PostProcess Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        PostProcessingBehaviour originalPPB = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PostProcessingBehaviour>();

        postPB = Instantiate(originalPPB.profile);
        originalPPB.profile = postPB;

        init_depth_sett = postPB.depthOfField.settings;
    }

    /// <summary>
    /// Cambia la distancia de enfoque de la cámara
    /// </summary>
    /// <param name="focus_dist"></param>
    public void ChangeDepthFocusDist(float focus_dist)
    {
        depth_sett.focusDistance = focus_dist;
        UpdateDepth(depth_sett);
    }

    /// <summary>
    /// Cambia la apertura de la cámara
    /// </summary>
    /// <param name="aperture"></param>
    public void ChangeDepthAperture(float aperture)
    {
        depth_sett.aperture = aperture;
        UpdateDepth(depth_sett);
    }

    /// <summary>
    /// Cambia la distancia focal de la cámara
    /// </summary>
    /// <param name="focal_length"></param>
    public void ChangeDepthFocalLength(float focal_length)
    {
        depth_sett.focalLength = focal_length;
        UpdateDepth(depth_sett);
    }

    /// <summary>
    /// Actualiza los parámetros de profundidad a como estaban por defecto
    /// </summary>
    public void ResetDepth()
    {
        UpdateDepth(init_depth_sett);
    }

    //IEnumerator LerpFocusDist(DepthOfFieldModel.Settings settings, float originalValue, float newValue)
    //{        
    //    while(Mathf.Approximately(originalValue, newValue))
    //    {
    //        originalValue = Mathf.Lerp(originalValue, newValue, 0.1f);
    //        settings.focusDistance = originalValue;
    //        UpdateDepth(settings);
    //        yield return seconds;
    //    }
    //}

    /// <summary>
    /// Actualiza
    /// </summary>
    /// <param name="settings">Opciones de profundidad de campo</param>
    void UpdateDepth(DepthOfFieldModel.Settings settings)
    {
        postPB.depthOfField.settings = settings;
    }
}
