using UnityEngine;

[System.Serializable]
public class AttackData
{
    [Header("Identity")]
    public string attackName = "Light";

    [Header("Damage")]
    public int damage = 10;
    public float knockback = 6f;
    public float hitstun = 0.2f;

    [Header("Timing (seconds)")]
    public float windupTime = 0.08f;
    public float activeTime = 0.12f;
    public float recoveryTime = 0.20f;
    public float cooldown = 0.35f;

    [Header("Hitbox Shape")]
    public Vector2 hitboxSize = new Vector2(0.9f, 0.8f);
    public Vector2 hitboxOffset = new Vector2(0.7f, 0f);
}