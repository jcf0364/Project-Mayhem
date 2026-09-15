using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DiveSpecial : MonoBehaviour, ISpecialAttack
{
    [Header("Dive")]
    [SerializeField] private float diveSpeed = 16f;
    [SerializeField] private float diveAngle = 45f;
    [SerializeField] private float minAirTime = 0.05f;

    private Rigidbody2D rb;
    private LocalPlayerMovement move;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        move = GetComponent<LocalPlayerMovement>();
    }

    public IEnumerator Execute(AttackData data, HitBox2D hitbox)
    {
        yield return new WaitForSeconds(data.windupTime);

        // Launch diagonally down in the facing direction.
        float facing = move != null ? move.FacingDirection : 1f;
        float rad = diveAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad) * facing, -Mathf.Sin(rad));

        rb.linearVelocity = dir * diveSpeed;

        hitbox.Configure(data);
        hitbox.Activate();

        // Stay active until landing, or until the active window expires.
        float elapsed = 0f;
        yield return new WaitForSeconds(minAirTime);

        while (elapsed < data.activeTime && !IsGrounded())
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        hitbox.Deactivate();

        // No landing lag — that's the point of the move.
        yield return new WaitForSeconds(data.recoveryTime);
    }

    private bool IsGrounded()
    {
        return Mathf.Abs(rb.linearVelocity.y) < 0.05f;
    }
}