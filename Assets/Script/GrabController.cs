using UnityEngine;

public class GrabController : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    // =====================================================
    // CONFIGURACIÓN
    // =====================================================

    [Header("Configuración")]
    [SerializeField] private float grabDistance = 3f;

    // =====================================================
    // ESTADO
    // =====================================================

    private GrabbableObject targetedObject;
    private GrabbableObject heldObject;
    private Rigidbody heldBody;

    private CubeInteractable highlightedObject;

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        DetectObject();
    }

    // =====================================================
    // DETECTAR OBJETO APUNTADO
    // =====================================================

    private void DetectObject()
    {
        // Si ya tenemos un objeto cogido,
        // quitamos cualquier resaltado.
        if (heldObject != null)
        {
            ClearTarget();
            return;
        }

        if (playerCamera == null)
        {
            return;
        }

        // Raycast exactamente desde el centro de la cámara.
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        // ¿Hay algo delante dentro de la distancia permitida?
        if (Physics.Raycast(ray, out hit, grabDistance))
        {
            // Buscar GrabbableObject en el objeto golpeado
            // o en alguno de sus padres.
            GrabbableObject grabbable =
                hit.collider.GetComponent<GrabbableObject>();

            if (grabbable == null)
            {
                grabbable =
                    hit.collider.GetComponentInParent<GrabbableObject>();
            }

            if (grabbable != null)
            {
                SetTarget(grabbable);
                return;
            }
        }

        // No estamos apuntando a un objeto válido.
        ClearTarget();
    }

    // =====================================================
    // ESTABLECER OBJETO APUNTADO
    // =====================================================

    private void SetTarget(GrabbableObject newTarget)
    {
        // Si seguimos apuntando al mismo,
        // no hacemos nada.
        if (targetedObject == newTarget)
        {
            return;
        }

        // Quitar resaltado del anterior.
        ClearTarget();

        targetedObject = newTarget;

        // Buscar CubeInteractable para usar
        // el sistema de resaltado existente.
        highlightedObject =
            targetedObject.GetComponent<CubeInteractable>();

        if (highlightedObject == null)
        {
            highlightedObject =
                targetedObject.GetComponentInChildren<CubeInteractable>();
        }

        if (highlightedObject != null)
        {
            highlightedObject.SetSelected(true);
        }
    }

    // =====================================================
    // LIMPIAR OBJETO APUNTADO
    // =====================================================

    private void ClearTarget()
    {
        if (highlightedObject != null)
        {
            highlightedObject.SetSelected(false);
        }

        highlightedObject = null;
        targetedObject = null;
    }

    // =====================================================
    // COGER
    // =====================================================

    public void Grab()
    {
        // Ya tenemos un objeto.
        if (heldObject != null)
        {
            Debug.Log("Ya tienes un objeto cogido.");
            return;
        }

        // No estamos apuntando a ningún cerdito válido.
        if (targetedObject == null)
        {
            Debug.Log(
                "No estás apuntando a un objeto interactuable."
            );

            return;
        }

        if (holdPoint == null)
        {
            Debug.LogError(
                "GrabController: HoldPoint no está asignado."
            );

            return;
        }

        heldObject = targetedObject;

        heldBody =
            heldObject.GetComponent<Rigidbody>();

        if (heldBody == null)
        {
            Debug.LogError(
                heldObject.name + " no tiene Rigidbody."
            );

            heldObject = null;
            return;
        }

        // Quitar resaltado.
        if (highlightedObject != null)
        {
            highlightedObject.SetSelected(false);
        }

        highlightedObject = null;
        targetedObject = null;

        // Si estaba apilado, liberarlo.
        PigStabilizer stabilizer =
            heldObject.GetComponent<PigStabilizer>();

        if (stabilizer != null)
        {
            stabilizer.ReleaseStack();
        }

        // Detener movimiento.
        heldBody.linearVelocity = Vector3.zero;
        heldBody.angularVelocity = Vector3.zero;

        // Desactivar física mientras lo llevamos.
        heldBody.useGravity = false;
        heldBody.isKinematic = true;

        // Convertirlo en hijo del HoldPoint.
        heldObject.transform.SetParent(holdPoint);

        // Colocarlo exactamente en el HoldPoint.
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        Debug.Log(
            "Objeto cogido: " + heldObject.name
        );
    }

    // =====================================================
    // SOLTAR
    // =====================================================

    public void Release()
    {
        if (heldObject == null)
        {
            Debug.Log(
                "No tienes ningún objeto para soltar."
            );

            return;
        }

        GrabbableObject objectToRelease = heldObject;
        Rigidbody bodyToRelease = heldBody;

        // Sacarlo del HoldPoint.
        objectToRelease.transform.SetParent(null);

        // Reactivar física.
        if (bodyToRelease != null)
        {
            bodyToRelease.isKinematic = false;
            bodyToRelease.useGravity = true;

            bodyToRelease.linearVelocity = Vector3.zero;
            bodyToRelease.angularVelocity = Vector3.zero;
        }

        heldObject = null;
        heldBody = null;

        Debug.Log(
            "Objeto soltado: " + objectToRelease.name
        );
    }

    // =====================================================
    // DIBUJAR RAYCAST EN SCENE
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
        {
            return;
        }

        Gizmos.DrawRay(
            playerCamera.transform.position,
            playerCamera.transform.forward * grabDistance
        );
    }
}