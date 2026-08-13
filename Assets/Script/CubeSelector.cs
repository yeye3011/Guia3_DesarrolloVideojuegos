using UnityEngine;

public class CubeSelector : MonoBehaviour
{
    [Header("Cámara")]
    [SerializeField] private Camera playerCamera;

    [Header("Distancia")]
    [SerializeField] private float interactionDistance = 5f;

    private CubeInteractable currentCube;

    public CubeInteractable CurrentCube => currentCube;

    private void Update()
    {
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance
        ))
        {
            CubeInteractable cube =
                hit.collider.GetComponent<CubeInteractable>();

            if (cube != null)
            {
                currentCube = cube;
                return;
            }
        }

        currentCube = null;
    }

    public void HighlightCurrentCube(bool value)
    {
        if (currentCube != null)
        {
            currentCube.SetSelected(value);
        }
    }
}