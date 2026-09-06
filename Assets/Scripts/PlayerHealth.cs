using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header ("Damage Response")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    private int currentHealth;
    private float invulnerableUntil;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Broadcast in Start, not Awake - subscribers register first.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (IsDead) return;
        if (IsInvulnerable) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        invulnerableUntil = Time.time + invulnerabilityDuration;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (IsDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        invulnerableUntil = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        OnDied?.Invoke();
        Debug.Log($"{name} died.");
    }
}
