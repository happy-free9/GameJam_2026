using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Rigidbody2D))]
public class SidePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float x = 0f;
        float y = 0f;

        Keyboard keyboard = Keyboard.current;

        if (IsPressed(keyboard, HotelHungerRuntimeManager.GetMovementBinding(HotelHungerRuntimeManager.MovementDirection.Left)) ||
            keyboard.leftArrowKey.isPressed)
            x -= 1f;

        if (IsPressed(keyboard, HotelHungerRuntimeManager.GetMovementBinding(HotelHungerRuntimeManager.MovementDirection.Right)) ||
            keyboard.rightArrowKey.isPressed)
            x += 1f;

        if (IsPressed(keyboard, HotelHungerRuntimeManager.GetMovementBinding(HotelHungerRuntimeManager.MovementDirection.Up)) ||
            keyboard.upArrowKey.isPressed)
            y += 1f;

        if (IsPressed(keyboard, HotelHungerRuntimeManager.GetMovementBinding(HotelHungerRuntimeManager.MovementDirection.Down)) ||
            keyboard.downArrowKey.isPressed)
            y -= 1f;

        moveInput = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private static bool IsPressed(Keyboard keyboard, Key key)
    {
        if (keyboard == null || key == Key.None)
        {
            return false;
        }

        KeyControl control = keyboard[key];
        return control != null && control.isPressed;
    }
}
