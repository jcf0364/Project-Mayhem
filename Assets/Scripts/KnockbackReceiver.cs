using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver : MonoBehaviour
{
    [Header("Knockback")]
    [Tooltip("Lower = heavier character, harder to move. Impulse depends on mass.")]
    [SerializeField] private float knockbackMultiplier = 1f;

    [Tooltip("Upward component added to the push. 0 = flat, 1 = strong pop-up")]
    [SerializeField] private float knockbackLift = 0.5f;

    [Header("Hitstun")]
    [SerializeField] private bool applyHitstun = true;

    private Rigidbody2D rb;
    private Coroutine hitstunRoutine;

    public bool InHitstun { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        var hurtbox = GetComponentInChildren<HurtBox2D>();
        if (hurtbox != null)
            hurtbox.OnHitReceived += HandleHit;
    }

    private void OnDisable()
    {
        var hurtbox = GetComponentInChildren<HurtBox2D>();
        if (hurtbox != null)
            hurtbox.OnHitReceived -= HandleHit;
    }

    private void HandleHit(HitInfo info)
    {
        if (info.KnockbackForce > 0f)
            ApplyKnockback(info);
        
        if (applyHitstun && info.HitstunDuration > 0f)
            StartHitstun(info.HitstunDuration);
    }

    private void ApplyKnockback(HitInfo info)
    {
        // Direction: away from where the attack came from.
        Vector2 dir = ((Vector2)transform.position - info.SourcePosition).normalized;

        // Exact overlap would give a zero vector and then NaN. Fall back.
        if (dir.sqrMagnitude < 0.001f)
            dir = Vector2.right;
        
        dir.y = knockbackLift;
        dir = dir.normalized;

        // Clear existing motion so distance doesn't depend on current speed.
        rb.linearVelocity = Vector2.zero;

        // Impulse, not Force - a hit is instantaneous.
        rb.AddForce(dir * info.KnockbackForce * knockbackMultiplier, ForceMode2D.Impulse);
    }

    private void StartHitstun(float duration)
    {
        if (hitstunRoutine != null) StopCoroutine(hitstunRoutine);
        hitstunRoutine = StartCoroutine(HitstunRoutine(duration));
    }

    private IEnumerator HitstunRoutine(float duration)
    {
        InHitstun = true;
        SetControlEnabled(false);

        yield return new WaitForSeconds(duration);

        SetControlEnabled(true);
        InHitstun = false;
        hitstunRoutine = null;
    }

    private void SetControlEnabled(bool enabled)
    {
        var attack = GetComponent<PlayerAttack>();
        if (attack != null) attack.SetInputEnabled(enabled);


    }
}
