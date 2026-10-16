using System;
using System.Collections.Generic;

/// One saved player. Plain data, no MonoBehaviour — these live in a file,
/// not in a scene.
[Serializable]
public class PlayerProfile
{
    // Stable identifier, separate from the name on purpose. A player can
    // rename themselves without orphaning their past leaderboard results.
    public string id;

    public string displayName;
    public string createdUtc;

    public PlayerProfile(string name)
    {
        id = Guid.NewGuid().ToString();
        displayName = name;
        createdUtc = DateTime.UtcNow.ToString("o");
    }
}

/// Wrapper so the list can be written to JSON.
/// Unity's JsonUtility cannot serialise a bare List at the top level —
/// it needs an object with the list as a field.
[Serializable]
public class ProfileList
{
    public List<PlayerProfile> profiles = new List<PlayerProfile>();
}