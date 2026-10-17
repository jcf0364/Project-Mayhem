using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ComboStep
{
    [Tooltip("Damage this hit deals.")]
    public int damage = 15;

    [Tooltip("Push force. Keep early hits small so the next hit still connects.")]
    public float knockback = 8f;

    [Tooltip("How long the target is stunned. Must last until the next hit lands.")]
    public float hitstun = 0.25f;

    [Tooltip("1 = normal. Above 1 = faster hit, below 1 = slower hit.")]
    public float speed = 1f;
}

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

    [Header("Timing (seconds, at speed 1)")]
    [Tooltip("Telegraph before the hitbox turns on.")]
    [SerializeField] private float windupTime = 0.08f;

    [Tooltip("The ONLY window where a hit can land.")]
    [SerializeField] private float activeTime = 0.12f;

    [Tooltip("Committed. Can't act again yet.")]
    [SerializeField] private float recoveryTime = 0.20f;

    [Tooltip("Wait after a combo ends before a new one can start.")]
    [SerializeField] private float cooldown = 0.35f;

    [Header("Combo")]
    [SerializeField] private ComboStep[] comboSteps = { new ComboStep() };

    [Tooltip("Extra time after a hit ends where a press still chains.")]
    [SerializeField] private float chainWindow = 0.25f;

    [Tooltip("Exact name of the attack state in the Animator.")]
    [SerializeField] private string attackStateName = "Attack";

    [Header("State")]
    [SerializeField] private bool inputEnabled = true;

    // Tracks whether a hit is playing right now
    private bool isAttacking;

    // Combo tracking
    private int currentStep = -1;      // -1 = no combo running
    private bool buffered;             // pressed during a hit
    private Coroutine windowRoutine;   // the chain window timer
    private float lastComboEndTime = -99f;

    // Allows other scripts to check attack state
    public bool IsAttacking => isAttacking;
    public int CurrentStep => currentStep;

    // Stores the hitbox's original horizontal position
    private float baseHitboxX;

    private BlockState blockState;
    private LocalPlayerMovement move;

    private void Awake()
    {
        // Save the original hitbox position
        if (hitbox != null)
            baseHitboxX = Mathf.Abs(hitbox.transform.localPosition.x);

        // Cached rather than fetched per press
        blockState = GetComponent<BlockState>();
        move = GetComponent<LocalPlayerMovement>();
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        var kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb[attackKey].wasPressedThisFrame)
            TryAttack();
    }

    // Enables or disables attack input
    // (hitstun and the round countdown both call this)
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        // Stop any attack and drop the combo when input is disabled
        if (!enabled)
        {
            StopAllCoroutines();

            if (hitbox != null)
                hitbox.Deactivate();

            EndCombo();
        }
    }

    // Called on every attack press
    public void TryAttack()
    {
        // A blocked press is rejected outright
        if (blockState != null && blockState.IsBlocking)
            return;

        // Mid-hit: queue the next hit
        if (isAttacking)
        {
            buffered = true;
            return;
        }

        // In the chain window: next hit straight away (AC 1)
        if (currentStep >= 0)
        {
            if (windowRoutine != null)
                StopCoroutine(windowRoutine);
            windowRoutine = null;

            StartStep(currentStep + 1);
            return;
        }

        // Fresh combo: wait for the cooldown
        if (Time.time - lastComboEndTime < cooldown)
            return;

        StartStep(0);
    }

    private void StartStep(int index)
    {
        currentStep = index;
        buffered = false;
        StartCoroutine(AttackRoutine(comboSteps[index]));
    }

    // One hit: windup, active, recovery
    private IEnumerator AttackRoutine(ComboStep step)
    {
        isAttacking = true;
        float speed = Mathf.Max(step.speed, 0.1f);

        // Face the hitbox and give it this hit's power
        FaceHitbox();
        hitbox.SetStats(step.damage, step.knockback, step.hitstun);

        if (move != null)
        {
            move.PlayAttackSound();

            // Restart the attack animation at this hit's speed
            if (move.anim != null)
            {
                move.anim.speed = speed;
                move.anim.SetBool("isAttacking", true); // avoids a 1-frame flicker
                move.anim.Play(attackStateName, 0, 0f);
            }
        }

        yield return new WaitForSeconds(windupTime / speed);

        hitbox.Activate();
        yield return new WaitForSeconds(activeTime / speed);
        hitbox.Deactivate();

        yield return new WaitForSeconds(recoveryTime / speed);

        isAttacking = false;
        if (move != null && move.anim != null)
            move.anim.speed = 1f;

        bool hasNext = currentStep + 1 < comboSteps.Length;

        if (!hasNext)
            EndCombo();                       // finisher done
        else if (buffered)
            StartStep(currentStep + 1);       // pressed during the hit
        else
            windowRoutine = StartCoroutine(ChainWindowRoutine());
    }

    // Waits for the next press. Too slow = combo drops.
    private IEnumerator ChainWindowRoutine()
    {
        yield return new WaitForSeconds(chainWindow);
        windowRoutine = null;
        EndCombo();
    }

    private void EndCombo()
    {
        if (currentStep >= 0)
            lastComboEndTime = Time.time;

        currentStep = -1;
        buffered = false;
        isAttacking = false;
        windowRoutine = null;

        if (move != null && move.anim != null)
            move.anim.speed = 1f;
    }

    // Moves the hitbox to face the correct direction
    private void FaceHitbox()
    {
        if (move == null || hitbox == null)
            return;

        var pos = hitbox.transform.localPosition;
        pos.x = baseHitboxX * Mathf.Sign(move.FacingDirection);
        hitbox.transform.localPosition = pos;
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