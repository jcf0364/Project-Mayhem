using System.Collections;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [Header("Fighters")]
    [SerializeField] private PlayerHealth player1;
    [SerializeField] private PlayerHealth player2;

    [Header("Lives")]
    [SerializeField] private int livesPerPlayer = 3;
    [SerializeField] private float roundResetDelay = 1.5f;

    [Header("UI")]
    [SerializeField] private TMP_Text livesTextP1;
    [SerializeField] private TMP_Text livesTextP2;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TMP_Text winnerText;

    private int livesP1;
    private int livesP2;
    private bool matchOver;

    private Vector3 startPosP1;
    private Vector3 startPosP2;

    private void Awake()
    {
        // Remember where the fighters were placed in the scene.
        startPosP1 = player1.transform.position;
        startPosP2 = player2.transform.position;
    }

    private void OnEnable()
    {
        player1.OnDied += HandleP1Died;
        player2.OnDied += HandleP2Died;
    }

    private void OnDisable()
    {
        player1.OnDied -= HandleP1Died;
        player2.OnDied -= HandleP2Died;
    }

    private void Start()
    {
        BeginMatch();
    }

    public void BeginMatch()
    {
        matchOver = false;
        livesP1 = livesPerPlayer;
        livesP2 = livesPerPlayer;

        if (winScreen != null) winScreen.SetActive(false);

        UpdateLivesDisplay();
        ResetRoundState();
    }

    private void HandleP1Died() => HandleDeath(1);
    private void HandleP2Died() => HandleDeath(2);

    private void HandleDeath(int loserId)
    {
        // Ignore anything after the match is decided.
        if (matchOver) return;

        if (loserId == 1) livesP1--;
        else              livesP2--;

        UpdateLivesDisplay();

        SetAllControlEnabled(false);

        if (livesP1 <= 0 || livesP2 <= 0)
        {
            int winnerId = livesP1 <= 0 ? 2 : 1;
            StartCoroutine(EndMatchRoutine(winnerId));
        }
        else
        {
            StartCoroutine(ResetRoundRoutine());
        }
    }

    private IEnumerator ResetRoundRoutine()
    {
        yield return new WaitForSeconds(roundResetDelay);
        ResetRoundState();
    }

    private void ResetRoundState()
    {
        player1.transform.position = startPosP1;
        player2.transform.position = startPosP2;

        ZeroVelocity(player1.gameObject);
        ZeroVelocity(player2.gameObject);

        // Health bars follow automatically via OnHealthChanged.
        player1.ResetHealth();
        player2.ResetHealth();

        SetAllControlEnabled(true);
    }

    private void ZeroVelocity(GameObject fighter)
    {
        var rb = fighter.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void SetAllControlEnabled(bool enabled)
    {
        SetControlEnabled(player1.gameObject, enabled);
        SetControlEnabled(player2.gameObject, enabled);
    }

    private void SetControlEnabled(GameObject fighter, bool enabled)
    {
        var move = fighter.GetComponent<LocalPlayerMovement>();
        if (move != null) move.SetInputEnabled(enabled);

        var attack = fighter.GetComponent<PlayerAttack>();
        if (attack != null) attack.SetInputEnabled(enabled);
    }

    private void UpdateLivesDisplay()
    {
        if (livesTextP1 != null)
            livesTextP1.text = $"Lives: {Mathf.Max(livesP1, 0)}";

        if (livesTextP2 != null)
            livesTextP2.text = $"Lives: {Mathf.Max(livesP2, 0)}";
    }

    private IEnumerator EndMatchRoutine(int winnerId)
    {
        matchOver = true;

        yield return new WaitForSeconds(roundResetDelay);

        if (winnerText != null)
            winnerText.text = $"PLAYER {winnerId} WINS";

        if (winScreen != null)
            winScreen.SetActive(true);
    }

    // --- Button handler ---

    public void RestartMatch()
    {
        BeginMatch();
    }
}