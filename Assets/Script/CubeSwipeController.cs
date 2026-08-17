using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch =
    UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CubeSwipeController : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private InteractionModeController modeController;

    // =====================================================
    // CONFIGURACIÓN SWIPE
    // =====================================================

    [Header("Configuración Swipe")]

    [Tooltip("Distancia mínima del gesto para considerarlo Swipe.")]
    [SerializeField] private float minSwipePixels = 70f;

    [Tooltip("Multiplicador de fuerza según la distancia del gesto.")]
    [SerializeField] private float impulseScale = 0.035f;

    [Tooltip("Fuerza mínima de lanzamiento.")]
    [SerializeField] private float minImpulse = 3.5f;

    [Tooltip("Fuerza máxima de lanzamiento.")]
    [SerializeField] private float maxImpulse = 12f;

    [Tooltip("Cuánto influye el gesto vertical en la elevación.")]
    [SerializeField] private float verticalInfluence = 0.65f;

    [Tooltip("Impulso vertical mínimo.")]
    [SerializeField] private float minimumUpwardForce = 0.15f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private Vector2 startPosition;

    private int swipeFingerId = -1;

    private bool tracking = false;

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
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (modeController == null)
            return;

        if (!modeController.IsSwipeMode)
            return;

        foreach (var touch in Touch.activeTouches)
        {
            int fingerId =
                touch.finger.index;

            // INICIAR
            if (!tracking &&
                touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Began)
            {
                TryStartSwipe(
                    touch,
                    fingerId
                );
            }

            // TERMINAR
            if (tracking &&
                fingerId == swipeFingerId &&
                touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Ended)
            {
                FinishSwipe(touch);
            }

            // CANCELAR
            if (tracking &&
                fingerId == swipeFingerId &&
                touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                CancelSwipe();
            }
        }
    }

    // =====================================================
    // INICIAR SWIPE
    // =====================================================

    private void TryStartSwipe(
        Touch touch,
        int fingerId)
    {
        // =================================================
        // SWIPE SOLO EN LA MITAD DERECHA
        // =================================================

        /*
         * La mitad izquierda queda reservada
         * para el joystick.
         */

        if (touch.screenPosition.x <
            Screen.width * 0.5f)
        {
            return;
        }

        // -------------------------------------------------
        // REFERENCIAS
        // -------------------------------------------------

        if (interactionManager == null ||
            playerCamera == null ||
            modeController == null)
        {
            Debug.LogWarning(
                "Faltan referencias en CubeSwipeController."
            );

            return;
        }

        // -------------------------------------------------
        // CERDITO
        // -------------------------------------------------

        CubeInteractable cube =
            interactionManager.SelectedCube;

        if (cube == null)
        {
            Debug.Log(
                "Swipe cancelado: no hay cerdito seleccionado."
            );

            modeController.DeactivateMode();

            return;
        }

        if (!interactionManager.IsHoldingCube)
        {
            Debug.Log(
                "Swipe cancelado: no hay cerdito agarrado."
            );

            modeController.DeactivateMode();

            return;
        }

        // -------------------------------------------------
        // RESERVAR DEDO
        // -------------------------------------------------

        if (!modeController.TryReserveFinger(
            fingerId))
        {
            return;
        }

        startPosition =
            touch.screenPosition;

        swipeFingerId =
            fingerId;

        tracking =
            true;

        Debug.Log(
            "Swipe iniciado."
        );
    }

    // =====================================================
    // TERMINAR SWIPE
    // =====================================================

    private void FinishSwipe(
        Touch touch)
    {
        Vector2 endPosition =
            touch.screenPosition;

        Vector2 delta =
            endPosition -
            startPosition;

        tracking =
            false;

        swipeFingerId =
            -1;

        // -------------------------------------------------
        // SWIPE DEMASIADO CORTO
        // -------------------------------------------------

        if (delta.magnitude <
            minSwipePixels)
        {
            Debug.Log(
                "Swipe demasiado corto."
            );

            modeController.DeactivateMode();

            return;
        }

        // -------------------------------------------------
        // OBTENER CERDITO
        // -------------------------------------------------

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
            Debug.LogWarning(
                "El cerdito no tiene Rigidbody."
            );

            modeController.DeactivateMode();

            return;
        }

        // =================================================
        // LIBERAR DE PILA
        // =================================================

        PigStabilizer stabilizer =
            cube.GetComponent<PigStabilizer>();

        if (stabilizer != null)
        {
            stabilizer.ReleaseStack();
        }

        // =================================================
        // DIRECCIÓN SEGÚN CÁMARA
        // =================================================

        Vector3 cameraForward =
            playerCamera.transform.forward;

        cameraForward.y =
            0f;

        cameraForward.Normalize();

        Vector3 cameraRight =
            playerCamera.transform.right;

        cameraRight.y =
            0f;

        cameraRight.Normalize();

        Vector2 normalizedDelta =
            delta.normalized;

        Vector3 horizontalDirection =
            cameraForward *
            normalizedDelta.y +
            cameraRight *
            normalizedDelta.x;

        // =================================================
        // COMPONENTE VERTICAL
        // =================================================

        float upwardAmount =
            Mathf.Max(
                minimumUpwardForce,
                normalizedDelta.y *
                verticalInfluence
            );

        Vector3 launchDirection =
            horizontalDirection +
            Vector3.up *
            upwardAmount;

        launchDirection.Normalize();

        // =================================================
        // FUERZA
        // =================================================

        float strength =
            Mathf.Clamp(
                delta.magnitude *
                impulseScale,
                minImpulse,
                maxImpulse
            );

        // =================================================
        // ACTIVAR FÍSICA
        // =================================================

        body.isKinematic =
            false;

        body.useGravity =
            true;

        body.linearVelocity =
            Vector3.zero;

        body.angularVelocity =
            Vector3.zero;

        // =================================================
        // LANZAMIENTO
        // =================================================

        body.AddForce(
            launchDirection *
            strength,
            ForceMode.Impulse
        );

        // =================================================
        // ROTACIÓN DURANTE VUELO
        // =================================================

        Vector3 torque =
            new Vector3(
                -normalizedDelta.y,
                normalizedDelta.x,
                -normalizedDelta.x
            );

        body.AddTorque(
            torque *
            strength *
            0.25f,
            ForceMode.Impulse
        );

        Debug.Log(
            "Cerdito lanzado. Fuerza: " +
            strength
        );

        // =================================================
        // DESACTIVAR SWIPE
        // =================================================

        modeController.DeactivateMode();
    }

    // =====================================================
    // CANCELAR SWIPE
    // =====================================================

    private void CancelSwipe()
    {
        tracking =
            false;

        swipeFingerId =
            -1;

        if (modeController != null)
        {
            modeController.DeactivateMode();
        }

        Debug.Log(
            "Swipe cancelado."
        );
    }
}