using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private InteractionModeController modeController;

    // =====================================================
    // CONFIGURACIÓN
    // =====================================================

    [Header("Configuración")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Doble Tap")]
    [SerializeField] private float doubleTapTime = 0.35f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private CubeInteractable selectedCube;
    private CubeInteractable heldCube;

    private bool timerStarted = false;

    private float lastReleaseTapTime = -1f;
    private int releaseTapCount = 0;

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
    // START
    // =====================================================

    private void Start()
    {
        UpdateModeButtons();
    }

    // =====================================================
    // TAP
    // =====================================================

    public void Tap(Vector2 screenPosition)
    {
        Debug.Log(
            "TAP recibido en: " + screenPosition
        );

        StartTimerIfNeeded();

        // -------------------------------------------------
        // VERIFICAR CÁMARA
        // -------------------------------------------------

        if (playerCamera == null)
        {
            Debug.LogError(
                "Player Camera no está asignada."
            );

            return;
        }

        // -------------------------------------------------
        // RAYO DESDE LA POSICIÓN TOCADA
        // -------------------------------------------------

        Ray ray =
            playerCamera.ScreenPointToRay(
                screenPosition
            );

        // -------------------------------------------------
        // RAYCAST
        // -------------------------------------------------

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxDistance,
            interactableLayer
        ))
        {
            Debug.Log(
                "El TAP no tocó un cerdito interactuable."
            );

            return;
        }

        // -------------------------------------------------
        // BUSCAR CUBE INTERACTABLE
        // -------------------------------------------------

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
                "El objeto tocado no tiene CubeInteractable."
            );

            return;
        }

        // -------------------------------------------------
        // MISMO CERDITO AGARRADO
        // -------------------------------------------------

        /*
         * Si tocamos el mismo cerdito que ya está
         * seleccionado/agarrado, necesitamos doble tap
         * para soltarlo.
         */

        if (heldCube == cube)
        {
            HandleReleaseDoubleTap();
            return;
        }

        // -------------------------------------------------
        // OTRO CERDITO
        // -------------------------------------------------

        /*
         * Si había otro cerdito seleccionado,
         * primero lo soltamos.
         */

        if (heldCube != null)
        {
            ReleaseCube();
        }

        // -------------------------------------------------
        // SELECCIONAR NUEVO CERDITO
        // -------------------------------------------------

        SelectAndGrab(cube);
    }

    // =====================================================
    // SELECCIONAR + AGARRAR
    // =====================================================

    private void SelectAndGrab(
        CubeInteractable cube)
    {
        if (cube == null)
            return;

        ResetDoubleTap();

        // -------------------------------------------------
        // QUITAR SELECCIÓN ANTERIOR
        // -------------------------------------------------

        if (selectedCube != null &&
            selectedCube != cube)
        {
            selectedCube.SetSelected(false);
        }

        // -------------------------------------------------
        // GUARDAR CERDITO
        // -------------------------------------------------

        selectedCube = cube;
        heldCube = cube;

        // -------------------------------------------------
        // FEEDBACK VISUAL DEL CERDITO
        // -------------------------------------------------

        selectedCube.SetSelected(true);

        // -------------------------------------------------
        // ACTUALIZAR BOTONES
        // -------------------------------------------------

        UpdateModeButtons();

        Debug.Log(
            "Cerdito seleccionado y agarrado: " +
            selectedCube.name
        );
    }

    // =====================================================
    // DOBLE TAP PARA SOLTAR
    // =====================================================

    private void HandleReleaseDoubleTap()
    {
        float currentTime =
            Time.unscaledTime;

        // -------------------------------------------------
        // PRIMER TAP
        // -------------------------------------------------

        if (releaseTapCount == 0 ||
            currentTime - lastReleaseTapTime >
            doubleTapTime)
        {
            releaseTapCount = 1;

            lastReleaseTapTime =
                currentTime;

            Debug.Log(
                "Primer tap para soltar. " +
                "Haz otro tap rápidamente."
            );

            return;
        }

        // -------------------------------------------------
        // SEGUNDO TAP
        // -------------------------------------------------

        releaseTapCount++;

        if (releaseTapCount >= 2)
        {
            Debug.Log(
                "DOBLE TAP detectado. " +
                "Soltando cerdito."
            );

            ResetDoubleTap();

            ReleaseCube();
        }
    }

    // =====================================================
    // REINICIAR DOBLE TAP
    // =====================================================

    private void ResetDoubleTap()
    {
        releaseTapCount = 0;
        lastReleaseTapTime = -1f;
    }

    // =====================================================
    // SOLTAR CERDITO
    // =====================================================

    public void ReleaseCube()
    {
        if (heldCube == null)
        {
            Debug.Log(
                "No hay ningún cerdito agarrado."
            );

            return;
        }

        CubeInteractable cubeToRelease =
            heldCube;

        // -------------------------------------------------
        // REACTIVAR FÍSICA
        // -------------------------------------------------

        Rigidbody body =
            cubeToRelease.GetComponent<Rigidbody>();

        if (body != null)
        {
            body.isKinematic = false;
        }

        // -------------------------------------------------
        // QUITAR SELECCIÓN
        // -------------------------------------------------

        cubeToRelease.SetSelected(false);

        // -------------------------------------------------
        // LIMPIAR REFERENCIAS
        // -------------------------------------------------

        heldCube = null;
        selectedCube = null;

        ResetDoubleTap();

        // Si Drag o Swipe estaban seleccionados,
        // también se cancelan.
        if (modeController != null)
        {
            modeController.DeactivateMode();
        }

        UpdateModeButtons();

        Debug.Log(
            "Cerdito soltado: " +
            cubeToRelease.name
        );
    }

    // =====================================================
    // ACTUALIZAR BOTONES DRAG / SWIPE
    // =====================================================

    private void UpdateModeButtons()
    {
        if (modeController != null)
        {
            modeController.UpdateButtonStates();
        }
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
            "Cronómetro iniciado."
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

        // Reactivar física por seguridad.
        if (heldCube != null)
        {
            Rigidbody body =
                heldCube.GetComponent<Rigidbody>();

            if (body != null)
            {
                body.isKinematic = false;
            }
        }

        selectedCube = null;
        heldCube = null;

        ResetDoubleTap();

        if (modeController != null)
        {
            modeController.DeactivateMode();
        }

        UpdateModeButtons();

        Debug.Log(
            "Selección eliminada."
        );
    }

    // =====================================================
    // OBTENER CERDITO AGARRADO
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

            heldCube.SetSelected(false);
        }

        heldCube = null;
        selectedCube = null;

        ResetDoubleTap();

        if (modeController != null)
        {
            modeController.DeactivateMode();
        }

        UpdateModeButtons();

        Debug.Log(
            "Agarre cancelado."
        );
    }
}