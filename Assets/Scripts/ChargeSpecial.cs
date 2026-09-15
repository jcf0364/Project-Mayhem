using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class ChargeSpecial : MonoBehaviour, ISpecialAttack
{
    [Header("Charge")]
    [SerializeField] private Key holdKey = Key.L;
    [SerializeField] private float maxChargeTime = 1.2f;
    [SerializeField] private float minDamageMultiplier = 0.4f;
    [SerializeField] private float lungeSpeed = 12f;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color chargeTint = new Color(1f, 0.6f, 0.6f);

    private Rigidbody2D rb;
    private LocalPlayerMovement move;
    private Color originalColour;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        move = GetComponent<LocalPlayerMovement>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColour = spriteRenderer.color;
    }

    public IEnumerator Execute(AttackData data, HitBox2D hitbox)
    {
        var kb = Keyboard.current;

        // --- Charge phase: rooted in place while held ---
        float charge = 0f;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        while (kb != null && kb[holdKey].isPressed && charge < maxChargeTime)
        {
            charge += Time.deltaTime;

            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(
                    originalColour, chargeTint, charge / maxChargeTime);

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColour;

        // --- Release: damage scales with charge ---
        float t = Mathf.Clamp01(charge / maxChargeTime);
        float multiplier = Mathf.Lerp(minDamageMultiplier, 1f, t);

        var scaled = new AttackData
        {
            attackName = data.attackName,
            damage = Mathf.RoundToInt(data.damage * multiplier),
            knockback = data.knockback * multiplier,
            hitstun = data.hitstun,
            hitboxSize = data.hitboxSize,
            hitboxOffset = data.hitboxOffset
        };

        // Lunge forward.
        float facing = move != null ? move.FacingDirection : 1f;
        rb.linearVelocity = new Vector2(facing * lungeSpeed, rb.linearVelocity.y);

        hitbox.Configure(scaled);
        hitbox.Activate();
        yield return new WaitForSeconds(data.activeTime);
        hitbox.Deactivate();

        yield return new WaitForSeconds(data.recoveryTime);
    }
}