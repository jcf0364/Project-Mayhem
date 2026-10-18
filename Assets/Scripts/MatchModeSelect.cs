using UnityEngine;

/// Buttons on the match mode screen (after picking PvP).
public class MatchModeSelect : MonoBehaviour
{
    [Tooltip("Scene to load after a mode is picked. Use the scene your PvP button loaded before.")]
    [SerializeField] private string nextScene = SceneLoader.ProfileSelect;

    // Hooked up to the buttons' On Click ()
    public void PickThreeRound() => Pick(MatchMode.ThreeRound);
    public void PickSuddenDeath() => Pick(MatchMode.SuddenDeath);
    public void Back() => SceneLoader.Load(SceneLoader.MainMenu);

    private void Pick(MatchMode mode)
    {
        if (GameSession.Instance != null)
            GameSession.Instance.matchMode = mode;

        SceneLoader.Load(nextScene);
    }
}