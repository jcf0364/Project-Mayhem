using UnityEngine;

public class PickupPowerUp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter2D(Collider2D col)
    {
        ActivatePowerup();
    }

    void ActivatePowerup()
    {
        Debug.Log("Picked up");

        Destroy(gameObject);
    }
}
