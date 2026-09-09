using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    public GameObject player1;
    public GameObject player2;
    public GameObject ground;

    void Start()
    {
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        player1.SetActive(false);
        player2.SetActive(false);
        ground.SetActive(false);

        Time.timeScale = 0f;
    }

    public void PlayGame()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        player1.SetActive(true);
        player2.SetActive(true);
        ground.SetActive(true);

        Time.timeScale = 1f;
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}