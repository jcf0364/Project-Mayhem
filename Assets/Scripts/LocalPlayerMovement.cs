using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LocalPlayerMovement : MonoBehaviour
{
    // Movement
    public float speed = 5f;
    public float jumpForce = 7f;

    // Player-specific controls
    public Key leftKey;
    public Key rightKey;
    public Key jumpKey;
    public Key attackKey;

    // Combat
    public float attackRange = 1f;
    public float baseKnockback = 5f;
    public float knockbackUpForce = 3f;
    public float damagePercent = 0f;
    public float damagePerHit = 10f;

    // Sound effects
    public AudioClip jumpSound;
    public AudioClip landingSound;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip specialMoveSound;

    // UI
    public TMP_Text damageText;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // Movement state
    private bool isGrounded;
    private float facingDirection = 1f;

    // Screen boundaries
    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float playerHalfHeight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        screenBounds = Camera.main.ScreenToWorldPoint(
            new Vector2(Screen.width, Screen.height)
        );

        if (spriteRenderer != null)
        {
            playerHalfWidth = spriteRenderer.bounds.extents.x;
            playerHalfHeight = spriteRenderer.bounds.extents.y;
        }

        UpdateDamageText();
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
        KeepPlayerInBounds();
    }

    void HandleMovement()
    {
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

        // Jump
        if (Keyboard.current[jumpKey].wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            if (jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }

            isGrounded = false;
        }
    }

    void HandleAttack()
    {
        if (Keyboard.current[attackKey].wasPressedThisFrame)
        {
            Attack();
        }
    }

    void Attack()
    {
        // Whoosh sound when attack button is pressed
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Vector2 attackPosition =
            (Vector2)transform.position +
            new Vector2(facingDirection, 0f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPosition,
            attackRange
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != gameObject &&
                hit.CompareTag("Player"))
            {
                LocalPlayerMovement otherPlayer =
                    hit.GetComponent<LocalPlayerMovement>();

                Rigidbody2D otherRb =
                    hit.GetComponent<Rigidbody2D>();

                if (otherPlayer != null && otherRb != null)
                {
                    // Quieter hit sound only when attack connects
                    if (hitSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(hitSound, 0.4f);
                    }

                    otherPlayer.damagePercent += damagePerHit;
                    otherPlayer.UpdateDamageText();

                    float knockback =
                        baseKnockback +
                        (otherPlayer.damagePercent * 0.05f);

                    Vector2 knockbackDirection =
                        new Vector2(
                            facingDirection,
                            1f
                        ).normalized;

                    otherRb.AddForce(
                        new Vector2(
                            knockbackDirection.x * knockback,
                            knockbackDirection.y * knockbackUpForce
                        ),
                        ForceMode2D.Impulse
                    );
                }
            }
        }
    }

    // Ready for when special moves are implemented
    public void PlaySpecialMoveSound()
    {
        if (specialMoveSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(specialMoveSound);
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

    void UpdateDamageText()
    {
        if (damageText != null)
        {
            string playerLabel =
                gameObject.name == "Player1" ? "P1" : "P2";

            damageText.text =
                playerLabel + ": " + damagePercent + "%";
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Play landing sound only when arriving on the ground
            if (!isGrounded &&
                landingSound != null &&
                audioSource != null)
            {
                audioSource.PlayOneShot(landingSound, 0.5f);
            }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            (Vector2)transform.position +
            new Vector2(facingDirection, 0f),
            attackRange
        );
    }
}