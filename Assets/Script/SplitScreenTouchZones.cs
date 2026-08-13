using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch =
    UnityEngine.InputSystem.EnhancedTouch.Touch;

public class SplitScreenTouchZones : MonoBehaviour
{
    // =====================================================
    // SALIDA DE CÁMARA
    // =====================================================

    public Vector2 LookDelta
    {
        get;
        private set;
    }

    // =====================================================
    // VARIABLES
    // =====================================================

    private Vector2 previousPosition;

    private int cameraFinger = -1;

    // =====================================================
    // ENHANCED TOUCH
    // =====================================================

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        LookDelta = Vector2.zero;

        foreach (var touch in Touch.activeTouches)
        {
            int fingerId =
                touch.finger.index;

            bool isRightSide =
                touch.screenPosition.x >=
                Screen.width * 0.5f;

            // ---------------------------------------------
            // NUEVO DEDO PARA CÁMARA
            // ---------------------------------------------

            if (touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (isRightSide &&
                    cameraFinger < 0)
                {
                    cameraFinger =
                        fingerId;

                    previousPosition =
                        touch.screenPosition;
                }
            }

            // ---------------------------------------------
            // MOVIMIENTO DE CÁMARA
            // ---------------------------------------------

            if (fingerId == cameraFinger)
            {
                if (touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    LookDelta =
                        touch.screenPosition -
                        previousPosition;

                    previousPosition =
                        touch.screenPosition;
                }
            }

            // ---------------------------------------------
            // LIBERAR DEDO
            // ---------------------------------------------

            if (touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Ended ||
                touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                if (fingerId == cameraFinger)
                {
                    cameraFinger = -1;
                    LookDelta = Vector2.zero;
                }
            }
        }
    }
}