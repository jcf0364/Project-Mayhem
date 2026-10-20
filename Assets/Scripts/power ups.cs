using UnityEngine;

public class powerups : MonoBehaviour
{
    public GameObject powerup;
    private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 10)
        {
            timer = 0;
            GameObject pu = Instantiate(powerup, transform.position, Quaternion.identity);
        }
    }
}
