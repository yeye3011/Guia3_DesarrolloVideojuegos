using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CubeDragController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private InteractionModeController modeController;

    private Plane dragPlane;
    private Vector3 grabOffset;

    private bool dragging = false;
    private int dragFingerId = -1;

    private Rigidbody draggedBody;

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
        // Si no estamos en modo Drag,
        // no hacemos nada.
        if (modeController == null)
            return;

        if (!modeController.IsDragMode)
        {
            if (dragging)
                EndDrag();

            return;
        }

        foreach (var touch in Touch.activeTouches)
        {
            int fingerId = touch.finger.index;

            // -------------------------------------------------
            // INICIAR DRAG
            // -------------------------------------------------

            if (touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Began)
            {
                TryStartDrag(touch, fingerId);
            }

            // -------------------------------------------------
            // ACTUALIZAR DRAG
            // -------------------------------------------------

            if (dragging &&
                fingerId == dragFingerId &&
                (touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Moved ||
                 touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Stationary))
            {
                UpdateDrag(touch);
            }

            // -------------------------------------------------
            // TERMINAR DRAG
            // -------------------------------------------------

            if (dragging &&
                fingerId == dragFingerId &&
                (touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Ended ||
                 touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Canceled))
            {
                EndDrag();
            }
        }
    }

    // =====================================================
    // INICIAR DRAG
    // =====================================================

    private void TryStartDrag(
        Touch touch,
        int fingerId)
    {
        // -------------------------------------------------
        // SOLO USAMOS LA MITAD DERECHA
        // -------------------------------------------------

        if (touch.screenPosition.x <
            Screen.width * 0.5f)
        {
            return;
        }

        // -------------------------------------------------
        // VERIFICAR REFERENCIAS
        // -------------------------------------------------

        if (interactionManager == null)
        {
            Debug.LogError(
                "InteractionManager no está asignado."
            );

            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError(
                "Player Camera no está asignada."
            );

            return;
        }

        // -------------------------------------------------
        // OBTENER CUBO SELECCIONADO
        // -------------------------------------------------

        CubeInteractable cube =
            interactionManager.SelectedCube;

        if (cube == null)
        {
            Debug.Log(
                "No hay cubo seleccionado."
            );

            return;
        }

        // -------------------------------------------------
        // COMPROBAR QUE EL CUBO ESTÁ AGARRADO
        // -------------------------------------------------

        if (!interactionManager.IsHoldingCube)
        {
            Debug.Log(
                "El cubo no está agarrado."
            );

            return;
        }

        // -------------------------------------------------
        // OBTENER RIGIDBODY
        // -------------------------------------------------

        draggedBody =
            cube.GetComponent<Rigidbody>();

        if (draggedBody == null)
        {
            Debug.LogWarning(
                "El cubo no tiene Rigidbody."
            );

            return;
        }

        // -------------------------------------------------
        // DESACTIVAR FÍSICA MIENTRAS ARRASTRAMOS
        // -------------------------------------------------

        draggedBody.isKinematic = true;

        // -------------------------------------------------
        // CREAR PLANO DE DRAG
        // -------------------------------------------------

        dragPlane = new Plane(
            Vector3.up,
            cube.transform.position
        );

        // -------------------------------------------------
        // CREAR RAYO DESDE EL DEDO
        // -------------------------------------------------

        Ray ray =
            playerCamera.ScreenPointToRay(
                touch.screenPosition
            );

        // -------------------------------------------------
        // INTERSECCIÓN RAYO / PLANO
        // -------------------------------------------------

        if (dragPlane.Raycast(
            ray,
            out float distance
        ))
        {
            Vector3 hitPoint =
                ray.GetPoint(distance);

            // Guardamos la diferencia entre
            // el punto tocado y el centro del cubo.
            // Esto evita que el cubo salte.
            grabOffset =
                cube.transform.position -
                hitPoint;

            dragging = true;

            dragFingerId = fingerId;

            Debug.Log(
                "Drag iniciado: " +
                cube.name
            );
        }
        else
        {
            // Si no pudimos encontrar el plano,
            // dejamos la física como estaba.
            draggedBody.isKinematic = false;
            draggedBody = null;
        }
    }

    // =====================================================
    // ACTUALIZAR DRAG
    // =====================================================

    private void UpdateDrag(Touch touch)
    {
        if (interactionManager == null)
        {
            EndDrag();
            return;
        }

        CubeInteractable cube =
            interactionManager.SelectedCube;

        if (cube == null)
        {
            EndDrag();
            return;
        }

        Ray ray =
            playerCamera.ScreenPointToRay(
                touch.screenPosition
            );

        if (dragPlane.Raycast(
            ray,
            out float distance
        ))
        {
            Vector3 targetPosition =
                ray.GetPoint(distance)
                + grabOffset;

            // -------------------------------------------------
            // MANTENER LA ALTURA DEL CUBO
            // -------------------------------------------------

            targetPosition.y =
                cube.transform.position.y;

            cube.transform.position =
                targetPosition;
        }
    }

    // =====================================================
    // TERMINAR DRAG
    // =====================================================

    private void EndDrag()
    {
        dragging = false;

        dragFingerId = -1;

        /*
         * IMPORTANTE:
         *
         * Aquí NO ponemos:
         *
         * draggedBody.isKinematic = false;
         *
         * porque terminar el Drag NO significa
         * soltar el cubo.
         *
         * El cubo sigue agarrado.
         *
         * El usuario tendrá que hacer TAP
         * para soltarlo.
         */

        if (modeController != null)
        {
            modeController.DeactivateMode();
        }

        Debug.Log(
            "Drag terminado. " +
            "El cubo sigue agarrado."
        );
    }
}