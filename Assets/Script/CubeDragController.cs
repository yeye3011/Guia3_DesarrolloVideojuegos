using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch =
    UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CubeDragController : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private InteractionModeController modeController;

    // =====================================================
    // CONFIGURACIÓN DRAG
    // =====================================================

    [Header("Configuración Drag")]

    [SerializeField]
    private float maxDragDistance = 5f;

    [SerializeField]
    private float minDragDistance = 1.2f;

    [SerializeField]
    private float dragMoveSpeed = 6f;

    [Tooltip("Pequeña elevación al comenzar Drag para separarlo de una pila.")]
    [SerializeField]
    private float dragLiftAmount = 0.15f;

    // =====================================================
    // SUELO
    // =====================================================

    [Header("Suelo")]

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float groundCheckHeight = 6f;

    [SerializeField]
    private float groundGap = 0.05f;

    // =====================================================
    // APILADO
    // =====================================================

    [Header("Apilado")]

    [Tooltip("Distancia horizontal necesaria para detectar otro cerdito.")]
    [SerializeField]
    private float stackDetectionDistance = 1.4f;

    [Tooltip("Separación vertical pequeña entre los cerditos.")]
    [SerializeField]
    private float stackGap = 0.03f;

    // =====================================================
    // ESTADO
    // =====================================================

    private Plane dragPlane;
    private Vector3 grabOffset;

    private bool dragging = false;
    private int dragFingerId = -1;

    private Rigidbody draggedBody;
    private CubeInteractable draggedCube;
    private Collider draggedCollider;

    private CubeInteractable currentSupportPig;

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

        if (!modeController.IsDragMode)
            return;

        foreach (var touch in Touch.activeTouches)
        {
            int fingerId =
                touch.finger.index;

            // INICIAR
            if (!dragging &&
                touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Began)
            {
                TryStartDrag(
                    touch,
                    fingerId
                );
            }

            // MOVER
            if (dragging &&
                fingerId == dragFingerId &&
                (touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Moved ||
                 touch.phase ==
                    UnityEngine.InputSystem.TouchPhase.Stationary))
            {
                UpdateDrag(touch);
            }

            // TERMINAR
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
        // Drag solamente en mitad derecha.
        if (touch.screenPosition.x <
            Screen.width * 0.5f)
        {
            return;
        }

        if (playerCamera == null ||
            playerTransform == null ||
            interactionManager == null ||
            modeController == null)
        {
            Debug.LogError(
                "CubeDragController: faltan referencias."
            );

            return;
        }

        CubeInteractable cube =
            interactionManager.SelectedCube;

        if (cube == null)
        {
            Debug.Log(
                "DRAG: no hay cerdito seleccionado."
            );

            modeController.DeactivateMode();

            return;
        }

        if (!interactionManager.IsHoldingCube)
        {
            Debug.Log(
                "DRAG: el cerdito no está agarrado."
            );

            modeController.DeactivateMode();

            return;
        }

        // =================================================
        // RESERVAR DEDO
        // =================================================

        if (!modeController.TryReserveFinger(
            fingerId))
        {
            return;
        }

        draggedCube = cube;

        draggedBody =
            draggedCube.GetComponent<Rigidbody>();

        draggedCollider =
            draggedCube.GetComponent<Collider>();

        if (draggedBody == null ||
            draggedCollider == null)
        {
            Debug.LogError(
                draggedCube.name +
                " necesita Rigidbody y Collider."
            );

            CancelDrag();
            return;
        }

        // =================================================
        // LIBERAR PILA ANTERIOR
        // =================================================

        PigStabilizer stabilizer =
            draggedCube.GetComponent<PigStabilizer>();

        if (stabilizer != null)
        {
            stabilizer.ReleaseStack();
        }

        // =================================================
        // DETENER FÍSICA
        // =================================================

        draggedBody.linearVelocity =
            Vector3.zero;

        draggedBody.angularVelocity =
            Vector3.zero;

        draggedBody.isKinematic =
            true;

        draggedBody.useGravity =
            false;

        // =================================================
        // PEQUEÑA ELEVACIÓN INICIAL
        // =================================================

        Vector3 liftedPosition =
            draggedBody.position;

        liftedPosition.y +=
            dragLiftAmount;

        draggedBody.position =
            liftedPosition;

        // =================================================
        // CREAR PLANO DE DRAG
        // =================================================

        dragPlane =
            new Plane(
                Vector3.up,
                draggedBody.position
            );

        Ray ray =
            playerCamera.ScreenPointToRay(
                touch.screenPosition
            );

        if (!dragPlane.Raycast(
            ray,
            out float distance))
        {
            CancelDrag();
            return;
        }

        Vector3 hitPoint =
            ray.GetPoint(distance);

        grabOffset =
            draggedBody.position -
            hitPoint;

        dragging = true;

        dragFingerId =
            fingerId;

        currentSupportPig =
            null;

        Debug.Log(
            "DRAG INICIADO: " +
            draggedCube.name
        );
    }

    // =====================================================
    // ACTUALIZAR DRAG
    // =====================================================

    private void UpdateDrag(
        Touch touch)
    {
        if (draggedCube == null ||
            draggedBody == null ||
            draggedCollider == null)
        {
            CancelDrag();
            return;
        }

        Ray ray =
            playerCamera.ScreenPointToRay(
                touch.screenPosition
            );

        if (!dragPlane.Raycast(
            ray,
            out float distance))
        {
            return;
        }

        Vector3 targetPosition =
            ray.GetPoint(distance) +
            grabOffset;

        // =================================================
        // LIMITAR DISTANCIA AL JUGADOR
        // =================================================

        targetPosition =
            ClampToPlayerDistance(
                targetPosition
            );

        // =================================================
        // BUSCAR CERDITO PARA APILAR
        // =================================================

        CubeInteractable supportPig =
            FindNearestPig(
                targetPosition
            );

        if (supportPig != null)
        {
            SnapOnTopOfPig(
                supportPig
            );

            return;
        }

        currentSupportPig = null;

        // =================================================
        // MOVER SOBRE SUELO
        // =================================================

        MoveOnGround(
            targetPosition
        );
    }

    // =====================================================
    // BUSCAR CERDITO MÁS CERCANO
    // =====================================================

    private CubeInteractable FindNearestPig(
        Vector3 targetPosition)
    {
        CubeInteractable[] pigs =
            FindObjectsByType<CubeInteractable>(
                FindObjectsSortMode.None
            );

        CubeInteractable bestPig = null;

        float bestDistance =
            float.MaxValue;

        float highestTop =
            float.NegativeInfinity;

        foreach (CubeInteractable pig in pigs)
        {
            if (pig == null ||
                pig == draggedCube)
            {
                continue;
            }

            Collider pigCollider =
                pig.GetComponent<Collider>();

            if (pigCollider == null)
                continue;

            Vector2 targetXZ =
                new Vector2(
                    targetPosition.x,
                    targetPosition.z
                );

            Vector2 pigXZ =
                new Vector2(
                    pigCollider.bounds.center.x,
                    pigCollider.bounds.center.z
                );

            float distance =
                Vector2.Distance(
                    targetXZ,
                    pigXZ
                );

            if (distance >
                stackDetectionDistance)
            {
                continue;
            }

            float pigTop =
                pigCollider.bounds.max.y;

            // Elegir el cerdito más alto.
            if (pigTop >
                highestTop + 0.01f)
            {
                highestTop =
                    pigTop;

                bestDistance =
                    distance;

                bestPig =
                    pig;
            }
            else if (
                Mathf.Abs(
                    pigTop -
                    highestTop
                ) <= 0.01f &&
                distance <
                bestDistance)
            {
                bestDistance =
                    distance;

                bestPig =
                    pig;
            }
        }

        return bestPig;
    }

    // =====================================================
    // COLOCAR ENCIMA DE OTRO CERDITO
    // =====================================================

    private void SnapOnTopOfPig(
        CubeInteractable supportPig)
    {
        if (supportPig == null)
            return;

        Collider supportCollider =
            supportPig.GetComponent<Collider>();

        if (supportCollider == null)
            return;

        // Distancia entre pivote y parte inferior.
        float bottomOffset =
            draggedBody.position.y -
            draggedCollider.bounds.min.y;

        // Protección por si el offset da un valor extraño.
        bottomOffset =
            Mathf.Max(
                bottomOffset,
                draggedCollider.bounds.extents.y
            );

        float supportTop =
            supportCollider.bounds.max.y;

        float desiredY =
            supportTop +
            bottomOffset +
            stackGap;

        Vector3 desiredPosition =
            new Vector3(
                supportCollider.bounds.center.x,
                desiredY,
                supportCollider.bounds.center.z
            );

        draggedBody.position =
            desiredPosition;

        // Mantenerlo de pie.
        float currentYRotation =
            draggedBody.rotation.eulerAngles.y;

        draggedBody.rotation =
            Quaternion.Euler(
                0f,
                currentYRotation,
                0f
            );

        if (currentSupportPig !=
            supportPig)
        {
            currentSupportPig =
                supportPig;

            Debug.Log(
                "SNAP SOBRE: " +
                supportPig.name
            );
        }
    }

    // =====================================================
    // MOVER SOBRE SUELO
    // =====================================================

    private void MoveOnGround(
        Vector3 targetPosition)
    {
        if (!TryGetGroundHeight(
            targetPosition,
            out float groundY))
        {
            // Si no encontramos suelo,
            // NO movemos el cerdito.
            return;
        }

        Vector3 desiredPosition =
            new Vector3(
                targetPosition.x,
                groundY,
                targetPosition.z
            );

        // =================================================
        // SEGURIDAD:
        // NUNCA PERMITIR QUE EL CERDITO BAJE DEL GROUND
        // =================================================

        float minimumY =
            groundY;

        desiredPosition.y =
            Mathf.Max(
                desiredPosition.y,
                minimumY
            );

        Vector3 newPosition =
            Vector3.MoveTowards(
                draggedBody.position,
                desiredPosition,
                dragMoveSpeed *
                Time.deltaTime
            );

        newPosition.y =
            Mathf.Max(
                newPosition.y,
                minimumY
            );

        draggedBody.MovePosition(
            newPosition
        );
    }

    // =====================================================
    // OBTENER ALTURA DEL SUELO
    // =====================================================

    private bool TryGetGroundHeight(
        Vector3 targetPosition,
        out float desiredY)
    {
        desiredY =
            draggedBody.position.y;

        float bottomOffset =
            draggedBody.position.y -
            draggedCollider.bounds.min.y;

        // Protección adicional.
        bottomOffset =
            Mathf.Max(
                bottomOffset,
                draggedCollider.bounds.extents.y
            );

        /*
         * IMPORTANTE:
         * El raycast comienza siempre suficientemente
         * arriba, independientemente de la posición Y
         * calculada por el dedo.
         */

        Vector3 origin =
            new Vector3(
                targetPosition.x,
                draggedCollider.bounds.max.y +
                    groundCheckHeight,
                targetPosition.z
            );

        float rayDistance =
            groundCheckHeight * 3f;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            rayDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            desiredY =
                hit.point.y +
                bottomOffset +
                groundGap;

            return true;
        }

        return false;
    }

    // =====================================================
    // LIMITAR DISTANCIA AL JUGADOR
    // =====================================================

    private Vector3 ClampToPlayerDistance(
        Vector3 targetPosition)
    {
        Vector3 playerFlat =
            new Vector3(
                playerTransform.position.x,
                0f,
                playerTransform.position.z
            );

        Vector3 targetFlat =
            new Vector3(
                targetPosition.x,
                0f,
                targetPosition.z
            );

        Vector3 fromPlayer =
            targetFlat -
            playerFlat;

        float distance =
            fromPlayer.magnitude;

        if (distance >
            maxDragDistance)
        {
            Vector3 direction =
                fromPlayer.normalized;

            targetPosition.x =
                playerFlat.x +
                direction.x *
                maxDragDistance;

            targetPosition.z =
                playerFlat.z +
                direction.z *
                maxDragDistance;
        }
        else if (
            distance <
                minDragDistance &&
            distance >
                0.001f)
        {
            Vector3 direction =
                fromPlayer.normalized;

            targetPosition.x =
                playerFlat.x +
                direction.x *
                minDragDistance;

            targetPosition.z =
                playerFlat.z +
                direction.z *
                minDragDistance;
        }

        return targetPosition;
    }

    // =====================================================
    // TERMINAR DRAG
    // =====================================================

    private void EndDrag()
    {
        if (!dragging)
            return;

        bool wasStacked =
            currentSupportPig != null;

        if (draggedBody != null)
        {
            draggedBody.linearVelocity =
                Vector3.zero;

            draggedBody.angularVelocity =
                Vector3.zero;

            draggedBody.isKinematic =
                false;

            draggedBody.useGravity =
                true;
        }

        // =================================================
        // AVISAR A PIGSTABILIZER
        // =================================================

        if (draggedCube != null)
        {
            PigStabilizer stabilizer =
                draggedCube.GetComponent<PigStabilizer>();

            if (stabilizer != null)
            {
                stabilizer.SetStacked(
                    wasStacked
                );
            }
        }

        if (wasStacked)
        {
            Debug.Log(
                "CERDITO COLOCADO EN PILA."
            );
        }

        ClearDragState();

        if (modeController != null)
        {
            modeController.DeactivateMode();
        }

        Debug.Log(
            "DRAG TERMINADO."
        );
    }

    // =====================================================
    // CANCELAR DRAG
    // =====================================================

    private void CancelDrag()
    {
        if (draggedBody != null)
        {
            draggedBody.linearVelocity =
                Vector3.zero;

            draggedBody.angularVelocity =
                Vector3.zero;

            draggedBody.isKinematic =
                false;

            draggedBody.useGravity =
                true;
        }

        ClearDragState();

        if (modeController != null)
        {
            modeController.DeactivateMode();
        }

        Debug.Log(
            "DRAG CANCELADO."
        );
    }

    // =====================================================
    // LIMPIAR ESTADO
    // =====================================================

    private void ClearDragState()
    {
        dragging = false;

        dragFingerId = -1;

        draggedBody = null;

        draggedCollider = null;

        draggedCube = null;

        currentSupportPig = null;
    }
}