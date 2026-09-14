using System.Collections;

/// Each character's special implements this.
/// PlayerAttack doesn't know or care which one it has.
public interface ISpecialAttack
{
    IEnumerator Execute(AttackData data, HitBox2D hitbox);
}