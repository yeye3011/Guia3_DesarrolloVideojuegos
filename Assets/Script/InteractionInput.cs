using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionInput : MonoBehaviour
{
    [SerializeField] private InteractionManager interactionManager;

    [SerializeField] private InputActionReference grabAction;

    private void OnEnable()
    {
        grabAction.action.Enable();
        grabAction.action.performed += OnGrab;
    }

    private void OnDisable()
    {
        grabAction.action.performed -= OnGrab;
        grabAction.action.Disable();
    }

    private void OnGrab(InputAction.CallbackContext context)
    {
        interactionManager.Tap();
    }
}   