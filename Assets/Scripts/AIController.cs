
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class AIController : MonoBehaviour
{
    // Available AI difficulty levels
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Header("References")]
    // Player 1 is the opponent the AI fights
    public Transform target;

    [Header("Difficulty")]
    public Difficulty difficulty = Difficulty.Easy;

    [Header("Countdown")]
    // Temporary delay for the initial match countdown
    public float startDelay = 4f;

    [Header("Combat Range")]
    public float preferredDistance = 3.5f;
    public float attackDistance = 4f;
    public float dodgeDistance = 4.5f;

    [Header("Jumping")]
    public float jumpForce = 7f;

    [Header("Debug")]
    public bool showDebugMessages = false;

    // Components belonging to the AI
    private Rigidbody2D rb;
    private LocalPlayerMovement movement;
    private PlayerAttack attack;
    private KnockbackReceiver knockback;
    private BlockState blockState;
    private SpriteRenderer spriteRenderer;
    private HitBox2D hitbox;
    private AudioSource audioSource;

    // Components belonging to Player 1
    private LocalPlayerMovement targetMovement;
    private PlayerAttack targetAttack;
    private KnockbackReceiver targetKnockback;

    // Original hitbox position
    private float hitboxOffsetX;

    // Difficulty-specific behaviour
    private float moveSpeed;
    private float reactionDelay;
    private float attackIntervalMin;
    private float attackIntervalMax;
    private float dodgeChance;
    private float retreatChance;
    private float approachChance;
    private float closeRetreatChance;
    private float actionDurationMin;
    private float actionDurationMax;
    private float dodgeCooldown;
    private float desiredDistance;
    private float jumpDodgeChance;

    // AI timers
    private float readyTime;
    private float nextDecisionTime;
    private float nextAttackTime;
    private float nextJumpTime;
    private float nextDodgeTime;
    private float actionEndTime;

    // Movement and attack states
    private float moveDirection;
    private bool previousTargetAttacking;
    private bool roundLocked = true;

    // Save the original attack key
    private Key originalAttackKey;
    private bool savedAttackKey;

    private void Awake()
    {
        // Get the AI's existing components
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<LocalPlayerMovement>();
        attack = GetComponent<PlayerAttack>();
        knockback = GetComponent<KnockbackReceiver>();
        blockState = GetComponent<BlockState>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Find Player 2's attack hitbox
        hitbox = GetComponentInChildren<HitBox2D>(true);

        if (hitbox != null)
        {
            hitboxOffsetX = Mathf.Abs(
                hitbox.transform.localPosition.x
            );
        }

        // Find Player 1's components
        if (target != null)
        {
            targetMovement =
                target.GetComponent<LocalPlayerMovement>();

            targetAttack =
                target.GetComponent<PlayerAttack>();

            targetKnockback =
                target.GetComponent<KnockbackReceiver>();
        }
    }

    private void Start()
    {
        // Keep movement component enabled for animation
        // and other combat systems, but disable keyboard input
        if (movement != null)
        {
            movement.enabled = true;
            movement.SetInputEnabled(false);
        }

        // Keep the existing PlayerAttack component active
        if (attack != null)
        {
            attack.enabled = true;

            // Prevent normal Player 2 keyboard attacks
            originalAttackKey = attack.GetAttackKey();
            savedAttackKey = true;
            attack.SetAttackKey(Key.F12);
        }

        // Apply the selected difficulty
        ApplyDifficulty();

        // Wait for the initial countdown
        readyTime = Time.time + startDelay;
        nextDecisionTime = readyTime;

        ScheduleNextAttack(readyTime);

        // Do not allow AI attacks before the fight starts
        SetRoundLocked(true);
    }

    // Check whether Player 1 is locked by the countdown
    private bool IsRoundWaiting()
    {
        // Keep the initial countdown delay
        if (Time.time < readyTime)
            return true;

        // MatchManager and RoundCountdown use
        // Player 1's input state to lock controls
        if (targetMovement != null &&
            !targetMovement.InputEnabled)
        {
            // Do not mistake ordinary hitstun for a countdown
            if (targetKnockback == null ||
                !targetKnockback.InHitstun)
            {
                return true;
            }
        }

        return false;
    }

    // Called only when the round lock changes
    private void SetRoundLocked(bool locked)
    {
        if (roundLocked == locked)
            return;

        roundLocked = locked;
        moveDirection = 0f;

        if (locked)
        {
            // Cancel active attacks during round transitions
            if (attack != null)
                attack.SetInputEnabled(false);
        }
        else
        {
            // Re-enable AI attacks when the round begins
            if (attack != null)
                attack.SetInputEnabled(true);

            // Reset AI decisions for the new round
            actionEndTime = Time.time;
            nextDecisionTime = Time.time;
            nextJumpTime = Time.time + 0.5f;
            nextDodgeTime = Time.time + 0.5f;

            // Give the player time before the next attack
            ScheduleNextAttack(Time.time);

            previousTargetAttacking = false;
        }
    }

    // Configure Easy, Medium and Hard behaviour
    private void ApplyDifficulty()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                // Defensive and beginner-friendly
                moveSpeed = 2f;
                reactionDelay = 0.45f;
                attackIntervalMin = 3.5f;
                attackIntervalMax = 4.5f;
                dodgeChance = 0.50f;
                retreatChance = 0.75f;
                approachChance = 0.65f;
                closeRetreatChance = 0.85f;
                desiredDistance = preferredDistance;
                actionDurationMin = 0.6f;
                actionDurationMax = 1.2f;
                dodgeCooldown = 2f;
                jumpDodgeChance = 0.30f;
                break;

            case Difficulty.Medium:
                // Balanced and more aggressive
                moveSpeed = 3f;
                reactionDelay = 0.25f;
                attackIntervalMin = 1.8f;
                attackIntervalMax = 2.5f;
                dodgeChance = 0.65f;
                retreatChance = 0.30f;
                approachChance = 0.90f;
                closeRetreatChance = 0.45f;
                desiredDistance = 2.1f;
                actionDurationMin = 0.35f;
                actionDurationMax = 0.75f;
                dodgeCooldown = 1.5f;
                jumpDodgeChance = 0.45f;
                break;

            case Difficulty.Hard:
                // Fast, aggressive and reactive
                moveSpeed = 4.2f;
                reactionDelay = 0.10f;
                attackIntervalMin = 0.9f;
                attackIntervalMax = 1.4f;
                dodgeChance = 0.90f;
                retreatChance = 0.12f;
                approachChance = 0.98f;
                closeRetreatChance = 0.30f;
                desiredDistance = 1.5f;
                actionDurationMin = 0.2f;
                actionDurationMax = 0.45f;
                dodgeCooldown = 0.9f;
                jumpDodgeChance = 0.65f;
                break;
        }
    }

    // Randomly schedule the next attack
    private void ScheduleNextAttack(float fromTime)
    {
        nextAttackTime = fromTime +
            Random.Range(attackIntervalMin, attackIntervalMax);
    }

    private void Update()
    {
        if (target == null)
            return;

        // Keep keyboard movement disabled for Player 2
        // even after hitstun re-enables player controls
        if (movement != null && movement.InputEnabled)
            movement.SetInputEnabled(false);

        // Respect the initial and between-round countdowns
        if (IsRoundWaiting())
        {
            SetRoundLocked(true);
            return;
        }

        // Unlock the AI when the new round begins
        SetRoundLocked(false);

        // Respect knockback
        if (knockback != null && knockback.InHitstun)
        {
            moveDirection = 0f;
            return;
        }

        // Respect blocking
        if (blockState != null && blockState.IsBlocking)
        {
            moveDirection = 0f;
            return;
        }

        // Calculate distance to Player 1
        float difference =
            target.position.x - transform.position.x;

        float distance = Mathf.Abs(difference);

        float directionToPlayer =
            difference >= 0f ? 1f : -1f;

        // Detect Player 1 beginning an attack
        bool playerIsAttacking =
            targetAttack != null && targetAttack.IsAttacking;

        bool newIncomingAttack =
            playerIsAttacking && !previousTargetAttacking;

        previousTargetAttacking = playerIsAttacking;

        // Attempt to dodge nearby attacks
        if (newIncomingAttack &&
            distance <= dodgeDistance &&
            Time.time >= nextDodgeTime &&
            Random.value < dodgeChance)
        {
            Dodge(directionToPlayer);
            return;
        }

        // Attack based on difficulty-specific timing
        if (distance <= attackDistance &&
            Time.time >= nextAttackTime &&
            attack != null &&
            !attack.IsAttacking)
        {
            attack.TryAttack();
            ScheduleNextAttack(Time.time);

            moveDirection = 0f;

            actionEndTime = Time.time +
                (difficulty == Difficulty.Easy ? 0.4f :
                 difficulty == Difficulty.Medium ? 0.25f :
                 0.15f);

            if (showDebugMessages)
                Debug.Log("AI attacking: " + difficulty);

            return;
        }

        // Continue the chosen movement briefly
        if (Time.time < actionEndTime)
            return;

        // Respect reaction time
        if (Time.time < nextDecisionTime)
            return;

        nextDecisionTime = Time.time + reactionDelay;

        // Choose between approaching, retreating and waiting
        ChooseMovement(distance, directionToPlayer);

        // Jump if Player 1 is on a higher platform
        if (target.position.y >
                transform.position.y + 1.5f &&
            movement != null &&
            movement.IsGrounded &&
            Time.time >= nextJumpTime)
        {
            Jump();
            nextJumpTime = Time.time + 2.5f;
        }
    }

    // Decide how the AI moves
    private void ChooseMovement(
        float distance,
        float directionToPlayer)
    {
        float decision = Random.value;

        // When far away, approach Player 1
        if (distance > attackDistance + 1f)
        {
            moveDirection =
                decision < approachChance
                ? directionToPlayer
                : 0f;
        }
        // When too close, retreat or wait
        else if (distance < desiredDistance)
        {
            moveDirection =
                decision < closeRetreatChance
                ? -directionToPlayer
                : 0f;
        }
        else
        {
            // Difficulty determines how often AI retreats
            if (decision < retreatChance)
            {
                moveDirection = -directionToPlayer;
            }
            else
            {
                float approachProbability =
                    difficulty == Difficulty.Easy ? 0.4f :
                    difficulty == Difficulty.Medium ? 0.8f :
                    0.95f;

                moveDirection =
                    Random.value < approachProbability
                    ? directionToPlayer
                    : 0f;
            }
        }

        // Keep the decision for a short duration
        actionEndTime = Time.time +
            Random.Range(actionDurationMin, actionDurationMax);
    }

    // Dodge by retreating and occasionally jumping
    private void Dodge(float directionToPlayer)
    {
        moveDirection = -directionToPlayer;

        if (movement != null &&
            movement.IsGrounded &&
            Time.time >= nextJumpTime &&
            Random.value < jumpDodgeChance)
        {
            Jump();
            nextJumpTime = Time.time + 2f;
        }

        actionEndTime = Time.time +
            (difficulty == Difficulty.Hard ? 0.35f :
             difficulty == Difficulty.Medium ? 0.5f :
             0.7f);

        nextDodgeTime = Time.time + dodgeCooldown;

        if (showDebugMessages)
            Debug.Log("AI dodging!");
    }

    // Jump using existing player physics and sounds
    private void Jump()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        if (movement != null &&
            movement.jumpSound != null &&
            audioSource != null)
        {
            audioSource.PlayOneShot(movement.jumpSound);
        }

        if (showDebugMessages)
            Debug.Log("AI jumping!");
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;

        // Do not override knockback physics
        if (knockback != null && knockback.InHitstun)
            return;

        // Keep the AI still while the round is locked
        if (roundLocked ||
            IsRoundWaiting() ||
            (blockState != null && blockState.IsBlocking))
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
            return;
        }

        // Apply the selected horizontal movement
        rb.linearVelocity = new Vector2(
            moveDirection * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Face towards Player 1
        bool faceRight =
            target.position.x >= transform.position.x;

        // Keep the shared facing value in step. KnockbackReceiver and
        // BlockState both read movement.FacingDirection, and with input
        // disabled nothing else ever updates it — so knockback fired
        // the wrong way.
        if (movement != null)
            movement.SetFacingDirection(faceRight ? 1f : -1f);

        if (spriteRenderer != null && movement != null)
        {
            spriteRenderer.flipX =
                movement.spriteFacesRightByDefault
                ? !faceRight
                : faceRight;
        }

        // Keep the attack hitbox on the facing side
        if (hitbox != null)
        {
            Vector3 position = hitbox.transform.localPosition;

            position.x = hitboxOffsetX *
                (faceRight ? 1f : -1f);

            hitbox.transform.localPosition = position;
        }

        // Update the AI's movement animation
        if (movement != null && movement.anim != null)
        {
            bool canMove =
                !roundLocked &&
                (knockback == null || !knockback.InHitstun);

            movement.anim.SetBool(
                "isMoving",
                canMove && Mathf.Abs(moveDirection) > 0.01f
            );
        }
    }

    private void OnDisable()
    {
        // Restore Player 2's original attack key
        if (attack != null && savedAttackKey)
        {
            attack.SetAttackKey(originalAttackKey);
        }
    }
}
