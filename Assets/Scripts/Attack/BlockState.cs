using UnityEngine;
using UnityEngine.InputSystem;

/// Holds a fighter's guard state and the numbers that soften an incoming hit.
/// This script never applies damage itself — HurtBox2D asks it whether a hit
/// was blocked, then does the reduction. Keeping the decision and the effect
/// apart means blocking works for anything that routes through a hurtbox.
[RequireComponent(typeof(LocalPlayerMovement))]
public class BlockState : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key blockKey = Key.S;

    [Header("Damage Through")]
    // Fractions rather than flat subtractions, so blocking stays useful
    // against both a 10-damage jab and a 40-damage heavy.
    [Tooltip("Fraction of damage that still lands. 0.2 = 20% chip.")]
    [Range(0f, 1f)]
    [SerializeField] private float damageThrough = 0.2f;

    [Tooltip("Fraction of knockback that still applies.")]
    [Range(0f, 1f)]
    [SerializeField] private float knockbackThrough = 0.3f;

    [Header("Feedback")]
    [SerializeField] private AudioClip blockSound;
    [SerializeField] private Color blockTint = new Color(0.6f, 0.8f, 1f);

    private LocalPlayerMovement movement;
    private PlayerAttack attack;
    private Animator anim;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    // Captured once at startup. Taken in Awake so a tint left over from a
    // previous frame can never be mistaken for the fighter's real colour.
    private Color normalColour;

    public bool IsBlocking { get; private set; }
    public float DamageThrough => damageThrough;
    public float KnockbackThrough => knockbackThrough;

    private void Awake()
    {
        movement = GetComponent<LocalPlayerMovement>();
        attack = GetComponent<PlayerAttack>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            normalColour = spriteRenderer.color;
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // isPressed, not wasPressedThisFrame — blocking is a held state,
        // not a one-shot action like attacking.
        bool wantsToBlock = kb[blockKey].isPressed;

        // Locked out whenever the fighter has no control. Covers both ways
        // control is taken away: the inputEnabled flag (hitstun, round
        // countdown, win screen), and the whole movement component being
        // switched off, which is what the match-start countdown does.
        if (!movement.enabled || !movement.InputEnabled) wantsToBlock = false;

        // No air blocking. Jumping is then a real commitment: you give up
        // your defence for the length of the arc.
        if (!movement.IsGrounded) wantsToBlock = false;

        // Can't guard out of a swing you already committed to. Without this
        // a player could attack and block in the same moment, which removes
        // the risk from attacking.
        if (attack != null && attack.IsAttacking) wantsToBlock = false;

        SetBlocking(wantsToBlock);
    }

    private void SetBlocking(bool blocking)
    {
        // Early return so the animator and sprite are only touched on a
        // real change, not every frame the key is held.
        if (blocking == IsBlocking) return;

        IsBlocking = blocking;

        if (anim != null)
            anim.SetBool("isBlocking", blocking);

        // Temporary stand-in for a block animation. Delete these two lines
        // once there's a real clip and the animator bool drives it.
        if (spriteRenderer != null)
            spriteRenderer.color = blocking ? blockTint : normalColour;
    }

    /// Asked by HurtBox2D on every incoming hit.
    /// Returns true only when the fighter is guarding AND the attack came
    /// from the side they are facing. Directional blocking is what stops
    /// "hold block forever" being strictly better than moving.
    public bool IsBlockingAgainst(Vector2 attackerPosition)
    {
        if (!IsBlocking) return false;

        // Positive when the attacker is to our right, negative to our left.
        float toAttacker = attackerPosition.x - transform.position.x;

        // Fighters standing inside each other give a near-zero difference,
        // and Mathf.Sign(0) is 1, which would make the result depend on
        // which way we happen to face. Count it as blocked instead.
        if (Mathf.Abs(toAttacker) < 0.01f) return true;

        // FacingDirection is +1 facing right, -1 facing left. Matching signs
        // mean the attacker is in front of us.
        return Mathf.Sign(toAttacker) == Mathf.Sign(movement.FacingDirection);
    }

    /// Called by HurtBox2D after a successful block, so the sound only
    /// plays on hits that were actually guarded.
    public void PlayBlockSound()
    {
        if (blockSound != null && audioSource != null)
            audioSource.PlayOneShot(blockSound, 0.6f);
    }
}