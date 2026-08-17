using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch =
    UnityEngine.InputSystem.EnhancedTouch.Touch;

public class SplitScreenTouchZones : MonoBehaviour
{
    // =====================================================
    // SALIDA PARA LA CÁMARA
    // =====================================================

    public Vector2 LookDelta
    {
        get;
        private set;
    }

    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Interacción")]
    [SerializeField]
    private InteractionModeController modeController;

    // =====================================================
    // CONFIGURACIÓN
    // =====================================================

    [Header("Zona de cámara")]

    [Tooltip(
        "Porcentaje de pantalla desde donde comienza " +
        "la zona de cámara."
    )]
    [Range(0.5f, 0.9f)]
    [SerializeField]
    private float cameraZoneStart = 0.5f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private Vector2 previousPosition;

    private int cameraFinger = -1;

    // =====================================================
    // ENABLE / DISABLE
    // =====================================================

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();

        cameraFinger = -1;

        LookDelta =
            Vector2.zero;
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        // Cada frame comenzamos sin movimiento.
        LookDelta =
            Vector2.zero;

        foreach (var touch in Touch.activeTouches)
        {
            int fingerId =
                touch.finger.index;

            // =================================================
            // 1. IGNORAR DEDO DE INTERACCIÓN
            // =================================================

            /*
             * Si CubeDragController o CubeSwipeController
             * reservó este dedo, este script NO puede
             * utilizarlo para mover la cámara.
             */

            if (modeController != null &&
                modeController.IsInteractionFinger(
                    fingerId))
            {
                // Si este dedo antes pertenecía
                // a la cámara, lo liberamos.
                if (cameraFinger ==
                    fingerId)
                {
                    cameraFinger =
                        -1;

                    LookDelta =
                        Vector2.zero;
                }

                continue;
            }

            // =================================================
            // 2. COMPROBAR ZONA DE CÁMARA
            // =================================================

            bool isCameraZone =
                touch.screenPosition.x >=
                Screen.width *
                cameraZoneStart;

            // =================================================
            // 3. ASIGNAR DEDO A CÁMARA
            // =================================================

            if (touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Began)
            {
                /*
                 * Cuando Drag o Swipe está activo,
                 * estamos esperando que uno de los dedos
                 * sea utilizado para esa interacción.

                 * Por seguridad, la cámara NO toma nuevos
                 * dedos mientras esperamos el gesto.
                 */

                bool interactionModeActive =
                    modeController != null &&
                    (
                        modeController.IsDragMode ||
                        modeController.IsSwipeMode
                    );

                if (isCameraZone &&
                    cameraFinger < 0 &&
                    !interactionModeActive)
                {
                    cameraFinger =
                        fingerId;

                    previousPosition =
                        touch.screenPosition;

                    continue;
                }
            }

            // =================================================
            // 4. ACTUALIZAR CÁMARA
            // =================================================

            if (fingerId ==
                cameraFinger)
            {
                // ---------------------------------------------
                // MOVIMIENTO
                // ---------------------------------------------

                if (touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    Vector2 currentPosition =
                        touch.screenPosition;

                    LookDelta =
                        currentPosition -
                        previousPosition;

                    previousPosition =
                        currentPosition;
                }

                // ---------------------------------------------
                // DEDO QUIETO
                // ---------------------------------------------

                if (touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    LookDelta =
                        Vector2.zero;
                }

                // ---------------------------------------------
                // LIBERAR
                // ---------------------------------------------

                if (touch.phase ==
                        UnityEngine.InputSystem.TouchPhase.Ended ||
                    touch.phase ==
                        UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    cameraFinger =
                        -1;

                    LookDelta =
                        Vector2.zero;
                }
            }
        }

        // =====================================================
        // SEGURIDAD
        // =====================================================

        /*
         * Si el dedo de cámara desapareció por cualquier
         * motivo, evitamos conservar movimiento residual.
         */

        if (cameraFinger >= 0)
        {
            bool cameraFingerStillExists =
                false;

            foreach (var touch in Touch.activeTouches)
            {
                if (touch.finger.index ==
                    cameraFinger)
                {
                    cameraFingerStillExists =
                        true;

                    break;
                }
            }

            if (!cameraFingerStillExists)
            {
                cameraFinger =
                    -1;

                LookDelta =
                    Vector2.zero;
            }
        }
    }
}