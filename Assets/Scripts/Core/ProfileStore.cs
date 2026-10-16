using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// Reads and writes the profile list to disk. Static because there is only
/// ever one profile file, and every scene needs to reach it without holding
/// a reference to an object that might not exist yet.
public static class ProfileStore
{
    private const string FileName = "profiles.json";

    // persistentDataPath, not dataPath. dataPath is inside the build folder
    // and is read-only once the game ships.
    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    private static ProfileList cache;

    public static List<PlayerProfile> GetAll()
    {
        if (cache == null) Load();
        return cache.profiles;
    }

    public static PlayerProfile GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return GetAll().FirstOrDefault(p => p.id == id);
    }

    /// Returns the profile's name, or a fallback if the id no longer exists
    /// — for example if the file was edited or a profile was deleted.
    public static string GetDisplayName(string id, string fallback)
    {
        var profile = GetById(id);
        return profile != null ? profile.displayName : fallback;
    }

    public static PlayerProfile Create(string displayName)
    {
        var profile = new PlayerProfile(displayName);
        GetAll().Add(profile);
        Save();
        return profile;
    }

    public static bool NameTaken(string displayName)
    {
        // Case-insensitive so "Sam" and "sam" can't both exist and confuse
        // whoever reads the leaderboard.
        return GetAll().Any(p =>
            string.Equals(p.displayName, displayName, System.StringComparison.OrdinalIgnoreCase));
    }

    public static void Load()
    {
        if (!File.Exists(FilePath))
        {
            cache = new ProfileList();
            return;
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            cache = JsonUtility.FromJson<ProfileList>(json) ?? new ProfileList();
        }
        catch (IOException e)
        {
            // A corrupt or unreadable file should not stop the game booting.
            Debug.LogWarning($"Could not read profiles: {e.Message}");
            cache = new ProfileList();
        }
    }

    public static void Save()
    {
        try
        {
            // true = pretty print, so the file is readable if you need to
            // inspect or hand-edit it while testing.
            string json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(FilePath, json);
        }
        catch (IOException e)
        {
            Debug.LogWarning($"Could not save profiles: {e.Message}");
        }
    }
}