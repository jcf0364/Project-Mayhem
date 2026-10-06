using UnityEngine;

/// Survives scene loads. Holds what the player picked.
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Selections")]
    public int player1CharacterIndex = 0;
    public int player2CharacterIndex = 1;
    public int arenaIndex = 0;

    [Header("Profiles")]
    public string player1Name = "Player 1";
    public string player2Name = "Player 2";

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
    }
}