using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// ADDED (combos): one hit in a combo
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

// ADDED (combos): the full combo for one character
[System.Serializable]
public class CharacterCombo
{
    [Tooltip("Must match the character name exactly, e.g. Mayhem or Angelo.")]
    public string characterName = "Mayhem";

    [Tooltip("Extra time after a hit ends where a press still chains.")]
    public float chainWindow = 0.25f;

    [Tooltip("Exact name of THIS character's attack state in the Animator.")]
    public string attackStateName = "Attack";

    public ComboStep[] steps;
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
    [Header("Timing (seconds)")]
    [Tooltip("Telegraph before the hitbox turns on.")]
    [SerializeField] private float windupTime = 0.08f;
    [Tooltip("The ONLY window where a hit can land.")]
    [SerializeField] private float activeTime = 0.12f;
    [Tooltip("Committed. Can't act again yet.")]

    private float recoveryTime;
    private float cooldown;
    public GameObject player;

    // ADDED (combos)
    [Header("Character Combos")]
    [Tooltip("One entry per character. The match picks the one for the chosen character.")]
    [SerializeField] private CharacterCombo[] characterCombos;

    [Header("Fallback Combo (no character matched)")]
    [SerializeField] private ComboStep[] comboSteps = { new ComboStep() };
    [SerializeField] private float chainWindow = 0.25f;

    [Tooltip("Exact name of the attack state in the Animator.")]
    [SerializeField] private string attackStateName = "Attack";


    [Header("State")]
    [SerializeField] private bool inputEnabled = true;

    private bool isAttacking;
    private float lastAttackTime = -99f;   // now: when the last combo ended

    public bool IsAttacking => isAttacking;

    // ADDED (combos): combo tracking
    private int currentStep = -1;      // -1 = no combo running
    private bool buffered;             // pressed during a hit
    private Coroutine windowRoutine;   // the chain window timer
    public int CurrentStep => currentStep;

    // ADDED (combos): cached components
    private LocalPlayerMovement move;
    private BlockState blockState;

    private float baseHitboxX;

    private void Awake()
    {
        PlayerPrefs.SetString("ID", " ");
        if (hitbox != null)
            baseHitboxX = Mathf.Abs(hitbox.transform.localPosition.x);
        spritestats();
        

        if (hitbox != null)
            baseHitboxX = Mathf.Abs(hitbox.transform.localPosition.x);
        spritestats();

        // ADDED (combos)
        move = GetComponent<LocalPlayerMovement>();
        blockState = GetComponent<BlockState>();
    }

    // ADDED (combos): pick the combo for the chosen character.
    // Start, not Awake, so HitBox2D has finished its own setup.
    private void Start()
    {
        if (hitbox == null || characterCombos == null)
            return;

        string character = hitbox.GetCharacterName();

        foreach (var combo in characterCombos)
        {
            if (combo.characterName == character &&
                combo.steps != null && combo.steps.Length > 0)
            {
                comboSteps = combo.steps;
                chainWindow = combo.chainWindow;

                // ADDED: each character plays its own attack animation
                if (!string.IsNullOrEmpty(combo.attackStateName))
                    attackStateName = combo.attackStateName;

                return;
            }
        }

        Debug.LogWarning($"{name}: no combo for '{character}', using the fallback combo.");
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
            EndCombo();   // CHANGED: was isAttacking = false; (EndCombo does that too)
        }
    }

    // CHANGED (combos): handles chaining and buffering
    public void TryAttack()
    {
        // No attacking while blocking
        if (blockState != null && blockState.IsBlocking) return;

        // Mid-hit: queue the next hit
        if (isAttacking)
        {
            buffered = true;
            return;
        }

        // In the chain window: next hit straight away
        if (currentStep >= 0)
        {
            if (windowRoutine != null) StopCoroutine(windowRoutine);
            windowRoutine = null;

            StartStep(currentStep + 1);
            return;
        }

        // Fresh combo: wait for the cooldown (set per character in spritestats)
        if (Time.time - lastAttackTime < cooldown) return;

        StartStep(0);
    }

    private void FaceHitbox()
    {
        var move = GetComponent<LocalPlayerMovement>();
        if (move == null || hitbox == null) return;

        var pos = hitbox.transform.localPosition;
        pos.x = baseHitboxX * Mathf.Sign(move.FacingDirection);
        hitbox.transform.localPosition = pos;
    }

    // ADDED (combos)
    private void StartStep(int index)
    {
        currentStep = index;
        buffered = false;
        StartCoroutine(AttackRoutine(comboSteps[index]));
    }

    // CHANGED (combos): runs one combo hit at that hit's power and speed
    private IEnumerator AttackRoutine(ComboStep step)
    {
        isAttacking = true;
        float speed = Mathf.Max(step.speed, 0.1f);

        FaceHitbox();
        hitbox.SetStats(step.damage, step.knockback, step.hitstun);

        // Attack swing sound.
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

        Debug.Log(recoveryTime);
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

    // ADDED (combos): waits for the next press. Too slow = combo drops.
    private IEnumerator ChainWindowRoutine()
    {
        yield return new WaitForSeconds(chainWindow);
        windowRoutine = null;
        EndCombo();
    }

    // ADDED (combos)
    private void EndCombo()
    {
        if (currentStep >= 0)
            lastAttackTime = Time.time;

        currentStep = -1;
        buffered = false;
        isAttacking = false;
        windowRoutine = null;

        if (move != null && move.anim != null)
            move.anim.speed = 1f;
    }

    //based on sprite
    private void spritestats()
    {
        if (player == null) player = transform.root.gameObject; // safety net
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
    public void AnimActivateHitbox() => hitbox.Activate();
    public void AnimDeactivateHitbox() => hitbox.Deactivate();
}