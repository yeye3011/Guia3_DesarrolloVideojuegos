using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameTimer gameTimer;

    [Header("Configuración")]
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    // =====================================================
    // VARIABLES
    // =====================================================

    private CubeInteractable selectedCube;
    private CubeInteractable heldCube;

    private bool timerStarted = false;

    // =====================================================
    // PROPIEDADES PÚBLICAS
    // =====================================================

    public CubeInteractable SelectedCube
    {
        get { return selectedCube; }
    }

    public bool IsHoldingCube
    {
        get { return heldCube != null; }
    }

    // =====================================================
    // TAP
    // =====================================================

    public void Tap()
    {
        Debug.Log("TAP recibido por InteractionManager.");

        // -------------------------------------------------
        // INICIAR CRONÓMETRO
        // -------------------------------------------------

        StartTimerIfNeeded();

        // -------------------------------------------------
        // TAP 1 → SELECCIONAR
        // -------------------------------------------------

        if (selectedCube == null)
        {
            TrySelect();
            return;
        }

        // -------------------------------------------------
        // TAP 2 → AGARRAR
        // -------------------------------------------------

        if (heldCube == null)
        {
            TryGrab();
            return;
        }

        // -------------------------------------------------
        // TAP 3 → SOLTAR
        // -------------------------------------------------

        TryPlace();
    }

    // =====================================================
    // CRONÓMETRO
    // =====================================================

    private void StartTimerIfNeeded()
    {
        if (timerStarted)
            return;

        if (gameTimer == null)
        {
            Debug.LogWarning(
                "GameTimer no está asignado en InteractionManager."
            );

            return;
        }

        gameTimer.StartTimer();

        timerStarted = true;

        Debug.Log(
            "Cronómetro iniciado con el primer input."
        );
    }

    // =====================================================
    // SELECCIONAR CUBO
    // =====================================================

    public void TrySelect()
    {
        if (playerCamera == null)
        {
            Debug.LogError(
                "Player Camera no está asignada."
            );

            return;
        }

        // -------------------------------------------------
        // RAYO DESDE EL CENTRO DE LA CÁMARA
        // -------------------------------------------------

        Ray ray =
            playerCamera.ViewportPointToRay(
                new Vector3(
                    0.5f,
                    0.5f,
                    0f
                )
            );

        // -------------------------------------------------
        // DETECTAR CUBO
        // -------------------------------------------------

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxDistance,
            interactableLayer
        ))
        {
            CubeInteractable cube =
                hit.collider.GetComponent<CubeInteractable>();

            if (cube == null)
            {
                cube =
                    hit.collider.GetComponentInParent<
                        CubeInteractable
                    >();
            }

            if (cube == null)
            {
                Debug.Log(
                    "El objeto detectado no es un cubo interactuable."
                );

                return;
            }

            // -------------------------------------------------
            // QUITAR SELECCIÓN DEL CUBO ANTERIOR
            // -------------------------------------------------

            if (selectedCube != null &&
                selectedCube != cube)
            {
                selectedCube.SetSelected(false);
            }

            // -------------------------------------------------
            // SELECCIONAR NUEVO CUBO
            // -------------------------------------------------

            selectedCube = cube;

            selectedCube.SetSelected(true);

            Debug.Log(
                "Cubo seleccionado: " +
                selectedCube.name
            );
        }
        else
        {
            Debug.Log(
                "No se encontró ningún cubo."
            );
        }
    }

    // =====================================================
    // AGARRAR CUBO
    // =====================================================

    public void TryGrab()
    {
        if (selectedCube == null)
        {
            Debug.Log(
                "No hay cubo seleccionado."
            );

            return;
        }

        if (heldCube != null)
        {
            Debug.Log(
                "Ya hay un cubo agarrado."
            );

            return;
        }

        heldCube = selectedCube;

        Debug.Log(
            "Cubo agarrado: " +
            heldCube.name
        );
    }

    // =====================================================
    // SOLTAR CUBO
    // =====================================================

    public void TryPlace()
    {
        if (heldCube == null)
        {
            Debug.Log(
                "No hay ningún cubo agarrado."
            );

            return;
        }

        Debug.Log(
            "Cubo soltado: " +
            heldCube.name
        );

        // -------------------------------------------------
        // VOLVER A ACTIVAR LA FÍSICA
        // -------------------------------------------------

        Rigidbody body =
            heldCube.GetComponent<Rigidbody>();

        if (body != null)
        {
            body.isKinematic = false;
        }

        // -------------------------------------------------
        // DEJAR DE TENERLO AGARRADO
        // -------------------------------------------------

        heldCube = null;

        // -------------------------------------------------
        // MANTENER SELECCIÓN
        // -------------------------------------------------

        if (selectedCube != null)
        {
            selectedCube.SetSelected(true);
        }

        Debug.Log(
            "El cubo quedó libre."
        );
    }

    // =====================================================
    // LIMPIAR SELECCIÓN
    // =====================================================

    public void ClearSelection()
    {
        if (selectedCube != null)
        {
            selectedCube.SetSelected(false);
        }

        selectedCube = null;

        Debug.Log(
            "Selección eliminada."
        );
    }

    // =====================================================
    // OBTENER CUBO AGARRADO
    // =====================================================

    public CubeInteractable GetHeldCube()
    {
        return heldCube;
    }

    // =====================================================
    // CANCELAR AGARRE
    // =====================================================

    public void CancelGrab()
    {
        if (heldCube != null)
        {
            Rigidbody body =
                heldCube.GetComponent<Rigidbody>();

            if (body != null)
            {
                body.isKinematic = false;
            }
        }

        heldCube = null;

        Debug.Log(
            "Agarre cancelado."
        );
    }
}