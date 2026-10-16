using UnityEngine;
using TMPro;

public class characterselected : MonoBehaviour
{
    public RuntimeAnimatorController animmayhem;
    public RuntimeAnimatorController animangelo;
    private Animator animplayer;
    public GameObject player;
    public TMP_Text text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       animplayer=player.GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(text.text);
        if (text.text == "Mayhem")
        {
            animplayer.runtimeAnimatorController = animmayhem as RuntimeAnimatorController;
            Debug.Log("sprite is mayhem");
        }
        else{
            if (text.text == "Angelo")
            {
                animplayer.runtimeAnimatorController = animangelo as RuntimeAnimatorController;
                Debug.Log("sprite is angelo");
            }
        }
    }
}
