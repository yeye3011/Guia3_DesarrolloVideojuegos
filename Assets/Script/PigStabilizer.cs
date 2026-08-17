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

        // Drag controla completamente el cerdito.
        if (body.isKinematic)
        {
            contactTime = 0f;
            return;
        }

        // Comprobamos realmente qué existe debajo.
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

        // Siempre intentamos que quede de pie
        // cuando tiene soporte.
        StabilizeRotation();

        // Si está encima de otro cerdito,
        // también estabilizamos horizontalmente.
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
    // COMPROBAR QUÉ HAY DEBAJO
    // =====================================================

    private void CheckSupportBelow()
    {
        hasSupport = false;
        supportedByPig = false;
        supportingPig = null;

        // Punto ligeramente por encima de
        // la parte inferior del collider.
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

            // Ignorar nuestro propio collider.
            if (hit.collider == pigCollider)
                continue;

            // Ignorar colliders hijos del mismo cerdito.
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.distance >= nearestDistance)
                continue;

            nearestDistance =
                hit.distance;

            hasSupport = true;

            CubeInteractable otherPig =
                hit.collider.GetComponent<CubeInteractable>();

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
            hasSupport ? Color.green : Color.red
        );
    }

    // =====================================================
    // COMPROBAR ALINEACIÓN DE PILA
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

        // Reducimos únicamente movimiento horizontal.
        // La gravedad continúa funcionando en Y.
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

        /*
         * No inventamos aquí qué cerdito está debajo.
         * CheckSupportBelow() lo detectará físicamente
         * en el siguiente FixedUpdate.
         */

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