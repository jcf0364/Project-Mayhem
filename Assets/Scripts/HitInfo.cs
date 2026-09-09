using UnityEngine;

public struct HitInfo
{
    public int Damage;
    public GameObject Source;
    public Vector2 SourcePosition;
    public Vector2 ContactPoint;
    public float KnockbackForce;
    public float HitstunDuration;

    public HitInfo(int damage, GameObject source, Vector2 contactPoint, float knockbackForce = 0f, float hitstunDuration = 0f)
    {
        Damage = damage;
        Source = source;
        SourcePosition = source != null
            ? (Vector2)source.transform.position
            : Vector2.zero;
        ContactPoint = contactPoint;
        KnockbackForce = knockbackForce;
        HitstunDuration = hitstunDuration;
    }
}
