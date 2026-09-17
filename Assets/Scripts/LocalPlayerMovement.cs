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

    // Sound effects
    public AudioClip jumpSound;
    public AudioClip landingSound;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip specialMoveSound;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

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

    // Combat
    private PlayerAttack playerAttack;
    private KnockbackReceiver knockbackReceiver;

    // Animator
    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        playerAttack = GetComponent<PlayerAttack>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();

        screenBounds = Camera.main.ScreenToWorldPoint(
            new Vector2(Screen.width, Screen.height)
        );

        if (spriteRenderer != null)
        {
            playerHalfWidth = spriteRenderer.bounds.extents.x;
            playerHalfHeight = spriteRenderer.bounds.extents.y;
        }
    }

    void Update()
    {
        HandleMovement();
        UpdateAnimator();
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
            if (anim != null) anim.SetBool("isMoving", false);
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

        if (anim != null)
            anim.SetBool("isMoving", move != 0f);

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
                audioSource.PlayOneShot(jumpSound);

            isGrounded = false;
        }
    }

    private void UpdateAnimator()
    {
        if (anim == null) return;

        anim.SetBool("isJumping", !isGrounded);

        if (playerAttack != null)
            anim.SetBool("isAttacking", playerAttack.IsAttacking);

        if (knockbackReceiver != null)
            anim.SetBool("isKnockback", knockbackReceiver.InHitstun);
    }

    // --- Audio hooks for the combat system ---

    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound);
    }

    public void PlayHitSound()
    {
        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound, 0.4f);
    }

    public void PlaySpecialMoveSound()
    {
        if (specialMoveSound != null && audioSource != null)
            audioSource.PlayOneShot(specialMoveSound);
    }

    void KeepPlayerInBounds()
    {
        Vector2 pos = transform.position;

        pos.x = Mathf.Clamp(
            pos.x,
            -screenBounds.x + playerHalfWidth,
            screenBounds.x - playerHalfWidth
        );

        pos.y = Mathf.Clamp(
            pos.y,
            -screenBounds.y - 1f,
            screenBounds.y - playerHalfHeight
        );

        if (pos.y > -screenBounds.y)
            transform.position = pos;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isGrounded && landingSound != null && audioSource != null)
                audioSource.PlayOneShot(landingSound, 0.5f);

            isGrounded = true;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}