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

    public void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    /// Called by Hitbox2D when a hit connects.
    public void ReceiveHit(HitInfo info)
    {
        info.Damage = Mathf.RoundToInt(info.Damage * damageMultiplier);
        TotalDamageTaken += info.Damage;

        if (logHits)
            Debug.Log($"{transform.root.name} hit for {info.Damage} " + $"(total {TotalDamageTaken}) by {info.Source.name}");

        
        OnHitReceived?.Invoke(info);

    }

    public void ResetDamageCounter() => TotalDamageTaken = 0;
}
