
using UnityEngine;

[RequireComponent(typeof(AIController))]
[DefaultExecutionOrder(-100)]
public class AISelectedDifficulty : MonoBehaviour
{
    private void Awake()
    {
        // Get the AI controller attached to Player 2
        AIController ai = GetComponent<AIController>();

        // Read the difficulty selected in the main menu
        int savedDifficulty = PlayerPrefs.GetInt(
            "AIDifficulty",
            (int)AIController.Difficulty.Easy
        );

        // Ensure the difficulty is within the valid range
        savedDifficulty = Mathf.Clamp(savedDifficulty, 0, 2);

        // Set the AI difficulty before the battle starts
        ai.difficulty = (AIController.Difficulty)savedDifficulty;

        // Display the selected difficulty in Unity's Console
        Debug.Log("AI difficulty selected: " + ai.difficulty);
    }
}
