using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour {

public GameObject Container;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Container.SetActive(true); 
            Time.timeScale = 0;
        }
    }

    public void ResumeButton(){
        Container.SetActive(false);
        Time.timeScale = 1;
    }

    public void OptionsButton(){
        
    }

    public void MainMenuButton(){
        //when main menu has been developed.....
        
    }
}