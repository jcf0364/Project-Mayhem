using UnityEngine;

public class AIDifficultySelection : MonoBehaviour
{
    // Saves the selected difficulty and starts the AI battle.
    private void StartAIBattle(AIController.Difficulty difficulty)
    {
        PlayerPrefs.SetInt("AIDifficulty", (int)difficulty);
        // PvAI has no character select, so pick Player 1's character at random.
        // The AI (Player 2) gets the other one.
        string character = Random.value < 0.5f ? "Mayhem" : "Angelo";
        PlayerPrefs.SetString("Character", character);
        // PvAI never plays Sudden Death
        if (GameSession.Instance != null)
            GameSession.Instance.matchMode = MatchMode.ThreeRound;
        PlayerPrefs.Save();

        // Go through SceneLoader so Time.timeScale gets reset.
        // Loading directly meant a paused game stayed frozen.
         SceneLoader.Load("Battle AI");
    }

    // Called by the Easy button
    public void SelectEasy()
    {
        StartAIBattle(AIController.Difficulty.Easy);
    }

    // Called by the Medium button
    public void SelectMedium()
    {
        StartAIBattle(AIController.Difficulty.Medium);
    }

    // Called by the Hard button
    public void SelectHard()
    {
        StartAIBattle(AIController.Difficulty.Hard);
    }
}