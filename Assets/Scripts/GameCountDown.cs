using UnityEngine;
using TMPro;
using System.Collections;

public class GameCountDown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
 
     public static bool InputEnabled { get; private set; } = false;
 
    void Start()
    {
        StartCoroutine(StartCountdownRoutine());
    }
 
    private IEnumerator StartCountdownRoutine()
    {
        InputEnabled = false;
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);
        countdownText.text = "GO!";
 
        StartGameplay();
 
        yield return new WaitForSeconds(1f);
 
        countdownText.gameObject.SetActive(false);
    }
 
    private void StartGameplay()
    {
        InputEnabled = true;
    }
}