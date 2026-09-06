using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth))]
public class HealthDebugTester : MonoBehaviour
{
    [SerializeField] private int damagePerHit = 12;

    private PlayerHealth health;

    private void Awake() => health = GetComponent<PlayerHealth>();

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) health.TakeDamage(damagePerHit);
        if (kb.digit2Key.wasPressedThisFrame) health.Heal(damagePerHit);
        if (kb.digit3Key.wasPressedThisFrame) health.ResetHealth();
    }
}