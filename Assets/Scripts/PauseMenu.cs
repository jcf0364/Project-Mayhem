using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    public GameObject Container;
    public GameObject mainMenuPanel;
    public GameObject player1;
    public GameObject player2;
    public GameObject ground;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Container.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ResumeButton()
    {
        Container.SetActive(false);
        Time.timeScale = 1;
    }

    public void OptionsButton()
    {
    }

    public void MainMenuButton()
    {
        Container.SetActive(false);

        player1.SetActive(false);
        player2.SetActive(false);
        ground.SetActive(false);

        mainMenuPanel.SetActive(true);

        Time.timeScale = 0;
    }
}