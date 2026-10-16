using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float minimumSeconds = 2f;

    IEnumerator Start()
    {
        // goes back to the main menu if this scene is opened on its own
        string target = string.IsNullOrEmpty(SceneLoader.NextScene) ? SceneLoader.MainMenu : SceneLoader.NextScene;

        AsyncOperation load = SceneManager.LoadSceneAsync(target);
        load.allowSceneActivation = false; // hold the switch until we ready

        
    }   

}
