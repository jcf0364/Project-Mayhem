using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // UI panels
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    // Game objects that are hidden while in the main menu
    public GameObject player1;
    public GameObject player2;
    public GameObject ground;

    void Start()
    {
        // Show the main menu when the game starts
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        // Hide gameplay objects while the menu is open
        player1.SetActive(false);
        player2.SetActive(false);
        ground.SetActive(false);

        // Pause the game while on the main menu
        Time.timeScale = 0f;
    }

    public void PlayGame()
    {
        // Hide all menu panels
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        // Enable the players and ground
        player1.SetActive(true);
        player2.SetActive(true);
        ground.SetActive(true);

        // Resume the game
        Time.timeScale = 1f;
    }

    public void OpenOptions()
    {
        // Hide the main menu and show the options menu
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        // Close options and return to the main menu
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        // Close the application
        Application.Quit();
    }
}