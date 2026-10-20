using UnityEngine;
using UnityEngine.Events;

/// Match mode panel in ProfileSelect (shown after picking PvP).
public class MatchModeSelect : MonoBehaviour
{
    [Tooltip("What happens after a mode is picked. Move the PvP button's old On Click actions here.")]
    [SerializeField] private UnityEvent onModePicked;

    [Header("Panels (for the Back button)")]
    [SerializeField] private GameObject gameModePanel;   // the PvP / PvAI panel
    [SerializeField] private GameObject matchModePanel;  // this panel

    // Hooked up to the buttons' On Click ()
    public void PickThreeRound() => Pick(MatchMode.ThreeRound);
    public void PickSuddenDeath() => Pick(MatchMode.SuddenDeath);

    public void Back()
    {
        if (matchModePanel != null) matchModePanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(true);
    }

    private void Pick(MatchMode mode)
    {
        if (GameSession.Instance != null)
            GameSession.Instance.matchMode = mode;

        onModePicked?.Invoke();
    }
}