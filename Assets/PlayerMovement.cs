using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //setting up the speed and jump force
    public float speed;
    public float jump;
    //searching for the corresponding rigidbody
    public Rigidbody2D rb;
    //declaring variables
    private float Move;
    private Vector2 screenBounds;
    private float PlayerHalfWidth;
    private float PlayerHalfHeight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //searches the resolution of the screen (more or less)
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        // searches the most extreme values of the sprite, strarting from the middle
        PlayerHalfWidth = GetComponent<SpriteRenderer>().bounds.extents.x;
        PlayerHalfHeight = GetComponent<SpriteRenderer>().bounds.extents.y;

    }

    // Update is called once per frame
    void Update()
    {
        //Movement!!! idk how the input keys work but both arrows and Q/D (on azerty keyboards at least) works 
        Move = Input.GetAxis("Horizontal");
        //sets up the speed
        rb.linearVelocity = new Vector2(speed * Move, rb.linearVelocity.y);
        //JUMPING!! by pressing space (idk how to change it sorry)
        if (Input.GetButtonDown("Jump"))
        {
            //sets up the jump speed
            rb.AddForce(new Vector2(rb.linearVelocity.x, jump));
        }
        //Borders of the screen, i spent so long fixing this, we keep it
        //sets up the left and right borders
        float clampedX = Mathf.Clamp(transform.position.x, -screenBounds.x+PlayerHalfWidth, screenBounds.x - PlayerHalfWidth);
        //pos logs in the player position
        Vector2 pos = transform.position;
        // puts value of the x between-screenBounds.x+PlayerHalfWidth and screenBounds.x - PlayerHalfWidth
        pos.x = clampedX;
        //sets up the top + bottom borders, bottom border is lower than the screen so as to be able to make the player fall off the screen
        float clampedY = Mathf.Clamp(transform.position.y, -screenBounds.y-1, screenBounds.y);
        // puts value of the y between -screenBounds.y-1 and screenBounds.y
        pos.y = clampedY;
        //condition to apply the borders, if it's not in it, no borders
        if (pos.y > -screenBounds.y)
        {
            //if player is on screen, there's borders
            transform.position = pos;
        }
    }
}
