using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameTimer gameTimer;

    [Header("Botones de interacción")]
    [SerializeField] private Button dragButton;
    [SerializeField] private Button swipeButton;

    [Header("Configuración")]
    [SerializeField] private float maxDistance = 20f;
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
    // START
    // =====================================================

    private void Start()
    {
        UpdateInteractionButtons();
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

        if (playerCamera == null)
        {
            Debug.LogError(
                "Player Camera no está asignada."
            );

            return;
        }

        // -------------------------------------------------
        // RAYO DESDE EL LUGAR TOCADO
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
                hit.collider.GetComponentInParent<CubeInteractable>();
        }

        if (cube == null)
        {
            Debug.Log(
                "El objeto tocado no tiene CubeInteractable."
            );

            return;
        }

        // -------------------------------------------------
        // SI TOCAMOS EL MISMO CERDITO AGARRADO → SOLTAR
        // -------------------------------------------------

        if (heldCube == cube)
        {
            ReleaseCube();
            return;
        }

        // -------------------------------------------------
        // SI HABÍA OTRO CERDITO → SOLTARLO
        // -------------------------------------------------

        if (heldCube != null)
        {
            ReleaseCube();
        }

        // -------------------------------------------------
        // SELECCIONAR + AGARRAR
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

        // Apagar selección anterior
        if (selectedCube != null &&
            selectedCube != cube)
        {
            selectedCube.SetSelected(false);
        }

        selectedCube = cube;
        heldCube = cube;

        // Encender luz/parpadeo
        selectedCube.SetSelected(true);

        // Habilitar Drag y Swipe
        UpdateInteractionButtons();

        Debug.Log(
            "Cerdito seleccionado y agarrado: " +
            selectedCube.name
        );
    }

    // =====================================================
    // SOLTAR
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

        // Reactivar física
        Rigidbody body =
            cubeToRelease.GetComponent<Rigidbody>();

        if (body != null)
        {
            body.isKinematic = false;
        }

        // Apagar selección
        cubeToRelease.SetSelected(false);

        heldCube = null;
        selectedCube = null;

        // Deshabilitar Drag y Swipe
        UpdateInteractionButtons();

        Debug.Log(
            "Cerdito soltado: " +
            cubeToRelease.name
        );
    }

    // =====================================================
    // BOTONES DRAG / SWIPE
    // =====================================================

    private void UpdateInteractionButtons()
    {
        bool hasCube =
            heldCube != null;

        if (dragButton != null)
        {
            dragButton.interactable =
                hasCube;
        }

        if (swipeButton != null)
        {
            swipeButton.interactable =
                hasCube;
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
                "GameTimer no está asignado."
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

        selectedCube = null;
        heldCube = null;

        UpdateInteractionButtons();

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

        UpdateInteractionButtons();

        Debug.Log(
            "Agarre cancelado."
        );
    }
}