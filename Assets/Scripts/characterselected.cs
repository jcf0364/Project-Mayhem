using UnityEngine;
using TMPro;

public class characterselected : MonoBehaviour
{
    public TMP_Text text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(text.text);
        if (text.text == "Mayhem")
        {
            PlayerPrefs.SetString("Character", "Mayhem");
        }
        else{
            if (text.text == "Angelo")
            {
                PlayerPrefs.SetString("Character", "Angelo");
            }
        }
        PlayerPrefs.Save();
    }
}
