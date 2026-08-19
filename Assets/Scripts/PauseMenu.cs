using UnityEngine;

public class Pause : MonoBehaviour {

public GameObject Container;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Container.SetActive(true); 
            Time.timeScale = 0;
        }
    }

    public void ResumeButton(){
        Time.timeScale = 1;
        Container.SetActive(false);
    }

    public void OptionsButton(){
        
    }

    public void MainMenuButton(){
        
    }
}