
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HitBox2D hitbox;

    [Header("Input")]
    [SerializeField] private Key attackKey = Key.J;

    // Updates the attack key when the player remaps controls
    public void SetAttackKey(Key newKey)
    {
        attackKey = newKey;
    }

    // Returns the player's currently assigned attack key
    // Used when cancelling control remapping
    public Key GetAttackKey()
    {
        return attackKey;
    }

    [Header("Timing (seconds)")]
    [Tooltip("Telegraph before the hitbox turns on.")]
    [SerializeField] private float windupTime = 0.08f;

    [Tooltip("The ONLY window where a hit can land.")]
    [SerializeField] private float activeTime = 0.12f;

    [Tooltip("Committed. Can't act again yet.")]
    [SerializeField] private float recoveryTime = 0.20f;

    [SerializeField] private float cooldown = 0.35f;

    [Header("State")]
    [SerializeField] private bool inputEnabled = true;

    // Tracks whether the player is attacking
    private bool isAttacking;

    // Records the time of the previous attack
    private float lastAttackTime = -99f;

    // Allows other scripts to check attack state
    public bool IsAttacking => isAttacking;

    // Stores the hitbox's original horizontal position
    private float baseHitboxX;

    private BlockState blockState;

    private void Awake()
    {
        // Save the original hitbox position
        if (hitbox != null)
            baseHitboxX = Mathf.Abs(hitbox.transform.localPosition.x);
        
        // Cached rather than fetched per press — TryAttack can run every
        // frame the key is mashed.
        blockState = GetComponent<BlockState>();
    }

    private void Update()
    {
        // Do not process attacks when input is disabled
        if (!inputEnabled)
            return;

        // Get the current keyboard
        var kb = Keyboard.current;

        if (kb == null)
            return;

        // Attack when the assigned key is pressed
        if (kb[attackKey].wasPressedThisFrame)
            TryAttack();
    }

    // Enables or disables attack input
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        // Stop any active attack when input is disabled
        if (!enabled)
        {
            StopAllCoroutines();

            if (hitbox != null)
                hitbox.Deactivate();

            isAttacking = false;
        }
    }

    // Attempts to start an attack
    public void TryAttack()
    {
        // Prevent attacking while already attacking
        if (isAttacking)
            return;

        // Prevent attacking before cooldown finishes
        if (Time.time - lastAttackTime < cooldown)
            return;
        // Checked before the cooldown so a blocked press is rejected
        // outright rather than quietly consuming the attack timer.
        if (blockState != null && blockState.IsBlocking) return;

        
        if (isAttacking) return;
        if (Time.time - lastAttackTime < cooldown) return;

        StartCoroutine(AttackRoutine());
    }

    // Moves the hitbox to face the correct direction
    private void FaceHitbox()
    {
        var move = GetComponent<LocalPlayerMovement>();

        if (move == null || hitbox == null)
            return;

        var pos = hitbox.transform.localPosition;

        pos.x = baseHitboxX * Mathf.Sign(move.FacingDirection);

        hitbox.transform.localPosition = pos;
    }

    // Controls the different stages of an attack
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Face the hitbox in the player's direction
        FaceHitbox();

        // Play the attack sound
        var move = GetComponent<LocalPlayerMovement>();

        if (move != null)
            move.PlayAttackSound();

        // Wait before activating the hitbox
        yield return new WaitForSeconds(windupTime);

        // Activate the hitbox so damage can be dealt
        hitbox.Activate();

        yield return new WaitForSeconds(activeTime);

        // Disable the hitbox after the active period
        hitbox.Deactivate();

        // Wait until the recovery period finishes
        yield return new WaitForSeconds(recoveryTime);

        isAttacking = false;
    }

    // Called by animation events when animations control the hitbox
    public void AnimActivateHitbox()
    {
        hitbox.Activate();
    }

    public void AnimDeactivateHitbox()
    {
        hitbox.Deactivate();
    }
}
