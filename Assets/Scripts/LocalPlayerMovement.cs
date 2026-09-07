using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LocalPlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;

    public Key leftKey;
    public Key rightKey;
    public Key jumpKey;
    public Key attackKey;

    public float attackRange = 1f;
    public float baseKnockback = 5f;
    public float knockbackUpForce = 3f;

    public float damagePercent = 0f;
    public float damagePerHit = 10f;

    public TMP_Text damageText;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float facingDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateDamageText();
    }

    void Update()
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

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        if (Keyboard.current[jumpKey].wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }

        if (Keyboard.current[attackKey].wasPressedThisFrame)
        {
            Attack();
        }
    }

    void Attack()
    {
        Vector2 attackPosition =
            (Vector2)transform.position +
            new Vector2(facingDirection, 0);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPosition,
            attackRange
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Player"))
            {
                LocalPlayerMovement otherPlayer =
                    hit.GetComponent<LocalPlayerMovement>();

                Rigidbody2D otherRb =
                    hit.GetComponent<Rigidbody2D>();

                if (otherPlayer != null && otherRb != null)
                {
                    otherPlayer.damagePercent += damagePerHit;
                    otherPlayer.UpdateDamageText();

                    float knockback =
                        baseKnockback +
                        (otherPlayer.damagePercent * 0.05f);

                    Vector2 knockbackDirection =
                        new Vector2(facingDirection, 1f).normalized;

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
            new Vector2(facingDirection, 0),
            attackRange
        );
    }
}