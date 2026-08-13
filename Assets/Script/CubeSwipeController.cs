using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CubeSwipeController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private InteractionModeController modeController;

    [Header("Swipe")]
    [SerializeField] private float minSwipePixels = 100f;
    [SerializeField] private float impulseScale = 0.015f;
    [SerializeField] private float maxImpulse = 8f;

    private Vector2 startPosition;

    private int swipeFingerId = -1;

    private bool tracking = false;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (!modeController.IsSwipeMode)
            return;

        foreach (var touch in Touch.activeTouches)
        {
            int fingerId = touch.finger.index;

            if (touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Began)
            {
                TryStartSwipe(touch, fingerId);
            }

            if (tracking &&
                fingerId == swipeFingerId &&
                touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Ended)
            {
                FinishSwipe(touch);
            }

            if (tracking &&
                fingerId == swipeFingerId &&
                touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                CancelSwipe();
            }
        }
    }

    private void TryStartSwipe(
        Touch touch,
        int fingerId)
    {
        // Swipe solamente en la mitad derecha
        if (touch.screenPosition.x <
            Screen.width * 0.5f)
        {
            return;
        }

        CubeInteractable cube =
            interactionManager.SelectedCube;

        if (cube == null)
            return;

        if (!interactionManager.IsHoldingCube)
            return;

        startPosition =
            touch.screenPosition;

        swipeFingerId =
            fingerId;

        tracking = true;
    }

    private void FinishSwipe(Touch touch)
    {
        Vector2 delta =
            touch.screenPosition -
            startPosition;

        tracking = false;
        swipeFingerId = -1;

        // Evitar confundir Tap con Swipe
        if (delta.magnitude < minSwipePixels)
        {
            modeController.DeactivateMode();
            return;
        }

        CubeInteractable cube =
            interactionManager.SelectedCube;

        if (cube == null)
        {
            modeController.DeactivateMode();
            return;
        }

        Rigidbody body =
            cube.GetComponent<Rigidbody>();

        if (body == null)
        {
            modeController.DeactivateMode();
            return;
        }

        // Dirección del gesto
        Vector3 direction =
            new Vector3(
                delta.x,
                0f,
                delta.y
            ).normalized;

        // Fuerza proporcional al swipe
        float strength =
            Mathf.Clamp(
                delta.magnitude *
                impulseScale,
                1f,
                maxImpulse
            );

        // Asegurar que la física esté activa
        body.isKinematic = false;

        // Lanzamiento
        body.AddForce(
            direction * strength,
            ForceMode.Impulse
        );

        Debug.Log(
            "Cubo lanzado. Fuerza: " +
            strength
        );

        modeController.DeactivateMode();
    }

    private void CancelSwipe()
    {
        tracking = false;
        swipeFingerId = -1;

        modeController.DeactivateMode();
    }
}