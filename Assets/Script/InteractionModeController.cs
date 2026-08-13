using UnityEngine;

public class InteractionModeController : MonoBehaviour
{
    public enum InteractionMode
    {
        None,
        Drag,
        Swipe
    }

    public InteractionMode CurrentMode { get; private set; }
        = InteractionMode.None;

    public bool IsDragMode =>
        CurrentMode == InteractionMode.Drag;

    public bool IsSwipeMode =>
        CurrentMode == InteractionMode.Swipe;

    public void ActivateDragMode()
    {
        CurrentMode = InteractionMode.Drag;

        Debug.Log("Modo DRAG activado");
    }

    public void ActivateSwipeMode()
    {
        CurrentMode = InteractionMode.Swipe;

        Debug.Log("Modo SWIPE activado");
    }

    public void DeactivateMode()
    {
        CurrentMode = InteractionMode.None;

        Debug.Log("Modo de interacción desactivado");
    }
}