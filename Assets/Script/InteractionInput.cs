using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionInput : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField]
    private InteractionManager interactionManager;

    [Header("Input Actions")]
    [SerializeField]
    private InputActionReference grabAction;

    private void OnEnable()
    {
        if (grabAction == null)
            return;

        grabAction.action.Enable();
        grabAction.action.performed += OnGrab;
    }

    private void OnDisable()
    {
        if (grabAction == null)
            return;

        grabAction.action.performed -= OnGrab;
        grabAction.action.Disable();
    }

    private void OnGrab(
        InputAction.CallbackContext context)
    {
        if (interactionManager == null)
        {
            Debug.LogError(
                "InteractionManager no está asignado."
            );

            return;
        }

        if (Touchscreen.current == null)
        {
            return;
        }

        Vector2 touchPosition =
            Touchscreen.current
                .primaryTouch
                .position
                .ReadValue();

        Debug.Log(
            "TAP en: " + touchPosition
        );

        interactionManager.Tap(
            touchPosition
        );
    }
}