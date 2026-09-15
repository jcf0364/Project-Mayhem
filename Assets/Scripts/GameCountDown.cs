using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameCountDown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    private List<LocalPlayerMovement> players = new List<LocalPlayerMovement>();
 
     public static bool InputEnabled { get; private set; } = false;
 
    void Start()
    {
        players.AddRange(FindObjectsByType<LocalPlayerMovement>());
        StartCoroutine(StartCountdownRoutine());
    }
 
    private IEnumerator StartCountdownRoutine()
    {
        foreach (LocalPlayerMovement player in players)
        {
            if (player != null)
            {
                player.enabled = false;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
        
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
        foreach (LocalPlayerMovement player in players)
        {
            if (player != null)
            {
                player.enabled = true;
            }
        }
        InputEnabled = true;
    }
}
