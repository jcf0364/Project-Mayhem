using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    public GameObject Container;

    private bool isPaused;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeButton();
            else PauseGame();
        }
    }

    private void PauseGame()
    {
        Container.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeButton()
    {
        Container.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void OptionsButton()
    {
    }
}