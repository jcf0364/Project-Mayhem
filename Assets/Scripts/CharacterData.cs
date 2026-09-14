using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Project Mayhem/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName = "Unnamed";
    [TextArea(2, 4)] public string description;
    public Sprite portrait;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float gravityScale = 3f;

    [Header("Durability")]
    public int maxHealth = 100;
    [Tooltip("Higher = knocked further. 0.6 heavyweight, 1.4 lightweight.")]
    public float knockbackMultiplier = 1f;
    public float invulnerabilityDuration = 0.3f;

    [Header("Attacks")]
    public AttackData lightAttack;
    public AttackData heavyAttack;
    public AttackData specialAttack;
}