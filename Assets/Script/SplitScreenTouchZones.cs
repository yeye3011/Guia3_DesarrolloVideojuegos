using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class SplitScreenTouchZones : MonoBehaviour
{
    public Vector2 MoveAxis { get; private set; }
    public Vector2 LookDelta { get; private set; }

    [SerializeField] private float dragRadius = 120f;

    private Vector2 leftStart;
    private Vector2 previousRight;

    private int leftFinger = -1;
    private int rightFinger = -1;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        LookDelta = Vector2.zero;

        foreach (var touch in Touch.activeTouches)
        {
            int id = touch.finger.index;

            bool isLeftSide =
                touch.screenPosition.x < Screen.width * 0.5f;

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (isLeftSide && leftFinger < 0)
                {
                    leftFinger = id;
                    leftStart = touch.screenPosition;
                }
                else if (!isLeftSide && rightFinger < 0)
                {
                    rightFinger = id;
                    previousRight = touch.screenPosition;
                }
            }

            if (id == leftFinger)
            {
                MoveAxis =
                    Vector2.ClampMagnitude(
                        (touch.screenPosition - leftStart)
                        / dragRadius,
                        1f
                    );
            }

            if (id == rightFinger)
            {
                LookDelta =
                    touch.screenPosition - previousRight;

                previousRight = touch.screenPosition;
            }

            if (touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Ended ||
                touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                if (id == leftFinger)
                {
                    leftFinger = -1;
                    MoveAxis = Vector2.zero;
                }

                if (id == rightFinger)
                {
                    rightFinger = -1;
                }
            }
        }
    }
}