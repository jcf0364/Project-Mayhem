using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private CharacterData character;

    [Header("References")]
    [SerializeField] private HitBox2D hitbox;

    [Header("Input")]
    [SerializeField] private Key lightKey = Key.J;
    [SerializeField] private Key heavyKey = Key.K;
    [SerializeField] private Key specialKey = Key.L;

    [Header("State")]
    [SerializeField] private bool inputEnabled = true;

    private bool isAttacking;
    private float lastLightTime = -99f;
    private float lastHeavyTime = -99f;
    private float lastSpecialTime = -99f;

    public bool IsAttacking => isAttacking;
    public CharacterData Character => character;

    private void Update()
    {
        if (!inputEnabled || character == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[lightKey].wasPressedThisFrame)
            TryAttack(character.lightAttack, ref lastLightTime);

        else if (kb[heavyKey].wasPressedThisFrame)
            TryAttack(character.heavyAttack, ref lastHeavyTime);

        else if (kb[specialKey].wasPressedThisFrame)
            TrySpecial();
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

    private void TryAttack(AttackData attack, ref float lastTime)
    {
        if (isAttacking) return;
        if (Time.time - lastTime < attack.cooldown) return;

        lastTime = Time.time;
        StartCoroutine(AttackRoutine(attack));
    }

    /// Specials are handled by a separate component per character.
    private void TrySpecial()
    {
        if (isAttacking) return;
        if (Time.time - lastSpecialTime < character.specialAttack.cooldown) return;

        var special = GetComponent<ISpecialAttack>();
        if (special == null)
        {
            // No special component — fall back to a normal attack.
            TryAttack(character.specialAttack, ref lastSpecialTime);
            return;
        }

        lastSpecialTime = Time.time;
        StartCoroutine(SpecialRoutine(special));
    }

    private IEnumerator AttackRoutine(AttackData attack)
    {
        isAttacking = true;

        yield return new WaitForSeconds(attack.windupTime);

        hitbox.Configure(attack);
        hitbox.Activate();
        yield return new WaitForSeconds(attack.activeTime);
        hitbox.Deactivate();

        yield return new WaitForSeconds(attack.recoveryTime);
        isAttacking = false;
    }

    private IEnumerator SpecialRoutine(ISpecialAttack special)
    {
        isAttacking = true;

        yield return special.Execute(character.specialAttack, hitbox);

        isAttacking = false;
    }
}