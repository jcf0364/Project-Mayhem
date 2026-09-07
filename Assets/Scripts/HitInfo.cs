using UnityEngine;

public struct HitInfo
{
    public int Damage;
    public GameObject Source;
    public Vector2 SourcePosition;
    public Vector2 ContactPoint;

    public HitInfo(int damage, GameObject source, Vector2 contactPoint)
    {
        Damage = damage;
        Source = source;
        SourcePosition = source != null
            ? (Vector2)source.transform.position
            : Vector2.zero;
        ContactPoint = contactPoint;
    }
}
