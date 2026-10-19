using UnityEngine;

/// Survives scene loads. Holds what the player picked.
// The two match types on the match mode screen
public enum MatchMode { ThreeRound, SuddenDeath }
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Selections")]
    public int player1CharacterIndex = 0;
    public int player2CharacterIndex = 1;
    public int arenaIndex = 0;
    [Header("Match")]
    public MatchMode matchMode = MatchMode.ThreeRound;

    [Header("Profiles")]
    // Ids, not names. If a player renames themselves mid-session the
    // match still points at the right profile.
    public string player1ProfileId;
    public string player2ProfileId;

    // Slot two can be unclaimed so one person can play alone.
    public bool player2IsGuest;

    /// Name to show on the results screen and write to the leaderboard.
    public string GetPlayer1Name()
    {
        return ProfileStore.GetDisplayName(player1ProfileId, "Player 1");
    }

    public string GetPlayer2Name()
    {
        if (player2IsGuest) return "Guest";
        return ProfileStore.GetDisplayName(player2ProfileId, "Player 2");
    }

    private void Awake()
    {
        // Only one allowed. Kill any duplicate.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetSelections()
    {
        player1CharacterIndex = 0;
        player2CharacterIndex = 1;
        arenaIndex = 0;
        matchMode = MatchMode.ThreeRound;
    }
}