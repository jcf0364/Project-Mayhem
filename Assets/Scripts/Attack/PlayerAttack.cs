using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HitBox2D hitbox;

    [Header("Input")]
    [SerializeField] private Key attackKey = Key.J;

    [Header("Timing (seconds)")]
    [Tooltip("Telegraph before the hitbox turns on.")]
    [SerializeField] private float windupTime = 0.08f;
    [Tooltip("The ONLY window where a hit can land.")]
    [SerializeField] private float activeTime = 0.12f;
    [Tooltip("Committed. Can't act again yet.")]
    
     private float recoveryTime;
     private float cooldown;
    public GameObject player;


    [Header("State")]
    [SerializeField] private bool inputEnabled = true;

    private bool isAttacking;
    private float lastAttackTime = -99f;

    public bool IsAttacking => isAttacking;

    private float baseHitboxX;

    private void Awake()
    {
        
        if (hitbox != null)
            baseHitboxX = Mathf.Abs(hitbox.transform.localPosition.x);
        spritestats();
    }

    private void Update()
    {
        if (!inputEnabled) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[attackKey].wasPressedThisFrame)
            TryAttack();
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if (!enabled)
        {
            StopAllCoroutines();
            if (hitbox != null) hitbox.Deactivate();
            isAttacking = false;
        }
    }

    public void TryAttack()
    {
        if (isAttacking) return;
        if (Time.time - lastAttackTime < cooldown) return;

        StartCoroutine(AttackRoutine());
    }

    private void FaceHitbox()
    {
        var move = GetComponent<LocalPlayerMovement>();
        if (move == null || hitbox == null) return;

        var pos = hitbox.transform.localPosition;
        pos.x = baseHitboxX * Mathf.Sign(move.FacingDirection);
        hitbox.transform.localPosition = pos;
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        FaceHitbox();

        // Attack swing sound.
        var move = GetComponent<LocalPlayerMovement>();
        if (move != null) move.PlayAttackSound();

        yield return new WaitForSeconds(windupTime);

        hitbox.Activate();
        yield return new WaitForSeconds(activeTime);
        hitbox.Deactivate();

        Debug.Log(recoveryTime);
        yield return new WaitForSeconds(recoveryTime);
        isAttacking = false;
    }

    //based on sprite
    private void spritestats()
    {
        if (player.name == "Player1")
        {
            string text = PlayerPrefs.GetString("Character");
            Debug.Log(text);

            if (text == "Mayhem")
            {
                recoveryTime = 0.20f;
                cooldown = 0.45f;
            }
            else
            {
                if (text == "Angelo")
                {
                    recoveryTime = 0.30f;
                    cooldown = 0.75f;
                }
            }
        }
        else
        {
            string text = PlayerPrefs.GetString("Character");
            if (text == "Mayhem")
            {
                recoveryTime = 0.30f;
                cooldown = 0.75f;
            }
            else
            {
                if (text == "Angelo")
                {
                    recoveryTime = 0.20f;
                    cooldown = 0.45f;
                }
            }
        }
    }

    // Called by Animation Events once animations drive the hitbox.
    public void AnimActivateHitbox()   => hitbox.Activate();
    public void AnimDeactivateHitbox() => hitbox.Deactivate();
}