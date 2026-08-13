using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private CubeSelector cubeSelector;

    private CubeInteractable heldCube;

    public void Tap()
    {
        CubeInteractable selectedCube =
            cubeSelector.CurrentCube;

        if (selectedCube == null)
            return;

        if (heldCube == null)
        {
            heldCube = selectedCube;

            Debug.Log(
                "Cubo agarrado: " +
                heldCube.name
            );
        }
        else if (heldCube == selectedCube)
        {
            heldCube = null;

            Debug.Log(
                "Cubo soltado"
            );
        }
    }
}