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

    [Header("Sudden Death")]
    [Tooltip("Health both fighters start with. 0.1 = 10% of max health.")]
    [Range(0.01f, 1f)]
    [SerializeField] private float suddenDeathHealth = 0.1f;
    [SerializeField] private GameObject suddenDeathBanner;
    [SerializeField] private float suddenDeathBannerTime = 2f;

    [Tooltip("Only used when there's no GameSession, e.g. pressing Play in this scene directly.")]
    [SerializeField] private MatchMode testMode = MatchMode.ThreeRound;

    [Header("UI")]
    [SerializeField] private TMP_Text livesTextP1;
    [SerializeField] private TMP_Text livesTextP2;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TMP_Text winnerText;

    [Header("Stats")]
    [SerializeField] private MatchStats stats;
    [SerializeField] private TMP_Text statsTextP1;
    [SerializeField] private TMP_Text statsTextP2;
    [SerializeField] private TMP_Text matchTimeText;

    [Header("Round Flow")]
    [SerializeField] private RoundCountdown roundCountdown;
    [SerializeField] private GameObject mainMenuPanel;

    private bool firstMatch = true;
    private int livesP1;
    private int livesP2;
    private bool matchOver;
    private MatchMode mode;

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
        StopAllCoroutines();
        matchOver = false;

        // Read the mode picked on the match mode screen.
        mode = GameSession.Instance != null
            ? GameSession.Instance.matchMode
            : testMode;

        // AC 2: Sudden Death gets one life each.
        int startLives = mode == MatchMode.SuddenDeath ? 1 : livesPerPlayer;
        livesP1 = startLives;
        livesP2 = startLives;

        if (stats != null) stats.ResetStats();

        if (winScreen != null) winScreen.SetActive(false);
        if (suddenDeathBanner != null) suddenDeathBanner.SetActive(false);

        UpdateLivesDisplay();
        ResetRoundState();

        if (mode == MatchMode.SuddenDeath)
            StartSuddenDeath();
    }

    private void ResetRoundState()
    {
        player1.transform.position = startPosP1;
        player2.transform.position = startPosP2;

        ZeroVelocity(player1.gameObject);
        ZeroVelocity(player2.gameObject);

        player1.ResetHealth();
        player2.ResetHealth();

        SetAllControlEnabled(true);
    }

    // --- Sudden Death ---

    private void StartSuddenDeath()
    {
        // AC 2: both fighters on low health. ResetHealth already ran,
        // so both bars drain from full to the same point.
        player1.SetHealth(LowHealth(player1));
        player2.SetHealth(LowHealth(player2));

        if (suddenDeathBanner != null)
            StartCoroutine(BannerRoutine());
    }

    private int LowHealth(PlayerHealth fighter)
    {
        return Mathf.Max(1, Mathf.RoundToInt(fighter.MaxHealth * suddenDeathHealth));
    }

    private IEnumerator BannerRoutine()
    {
        suddenDeathBanner.SetActive(true);
        yield return new WaitForSeconds(suddenDeathBannerTime);
        suddenDeathBanner.SetActive(false);
    }

    // --- Deaths and rounds (unchanged) ---

    private void HandleP1Died() => HandleDeath(1);
    private void HandleP2Died() => HandleDeath(2);

    private void HandleDeath(int loserId)
    {
        // Ignore anything after the match is decided.
        if (matchOver) return;

        if (loserId == 1) livesP1--;
        else livesP2--;

        UpdateLivesDisplay();

        if (stats != null)
            stats.RecordRoundWin(loserId == 1 ? 2 : 1);

        SetAllControlEnabled(false);

        // AC 3: in Sudden Death, lives start at 1, so the first KO
        // always lands here and ends the match.
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

        player1.transform.position = startPosP1;
        player2.transform.position = startPosP2;

        ZeroVelocity(player1.gameObject);
        ZeroVelocity(player2.gameObject);

        player1.ResetHealth();
        player2.ResetHealth();

        if (roundCountdown != null)
            yield return roundCountdown.RunCountdown();
        else
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
        if (stats != null) stats.StopTimer();

        yield return new WaitForSeconds(roundResetDelay);

        if (winnerText != null)
        {
            // Profile name when one is signed in, "Player 1" / "Guest"
            // otherwise. GameSession handles the fallback.
            string winnerName = GameSession.Instance != null
                ? (winnerId == 1
                    ? GameSession.Instance.GetPlayer1Name()
                    : GameSession.Instance.GetPlayer2Name())
                : $"PLAYER {winnerId}";

            winnerText.text = $"{winnerName.ToUpper()} WINS";
        }

        ShowStats();

        if (winScreen != null)
            winScreen.SetActive(true);
    }

    private void ShowStats()
    {
        if (stats == null) return;

        if (statsTextP1 != null)
            statsTextP1.text =
                "<b>PLAYER 1</b>\n" +
                $"Rounds won: {stats.RoundsWonP1}\n" +
                $"Lives left: {Mathf.Max(livesP1, 0)}\n" +
                $"Damage dealt: {stats.DamageDealtP1}\n" +
                $"Hits landed: {stats.HitsLandedP1}";

        if (statsTextP2 != null)
            statsTextP2.text =
                "<b>PLAYER 2</b>\n" +
                $"Rounds won: {stats.RoundsWonP2}\n" +
                $"Lives left: {Mathf.Max(livesP2, 0)}\n" +
                $"Damage dealt: {stats.DamageDealtP2}\n" +
                $"Hits landed: {stats.HitsLandedP2}";

        if (matchTimeText != null)
            matchTimeText.text = $"Match time: {stats.FormatDuration()}";
    }

    // --- Button handlers ---

    public void RestartMatch()
    {
        StopAllCoroutines();
        BeginMatch();

        if (firstMatch)
        {
            firstMatch = false;
            return;
        }

        StartCoroutine(RestartCountdownRoutine());
    }

    private IEnumerator RestartCountdownRoutine()
    {
        if (roundCountdown != null)
            yield return roundCountdown.RunCountdown();
    }

    public void ReturnToMenu()
    {
        SceneLoader.Load(SceneLoader.MainMenu);
    }

    /// Called by the main menu's PLAY button.
    public void StartNewMatch()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        RestartMatch();
    }
}