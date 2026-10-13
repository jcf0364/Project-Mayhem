using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HurtBox2D : MonoBehaviour
{
    [Header("Damage Routing")]
    [Tooltip("2.5 on a head hurtbox gives critical hits for free.")]
    [SerializeField] private float damageMultiplier = 1f;

    [Header("Debug")]
    [SerializeField] private bool logHits = true;

    /// Fires whenever this hurtbox is struck.
    /// Health, score, VFX and audio all subscribe here later.
    public event Action<HitInfo> OnHitReceived;

    public int TotalDamageTaken { get; private set; }

    private BlockState blockState;

    public void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        // BlockState lives on the fighter; this hurtbox is a child of it.
        blockState = GetComponentInParent<BlockState>();
    }

    /// Single funnel for every incoming hit. The block reduction happens
    /// here, not in PlayerHealth, because health, knockback and match stats
    /// all subscribe to OnHitReceived — softening the HitInfo once means
    /// all three see the blocked values without any of them knowing that
    /// blocking exists.
    public void ReceiveHit(HitInfo info)
    {
        // Hurtbox multiplier first — a head hurtbox should scale the raw
        // damage before the guard reduces it, not after.
        info.Damage = Mathf.RoundToInt(info.Damage * damageMultiplier);

        bool blocked = blockState != null
                       && blockState.IsBlockingAgainst(info.SourcePosition);

        if (blocked)
        {
            info.Damage = Mathf.RoundToInt(info.Damage * blockState.DamageThrough);
            info.KnockbackForce *= blockState.KnockbackThrough;

            // Zeroed, not reduced. This is the real payoff of blocking:
            // the attacker is still in recovery while the defender can
            // already act. Merely shortening hitstun would leave both
            // recovering together and give the defender nothing.
            info.HitstunDuration = 0f;

            blockState.PlayBlockSound();
        }

        TotalDamageTaken += info.Damage;

        if (logHits)
            Debug.Log($"{transform.root.name} {(blocked ? "BLOCKED" : "hit")} " +
                      $"for {info.Damage} (total {TotalDamageTaken}) by {info.Source.name}");

        OnHitReceived?.Invoke(info);
    }

    public void ResetDamageCounter() => TotalDamageTaken = 0;
}
