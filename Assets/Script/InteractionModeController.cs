using UnityEngine;
using UnityEngine.UI;

public class InteractionModeController : MonoBehaviour
{
    // =====================================================
    // MODOS
    // =====================================================

    public enum InteractionMode
    {
        None,
        Drag,
        Swipe
    }

    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField]
    private InteractionManager interactionManager;

    [SerializeField]
    private Button dragButton;

    [SerializeField]
    private Button swipeButton;

    // =====================================================
    // APARIENCIA
    // =====================================================

    [Header("Apariencia de botones")]

    [Tooltip("Color cuando no hay cerdito seleccionado.")]
    [SerializeField]
    private Color disabledColor =
        new Color(1f, 1f, 1f, 0.35f);

    [Tooltip("Color cuando la acción está disponible.")]
    [SerializeField]
    private Color availableColor =
        Color.white;

    [Tooltip("Color cuando la acción está seleccionada.")]
    [SerializeField]
    private Color activeColor =
        new Color(1f, 0.75f, 0.35f, 1f);

    // =====================================================
    // ESTADO
    // =====================================================

    public InteractionMode CurrentMode
    {
        get;
        private set;
    } = InteractionMode.None;

    public bool IsDragMode =>
        CurrentMode == InteractionMode.Drag;

    public bool IsSwipeMode =>
        CurrentMode == InteractionMode.Swipe;

    // Dedo utilizado actualmente por Drag/Swipe.
    private int reservedFingerId = -1;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        UpdateButtonStates();
    }

    // =====================================================
    // ACTIVAR DRAG
    // =====================================================

    public void ActivateDragMode()
    {
        if (!CanInteract())
        {
            Debug.Log(
                "Drag no disponible: " +
                "no hay cerdito seleccionado."
            );

            UpdateButtonStates();
            return;
        }

        CurrentMode =
            InteractionMode.Drag;

        reservedFingerId = -1;

        Debug.Log(
            "Modo DRAG activado."
        );

        UpdateButtonStates();
    }

    // =====================================================
    // ACTIVAR SWIPE
    // =====================================================

    public void ActivateSwipeMode()
    {
        if (!CanInteract())
        {
            Debug.Log(
                "Swipe no disponible: " +
                "no hay cerdito seleccionado."
            );

            UpdateButtonStates();
            return;
        }

        CurrentMode =
            InteractionMode.Swipe;

        reservedFingerId = -1;

        Debug.Log(
            "Modo SWIPE activado."
        );

        UpdateButtonStates();
    }

    // =====================================================
    // DESACTIVAR MODO
    // =====================================================

    public void DeactivateMode()
    {
        CurrentMode =
            InteractionMode.None;

        reservedFingerId = -1;

        Debug.Log(
            "Modo de interacción desactivado."
        );

        UpdateButtonStates();
    }

    // =====================================================
    // RESERVAR DEDO
    // =====================================================

    public bool TryReserveFinger(
        int fingerId)
    {
        // Nadie tiene reservado un dedo todavía.
        if (reservedFingerId < 0)
        {
            reservedFingerId =
                fingerId;

            return true;
        }

        // El mismo dedo puede seguir utilizando
        // la interacción.
        return reservedFingerId ==
               fingerId;
    }

    // =====================================================
    // CONSULTAR DEDO RESERVADO
    // =====================================================

    public bool IsFingerReserved(
        int fingerId)
    {
        return reservedFingerId ==
               fingerId;
    }

    public bool IsInteractionFinger(int fingerId)
    {
        return IsFingerReserved(fingerId);
    }

    // =====================================================
    // ¿SE PUEDE INTERACTUAR?
    // =====================================================

    private bool CanInteract()
    {
        if (interactionManager == null)
            return false;

        return
            interactionManager.SelectedCube != null &&
            interactionManager.IsHoldingCube;
    }

    // =====================================================
    // ACTUALIZAR BOTONES
    // =====================================================

    public void UpdateButtonStates()
    {
        bool hasPig =
            CanInteract();

        // -------------------------------------------------
        // SIN CERDITO
        // -------------------------------------------------

        if (!hasPig)
        {
            SetButtonState(
                dragButton,
                false,
                disabledColor
            );

            SetButtonState(
                swipeButton,
                false,
                disabledColor
            );

            return;
        }

        // -------------------------------------------------
        // DRAG ACTIVO
        // -------------------------------------------------

        if (CurrentMode ==
            InteractionMode.Drag)
        {
            SetButtonState(
                dragButton,
                true,
                activeColor
            );

            SetButtonState(
                swipeButton,
                true,
                availableColor
            );

            return;
        }

        // -------------------------------------------------
        // SWIPE ACTIVO
        // -------------------------------------------------

        if (CurrentMode ==
            InteractionMode.Swipe)
        {
            SetButtonState(
                dragButton,
                true,
                availableColor
            );

            SetButtonState(
                swipeButton,
                true,
                activeColor
            );

            return;
        }

        // -------------------------------------------------
        // CERDITO SELECCIONADO,
        // PERO NINGÚN MODO ACTIVO
        // -------------------------------------------------

        SetButtonState(
            dragButton,
            true,
            availableColor
        );

        SetButtonState(
            swipeButton,
            true,
            availableColor
        );
    }

    // =====================================================
    // CAMBIAR ESTADO DE BOTÓN
    // =====================================================

    private void SetButtonState(
        Button button,
        bool interactable,
        Color color)
    {
        if (button == null)
            return;

        button.interactable =
            interactable;

        Image image =
            button.GetComponent<Image>();

        if (image != null)
        {
            image.color =
                color;
        }
    }
}