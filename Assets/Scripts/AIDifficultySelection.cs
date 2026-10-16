
using UnityEngine;
using UnityEngine.SceneManagement;

public class AIDifficultySelection : MonoBehaviour
{
    // Saves the selected difficulty and starts the AI battle
    private void StartAIBattle(AIController.Difficulty difficulty)
    {
        PlayerPrefs.SetInt("AIDifficulty", (int)difficulty);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Battle AI");
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
