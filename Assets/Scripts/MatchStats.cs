using UnityEngine;

/// Tracks per-match statistics. One instance, sitting on MatchManager.
public class MatchStats : MonoBehaviour
{
    [Header("Fighters")]
    [SerializeField] private HurtBox2D hurtboxP1;
    [SerializeField] private HurtBox2D hurtboxP2;

    // Damage a player DEALT is damage their opponent TOOK.
    public int DamageDealtP1 { get; private set; }
    public int DamageDealtP2 { get; private set; }

    public int HitsLandedP1 { get; private set; }
    public int HitsLandedP2 { get; private set; }

    public int RoundsWonP1 { get; private set; }
    public int RoundsWonP2 { get; private set; }

    public float MatchDuration => matchRunning
        ? Time.time - matchStartTime
        : finalDuration;

    private float matchStartTime;
    private float finalDuration;
    private bool matchRunning;

    private void OnEnable()
    {
        if (hurtboxP1 != null) hurtboxP1.OnHitReceived += OnP1Hit;
        if (hurtboxP2 != null) hurtboxP2.OnHitReceived += OnP2Hit;
    }

    private void OnDisable()
    {
        if (hurtboxP1 != null) hurtboxP1.OnHitReceived -= OnP1Hit;
        if (hurtboxP2 != null) hurtboxP2.OnHitReceived -= OnP2Hit;
    }

    // P1 was hit, so P2 dealt the damage.
    private void OnP1Hit(HitInfo info)
    {
        DamageDealtP2 += info.Damage;
        HitsLandedP2++;
    }

    private void OnP2Hit(HitInfo info)
    {
        DamageDealtP1 += info.Damage;
        HitsLandedP1++;
    }

    public void ResetStats()
    {
        DamageDealtP1 = 0;
        DamageDealtP2 = 0;
        HitsLandedP1 = 0;
        HitsLandedP2 = 0;
        RoundsWonP1 = 0;
        RoundsWonP2 = 0;

        matchStartTime = Time.time;
        matchRunning = true;
    }

    public void RecordRoundWin(int winnerId)
    {
        if (winnerId == 1) RoundsWonP1++;
        else               RoundsWonP2++;
    }

    public void StopTimer()
    {
        finalDuration = Time.time - matchStartTime;
        matchRunning = false;
    }

    public string FormatDuration()
    {
        int total = Mathf.FloorToInt(MatchDuration);
        return $"{total / 60:0}:{total % 60:00}";
    }
}