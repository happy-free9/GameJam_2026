using UnityEngine;

public class Ending2DPlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!canMove)
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        if (!canMove)
            return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove && rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}