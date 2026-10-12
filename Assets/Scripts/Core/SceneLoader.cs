using UnityEngine;
using UnityEngine.SceneManagement;

/// One place that knows every scene name.
public class SceneLoader : MonoBehaviour
{
    public const string MainMenu = "MainMenu";
    public const string CharacterSelect = "CharacterSelect";
    public const string ArenaSelect = "ArenaSelect";
    public const string Battle = "Battle";
    public const string Battle1 = "Battle Arena 1";
    public const string Battle2 = "Battle Arena 2";
    public const string Battle3 = "Battle Arena 3";


    public void LoadMainMenu() => Load(MainMenu);
    public void LoadCharacterSelect() => Load(CharacterSelect);
    public void LoadArenaSelect() => Load(ArenaSelect);
    public void LoadBattle() => Load(Battle);
    public void LoadBattle1() => Load(Battle1);
    public void LoadBattle2() => Load(Battle2);
    public void LoadBattle3() => Load(Battle3);
    

    public static void Load(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}