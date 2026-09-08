using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField]
    private CharacterController characterController;

    // =====================================================
    // MOVIMIENTO
    // =====================================================

    [Header("Movimiento")]
    [SerializeField]
    private float moveSpeed = 5f;

    // =====================================================
    // ROTACIÓN Y
    // =====================================================

    [Header("Rotación Y")]
    [SerializeField]
    private float rotationSpeed = 100f;

    // -1 = izquierda
    //  0 = sin rotación
    //  1 = derecha
    private float rotationInput = 0f;

    // =====================================================
    // INPUT
    // =====================================================

    [Header("Input")]
    [SerializeField]
    private InputActionReference moveAction;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        // Si no se asignó manualmente,
        // buscamos el CharacterController del Player.
        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }

        if (characterController == null)
        {
            Debug.LogError(
                "Player necesita un CharacterController."
            );
        }
    }

    // =====================================================
    // INPUT ACTION
    // =====================================================

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        MovePlayer();
        RotatePlayer();
    }

    // =====================================================
    // MOVIMIENTO X / Z
    // =====================================================

    private void MovePlayer()
    {
        if (moveAction == null ||
            characterController == null)
        {
            return;
        }

        // Leer joystick.
        Vector2 input =
            moveAction.action.ReadValue<Vector2>();

        // Movimiento relativo a la dirección
        // hacia la que mira el Player.
        Vector3 direction =
            transform.forward * input.y +
            transform.right * input.x;

        direction =
            Vector3.ClampMagnitude(
                direction,
                1f
            );

        Vector3 movement =
            direction *
            moveSpeed *
            Time.deltaTime;

        characterController.Move(
            movement
        );
    }

    // =====================================================
    // ROTACIÓN SOBRE Y
    // =====================================================

    private void RotatePlayer()
    {
        if (Mathf.Approximately(
            rotationInput,
            0f))
        {
            return;
        }

        float rotationAmount =
            rotationInput *
            rotationSpeed *
            Time.deltaTime;

        transform.Rotate(
            Vector3.up *
            rotationAmount
        );
    }

    // =====================================================
    // ROTAR A LA IZQUIERDA
    // =====================================================

    public void RotateLeftStart()
    {
        rotationInput = -1f;
    }

    // =====================================================
    // ROTAR A LA DERECHA
    // =====================================================

    public void RotateRightStart()
    {
        rotationInput = 1f;
    }

    // =====================================================
    // DETENER ROTACIÓN
    // =====================================================

    public void StopRotation()
    {
        rotationInput = 0f;
    }
}