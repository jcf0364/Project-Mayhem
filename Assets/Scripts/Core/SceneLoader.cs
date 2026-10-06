using UnityEngine;
using UnityEngine.SceneManagement;

/// One place that knows every scene name.
public class SceneLoader : MonoBehaviour
{
    public const string MainMenu = "MainMenu";
    public const string CharacterSelect = "CharacterSelect";
    public const string ArenaSelect = "ArenaSelect";
    public const string Battle = "Battle";

    public void LoadMainMenu() => Load(MainMenu);
    public void LoadCharacterSelect() => Load(CharacterSelect);
    public void LoadArenaSelect() => Load(ArenaSelect);
    public void LoadBattle() => Load(Battle);

    public static void Load(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}