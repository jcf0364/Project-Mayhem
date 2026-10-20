using UnityEngine;

public class PickupPowerUp : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter2D(Collider2D col)
    {
        PlayerPrefs.SetString("ID", " ");
        ActivatePowerup(col);
        
    }

    void ActivatePowerup(Collider2D col)
    {
        Debug.Log("Picked up");
        PlayerPrefs.SetString("ID", col.gameObject.name);
        Debug.Log(col.gameObject.name);
        PlayerPrefs.SetFloat("multiplier", 1.5f);
        Destroy(gameObject);
    }
}
