using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage; // red, front
    [SerializeField] private Image chipImage; // yellow, behind

    [Header("Main Bar")]
    [SerializeField] private float fillLerpSpeed = 25f;

    [Header("Chip Damage")]
    [SerializeField] private float chipDelay = 0.45f;
    [SerializeField] private float chipDrainSpeed = 0.6f;

    private float targetFill = 1f;
    private float chipHoldUntil;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    /// Bind at runtime, needed once fighters are spawned rather than
    /// placed in the scene.
    public void Bind(PlayerHealth health)
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;

        playerHealth = health;

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);

            targetFill = 1f;
            if (fillImage != null) fillImage.fillAmount = 1f;
            if (chipImage != null) chipImage.fillAmount = 1f;
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        float newTarget = max > 0 ? (float)current / max : 0f;

        // Only start the chip hold when health actually DROPS.
        if (newTarget < targetFill)
            chipHoldUntil = Time.time + chipDelay;

        // Healing or resetting: bring the chip layer straight back up.
        if (newTarget > targetFill && chipImage != null)
            chipImage.fillAmount = newTarget;

        targetFill = newTarget;
    }

    private void Update()
    {
        if (fillImage == null) return;

        // Main bar chases the target quickly.
        fillImage.fillAmount = Mathf.MoveTowards(
            fillImage.fillAmount, targetFill, fillLerpSpeed * Time.deltaTime);

        if (chipImage == null) return;

        // Chip waits, then drains slowly to meet the main bar.
        if (Time.time >= chipHoldUntil)
        {
            chipImage.fillAmount = Mathf.MoveTowards(
                chipImage.fillAmount, fillImage.fillAmount, chipDrainSpeed * Time.deltaTime);

        }

        //Chip can never sit below the main bar.
        if (chipImage.fillAmount < fillImage.fillAmount)
            chipImage.fillAmount = fillImage.fillAmount;
    }

}
