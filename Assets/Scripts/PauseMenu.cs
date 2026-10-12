
using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    // Reference to the pause menu container
    public GameObject Container;

    // Reference to the main menu script
    public MainMenu mainMenu;

    // Reference to the pause options panel
    public GameObject pauseOptionsPanel;

    // Tracks whether the game is paused
    private bool isPaused;

    void Update()
    {
        // Check if the keyboard is connected
        if (Keyboard.current == null)
            return;

        // Press ESC to pause or resume the game
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Only allow pausing while gameplay is active
            if (mainMenu != null &&
                mainMenu.mainMenuPanel.activeSelf)
                return;

            if (isPaused)
                ResumeButton();
            else
                PauseGame();
        }
    }

    // Pauses gameplay and opens the pause menu
    private void PauseGame()
    {
        Container.SetActive(true);

        // Stop gameplay time
        Time.timeScale = 0f;

        isPaused = true;
    }

    // Resumes gameplay and closes the pause menu
    public void ResumeButton()
    {
        Container.SetActive(false);

        // Close pause options if they are open
        if (pauseOptionsPanel != null)
            pauseOptionsPanel.SetActive(false);

        // Resume gameplay time
        Time.timeScale = 1f;

        isPaused = false;
    }

    // Existing Options button function
    public void OptionsButton()
    {
        // Pause Options visibility is handled by
        // the existing Unity Button On Click events
    }

    // Returns to the main menu from a paused match
    public void MainMenuButton()
    {
        // Close the pause menu
        Container.SetActive(false);

        // Close the pause options panel
        if (pauseOptionsPanel != null)
            pauseOptionsPanel.SetActive(false);

        // Reset the pause state
        isPaused = false;

        // Hide gameplay objects
        mainMenu.player1.SetActive(false);
        mainMenu.player2.SetActive(false);
        mainMenu.ground.SetActive(false);

        // Hide the main menu's Options panel
        mainMenu.optionsPanel.SetActive(false);

        // Show the main menu
        mainMenu.mainMenuPanel.SetActive(true);

        // Keep gameplay paused while on the main menu
        Time.timeScale = 0f;
    }
}
