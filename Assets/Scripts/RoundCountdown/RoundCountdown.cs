using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// Countdown run between rounds. Separate from GameCountDown,
/// which handles the initial match start.
public class RoundCountdown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float stepDuration = 1f;
    [SerializeField] private float goDuration = 0.6f;

    private readonly List<LocalPlayerMovement> players = new List<LocalPlayerMovement>();

    private void Awake()
    {
        players.AddRange(FindObjectsByType<LocalPlayerMovement>());
    }

    /// Freezes both fighters, counts down, then returns control.
    /// Call with: yield return countdown.RunCountdown();
    public IEnumerator RunCountdown()
    {
        SetPlayersEnabled(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        yield return ShowStep("3");
        yield return ShowStep("2");
        yield return ShowStep("1");

        if (countdownText != null)
            countdownText.text = "GO!";

        // Control returns ON "GO!", not after it clears.
        SetPlayersEnabled(true);

        yield return new WaitForSeconds(goDuration);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private IEnumerator ShowStep(string label)
    {
        if (countdownText != null)
            countdownText.text = label;

        yield return new WaitForSeconds(stepDuration);
    }

    private void SetPlayersEnabled(bool enabled)
    {
        foreach (var player in players)
        {
            if (player == null) continue;

            player.SetInputEnabled(enabled);

            var attack = player.GetComponent<PlayerAttack>();
            if (attack != null) attack.SetInputEnabled(enabled);

            if (!enabled)
            {
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }
}