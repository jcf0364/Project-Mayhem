using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerMovement : MonoBehaviour
{
    // Movement
    public float speed = 5f;
    public float jumpForce = 7f;

    // Player-specific controls
    public Key leftKey;
    public Key rightKey;
    public Key jumpKey;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Movement state
    private bool isGrounded;
    private float facingDirection = 1f;
    public float FacingDirection => facingDirection;

    private bool inputEnabled = true;
    public void SetInputEnabled(bool enabled) => inputEnabled = enabled;

    // Screen boundaries
    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float playerHalfHeight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get the screen size in world coordinates
        screenBounds = Camera.main.ScreenToWorldPoint(
            new Vector2(Screen.width, Screen.height)
        );

        // Get half the size of the player sprite
        if (spriteRenderer != null)
        {
            playerHalfWidth = spriteRenderer.bounds.extents.x;
            playerHalfHeight = spriteRenderer.bounds.extents.y;
        }
    }

    void Update()
    {
        HandleMovement();
    }

    void LateUpdate()
    {
        var knockback = GetComponent<KnockbackReceiver>();
        if (knockback != null && knockback.InHitstun) return;

        KeepPlayerInBounds();
    }

    void HandleMovement()
    {
        if (!inputEnabled)
        {
            return;
        }

        float move = 0f;

        if (Keyboard.current[leftKey].isPressed)
        {
            move = -1f;
            facingDirection = -1f;
        }

        if (Keyboard.current[rightKey].isPressed)
        {
            move = 1f;
            facingDirection = 1f;
        }

        rb.linearVelocity = new Vector2(
            move * speed,
            rb.linearVelocity.y
        );

        if (Keyboard.current[jumpKey].wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;
        }

        if (Mathf.Abs(move) > 0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * facingDirection;
            transform.localScale = s;
        }
    }


    void KeepPlayerInBounds()
    {
        Vector2 pos = transform.position;

        float clampedX = Mathf.Clamp(
            pos.x,
            -screenBounds.x + playerHalfWidth,
            screenBounds.x - playerHalfWidth
        );

        pos.x = clampedX;

        float clampedY = Mathf.Clamp(
            pos.y,
            -screenBounds.y - 1f,
            screenBounds.y - playerHalfHeight
        );

        pos.y = clampedY;

        if (pos.y > -screenBounds.y)
        {
            transform.position = pos;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}