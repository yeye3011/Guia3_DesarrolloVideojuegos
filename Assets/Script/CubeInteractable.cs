using UnityEngine;

public class CubeInteractable : MonoBehaviour
{
    [Header("Luz de selección")]
    [SerializeField] private Light selectionLight;

    [Header("Configuración")]
    [SerializeField] private float blinkSpeed = 5f;

    private bool isSelected = false;

    private float baseIntensity;

    private void Awake()
    {
        if (selectionLight != null)
        {
            baseIntensity = selectionLight.intensity;
            selectionLight.enabled = false;
        }
    }

    private void Update()
    {
        if (!isSelected || selectionLight == null)
            return;

        float intensity =
            (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;

        selectionLight.intensity =
            Mathf.Lerp(0.2f, baseIntensity, intensity);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectionLight != null)
        {
            selectionLight.enabled = selected;

            if (!selected)
                selectionLight.intensity = baseIntensity;
        }
    }
}