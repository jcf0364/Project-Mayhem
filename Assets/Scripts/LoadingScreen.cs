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

        float timer = 0f;
        while (load.progress < 0.9f || timer < minimumSeconds)
        {
            timer += Time.unscaledDeltaTime;

            float realProgress = Mathf.Clamp01(load.progress / 0.9f); // Unity reports 0 to 0.9 while loading
            float timeProgress = Mathf.Clamp01(timer / minimumSeconds);
            float shown = Mathf.Min(realProgress, timeProgress);

            progressBar.value = shown;
            loadingText.text = "Loading... " + Mathf.RoundToInt(shown * 100f) + "%";
            yield return null;
        }
        progressBar.value = 1f;
        loadingText.text = "Loading... 100%";
        load.allowSceneActivation = true;
    }   

}
