using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HurtBox2D : MonoBehaviour
{
    [Header("Damage Routing")]
    [Tooltip("2.5 on a head hurtbox gives critical hits for free.")]
    [SerializeField] private float damageMultiplier = 1f;
    public GameObject player;

    [Header("Debug")]
    [SerializeField] private bool logHits = true;

    /// Fires whenever this hurtbox is struck.
    /// Health, score, VFX and audio all subscribe here later.
    public event Action<HitInfo> OnHitReceived;

    public int TotalDamageTaken { get; private set; }

    private BlockState blockState;
    private int mult;
    private float timer;

    public void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        
        // BlockState lives on the fighter; this hurtbox is a child of it.
        blockState = GetComponentInParent<BlockState>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 10)
        {
            timer = 0;
            damageMultiplier = 1;
            PlayerPrefs.SetString("ID", " ");
        }
        
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
        powerups();
        info.Damage = Mathf.RoundToInt(info.Damage * damageMultiplier);
        
        Debug.Log(damageMultiplier);

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
    void powerups()
    {
        Debug.Log(PlayerPrefs.GetString("ID"));
        if (player.name == PlayerPrefs.GetString("ID"))
        {   
            Debug.Log("multiplier active");
            
            damageMultiplier = PlayerPrefs.GetFloat("multiplier", 1f);
        }
    }
    public void ResetDamageCounter() => TotalDamageTaken = 0;
}
