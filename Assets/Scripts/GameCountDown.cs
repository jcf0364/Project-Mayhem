using UnityEngine;
using TMPro;
using System.Collections;

public class GameCountDown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    void Start()
    {
        StartCoroutine(StartCountdownRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
