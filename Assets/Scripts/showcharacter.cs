using UnityEngine;

public class showcharacter : MonoBehaviour
{
    public RuntimeAnimatorController animmayhem;
    public RuntimeAnimatorController animangelo;
    private Animator animplayer;
    public GameObject player;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player.name == "Player1")
        {
            string text = PlayerPrefs.GetString("Character");
            animplayer = player.GetComponent<Animator>();
            if (text == "Mayhem")
            {
                animplayer.runtimeAnimatorController = animmayhem as RuntimeAnimatorController;
                Debug.Log("sprite is mayhem");
            }
            else
            {
                if (text == "Angelo")
                {
                    animplayer.runtimeAnimatorController = animangelo as RuntimeAnimatorController;
                    Debug.Log("sprite is angelo");
                }
            }
        }
        else
        {
            string text = PlayerPrefs.GetString("Character");
            animplayer = player.GetComponent<Animator>();
            if (text == "Mayhem")
            {
                animplayer.runtimeAnimatorController = animangelo as RuntimeAnimatorController;
                Debug.Log("sprite is angelo");
                
            }
            else
            {
                if (text == "Angelo")
                {
                    animplayer.runtimeAnimatorController = animmayhem as RuntimeAnimatorController;
                    Debug.Log("sprite is mayhem");
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
