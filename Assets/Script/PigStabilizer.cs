using UnityEngine;

public class PigStabilizer : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private Rigidbody body;

    // =====================================================
    // DETECCIÓN DE SOPORTE
    // =====================================================

    [Header("Detección de soporte")]

    [Tooltip("Capas que pueden sostener al cerdito.")]
    [SerializeField] private LayerMask supportLayers;

    [Tooltip("Tiempo de contacto antes de comenzar a estabilizar.")]
    [SerializeField] private float contactDelay = 0.15f;

    [Tooltip("Distancia extra para comprobar soporte debajo.")]
    [SerializeField] private float supportCheckDistance = 0.15f;

    // =====================================================
    // PROTECCIÓN CONTRA EL SUELO
    // =====================================================

    [Header("Protección anticaída")]

    [Tooltip("Seleccionar únicamente la capa Ground.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Altura desde la que buscamos el suelo.")]
    [SerializeField] private float groundRecoveryCheckHeight = 10f;

    [Tooltip("Separación mínima entre el cerdito y el suelo.")]
    [SerializeField] private float minimumGroundGap = 0.03f;

    [Tooltip("Margen permitido antes de considerar que atravesó el suelo.")]
    [SerializeField] private float fallThroughTolerance = 0.08f;

    // =====================================================
    // ESTABILIZACIÓN NORMAL
    // =====================================================

    [Header("Estabilización al caer")]

    [Tooltip("Velocidad máxima para comenzar a enderezarse.")]
    [SerializeField] private float maxSpeedToStabilize = 3f;

    [Tooltip("Fuerza con la que vuelve a quedar de pie.")]
    [SerializeField] private float uprightStrength = 7f;

    [Tooltip("Amortiguación de la rotación.")]
    [SerializeField] private float rotationDamping = 6f;

    [Tooltip("Velocidad angular máxima mientras se estabiliza.")]
    [SerializeField]
    private float maxAngularVelocityWhileStabilizing = 2f;

    // =====================================================
    // ESTABILIZACIÓN DE PILA
    // =====================================================

    [Header("Estabilización de pila")]

    [Tooltip("Fuerza para mantener el cerdito centrado.")]
    [SerializeField] private float stackPositionStrength = 8f;

    [Tooltip("Distancia máxima X/Z para considerarlo alineado.")]
    [SerializeField] private float stackAlignmentTolerance = 0.2f;

    [Tooltip("Velocidad máxima para bloquear la pila.")]
    [SerializeField] private float stackVelocityThreshold = 0.7f;

    // =====================================================
    // ESTADO
    // =====================================================

    private bool hasSupport = false;
    private bool supportedByPig = false;

    private float contactTime = 0f;

    private Transform supportingPig;

    private bool stackLocked = false;

    private Collider pigCollider;

    // =====================================================
    // PROPIEDADES
    // =====================================================

    public bool IsStackLocked => stackLocked;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }

        pigCollider = GetComponent<Collider>();

        if (body == null)
        {
            Debug.LogError(
                name +
                " necesita Rigidbody para PigStabilizer."
            );
        }

        if (pigCollider == null)
        {
            Debug.LogError(
                name +
                " necesita Collider para PigStabilizer."
            );
        }
    }

    // =====================================================
    // FIXED UPDATE
    // =====================================================

    private void FixedUpdate()
    {
        if (body == null ||
            pigCollider == null)
        {
            return;
        }

        // Mientras Drag lo controla,
        // PigStabilizer no modifica su posición.
        if (body.isKinematic)
        {
            contactTime = 0f;
            return;
        }

        // =================================================
        // PROTECCIÓN CONTRA ATRAVESAR EL SUELO
        // =================================================

        RecoverIfBelowGround();

        // =================================================
        // SOPORTE NORMAL
        // =================================================

        CheckSupportBelow();

        if (!hasSupport)
        {
            contactTime = 0f;

            stackLocked = false;

            supportedByPig = false;

            supportingPig = null;

            return;
        }

        contactTime +=
            Time.fixedDeltaTime;

        if (contactTime < contactDelay)
            return;

        // Enderezar cuando tiene soporte.
        StabilizeRotation();

        // Si está sobre otro cerdito,
        // estabilizar también la pila.
        if (supportedByPig &&
            supportingPig != null)
        {
            CheckStackAlignment();

            if (stackLocked)
            {
                StabilizeStack();
            }
        }
        else
        {
            stackLocked = false;
        }
    }

    // =====================================================
    // PROTECCIÓN ANTICAÍDA
    // =====================================================

    private void RecoverIfBelowGround()
    {
        /*
         * Buscamos el suelo desde arriba utilizando
         * únicamente la capa Ground.
         *
         * Así no importa si Plano_guia está en Y = 0,
         * Y = 2, Y = -3, etc.
         */

        Vector3 origin =
            new Vector3(
                pigCollider.bounds.center.x,
                pigCollider.bounds.max.y +
                    groundRecoveryCheckHeight,
                pigCollider.bounds.center.z
            );

        float rayDistance =
            groundRecoveryCheckHeight * 3f;

        if (!Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            rayDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        // Distancia real entre el pivote del cerdito
        // y la parte inferior de su Collider.
        float bottomOffset =
            transform.position.y -
            pigCollider.bounds.min.y;

        bottomOffset =
            Mathf.Max(
                bottomOffset,
                pigCollider.bounds.extents.y
            );

        // Esta es la Y mínima correcta para ESTE cerdito.
        float safeY =
            hit.point.y +
            bottomOffset +
            minimumGroundGap;

        /*
         * Solo recuperamos si realmente ha atravesado
         * el suelo. No queremos interferir con saltos,
         * Swipe o con una pila normal.
         */

        if (transform.position.y >=
            safeY - fallThroughTolerance)
        {
            return;
        }

        Vector3 recoveredPosition =
            body.position;

        recoveredPosition.y =
            safeY;

        body.position =
            recoveredPosition;

        // Detener únicamente la caída vertical.
        Vector3 velocity =
            body.linearVelocity;

        if (velocity.y < 0f)
        {
            velocity.y = 0f;
        }

        body.linearVelocity =
            velocity;

        Debug.LogWarning(
            name +
            ": atravesó el suelo y fue recuperado."
        );
    }

    // =====================================================
    // COMPROBAR QUÉ HAY DEBAJO
    // =====================================================

    private void CheckSupportBelow()
    {
        hasSupport = false;
        supportedByPig = false;
        supportingPig = null;

        Vector3 origin =
            new Vector3(
                pigCollider.bounds.center.x,
                pigCollider.bounds.min.y + 0.05f,
                pigCollider.bounds.center.z
            );

        float distance =
            supportCheckDistance + 0.08f;

        RaycastHit[] hits =
            Physics.RaycastAll(
                origin,
                Vector3.down,
                distance,
                supportLayers,
                QueryTriggerInteraction.Ignore
            );

        float nearestDistance =
            float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            // Ignorar nuestro propio Collider.
            if (hit.collider == pigCollider)
                continue;

            // Ignorar hijos del mismo cerdito.
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.distance >= nearestDistance)
                continue;

            nearestDistance =
                hit.distance;

            hasSupport = true;

            CubeInteractable otherPig =
                hit.collider.GetComponent<
                    CubeInteractable>();

            if (otherPig == null)
            {
                otherPig =
                    hit.collider.GetComponentInParent<
                        CubeInteractable>();
            }

            if (otherPig != null &&
                otherPig.gameObject != gameObject)
            {
                supportedByPig = true;

                supportingPig =
                    otherPig.transform;
            }
            else
            {
                supportedByPig = false;

                supportingPig = null;
            }
        }

        Debug.DrawRay(
            origin,
            Vector3.down * distance,
            hasSupport ?
                Color.green :
                Color.red
        );
    }

    // =====================================================
    // COMPROBAR ALINEACIÓN
    // =====================================================

    private void CheckStackAlignment()
    {
        if (supportingPig == null)
        {
            stackLocked = false;
            return;
        }

        Vector2 myPosition =
            new Vector2(
                transform.position.x,
                transform.position.z
            );

        Vector2 supportPosition =
            new Vector2(
                supportingPig.position.x,
                supportingPig.position.z
            );

        float horizontalDistance =
            Vector2.Distance(
                myPosition,
                supportPosition
            );

        if (horizontalDistance >
            stackAlignmentTolerance)
        {
            stackLocked = false;
            return;
        }

        if (body.linearVelocity.magnitude >
            stackVelocityThreshold)
        {
            return;
        }

        stackLocked = true;
    }

    // =====================================================
    // ESTABILIZAR PILA
    // =====================================================

    private void StabilizeStack()
    {
        if (supportingPig == null)
        {
            stackLocked = false;
            return;
        }

        Vector3 current =
            body.position;

        Vector3 target =
            new Vector3(
                supportingPig.position.x,
                current.y,
                supportingPig.position.z
            );

        Vector3 newPosition =
            Vector3.Lerp(
                current,
                target,
                stackPositionStrength *
                Time.fixedDeltaTime
            );

        body.MovePosition(
            newPosition
        );

        Vector3 velocity =
            body.linearVelocity;

        velocity.x =
            Mathf.Lerp(
                velocity.x,
                0f,
                stackPositionStrength *
                Time.fixedDeltaTime
            );

        velocity.z =
            Mathf.Lerp(
                velocity.z,
                0f,
                stackPositionStrength *
                Time.fixedDeltaTime
            );

        body.linearVelocity =
            velocity;
    }

    // =====================================================
    // ESTABILIZAR ROTACIÓN
    // =====================================================

    private void StabilizeRotation()
    {
        if (body.linearVelocity.magnitude >
            maxSpeedToStabilize)
        {
            return;
        }

        body.angularVelocity =
            Vector3.Lerp(
                body.angularVelocity,
                Vector3.zero,
                rotationDamping *
                Time.fixedDeltaTime
            );

        body.maxAngularVelocity =
            maxAngularVelocityWhileStabilizing;

        float currentY =
            body.rotation.eulerAngles.y;

        Quaternion targetRotation =
            Quaternion.Euler(
                0f,
                currentY,
                0f
            );

        Quaternion newRotation =
            Quaternion.Slerp(
                body.rotation,
                targetRotation,
                uprightStrength *
                Time.fixedDeltaTime
            );

        body.MoveRotation(
            newRotation
        );
    }

    // =====================================================
    // LIBERAR PILA
    // =====================================================

    public void ReleaseStack()
    {
        stackLocked = false;

        supportedByPig = false;

        supportingPig = null;

        contactTime = 0f;

        Debug.Log(
            name +
            ": liberado de la pila."
        );
    }

    // =====================================================
    // MARCAR COMO APILADO DESDE DRAG
    // =====================================================

    public void SetStacked(
        bool stacked)
    {
        if (!stacked)
        {
            ReleaseStack();
            return;
        }

        contactTime = 0f;

        if (body != null)
        {
            body.linearVelocity =
                Vector3.zero;

            body.angularVelocity =
                Vector3.zero;
        }

        Debug.Log(
            name +
            ": colocado en posición de apilado."
        );
    }
}