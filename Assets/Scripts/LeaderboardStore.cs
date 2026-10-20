using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// One saved match result on the leaderboard.
[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public string date;

    // Empty constructor so JsonUtility can load entries from the file.
    public LeaderboardEntry() { }

    public LeaderboardEntry(string playerName, int score)
    {
        this.playerName = playerName;
        this.score = score;
        date = System.DateTime.Now.ToString("dd MMM yyyy");
    }
}

/// Wrapper so JsonUtility can save the list (it can't save a bare list).
[System.Serializable]
public class LeaderboardList
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

/// Reads and writes the top 5 to disk. Same pattern as ProfileStore,
/// so results survive closing the game.
public static class LeaderboardStore
{
    public const int MaxEntries = 5;

    private const string FileName = "leaderboard.json";

    // persistentDataPath, not dataPath, so it still saves in a build.
    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    private static LeaderboardList cache;

    /// The saved entries, best first.
    public static List<LeaderboardEntry> GetAll()
    {
        if (cache == null) Load();
        return cache.entries;
    }

    /// Puts the entry in score order. Entries below it shift down one,
    /// and anything past 5th drops off.
    /// Returns its rank (1-5), or 0 if it didn't make the top 5.
    public static int Add(LeaderboardEntry entry)
    {
        var entries = GetAll();

        // Find the first entry with a LOWER score. On a tie, the older
        // result keeps its place and the new one goes below it.
        int index = entries.Count;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entry.score > entries[i].score)
            {
                index = i;
                break;
            }
        }

        // Not good enough for the top 5.
        if (index >= MaxEntries) return 0;

        entries.Insert(index, entry);

        while (entries.Count > MaxEntries)
            entries.RemoveAt(entries.Count - 1);

        Save();
        return index + 1;
    }

    public static void Clear()
    {
        GetAll().Clear();
        Save();
    }

    public static void Load()
    {
        if (!File.Exists(FilePath))
        {
            cache = new LeaderboardList();
            return;
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            cache = JsonUtility.FromJson<LeaderboardList>(json) ?? new LeaderboardList();
        }
        catch (IOException e)
        {
            // A broken file should not stop the game.
            Debug.LogWarning($"Could not read leaderboard: {e.Message}");
            cache = new LeaderboardList();
        }
    }

    public static void Save()
    {
        try
        {
            // true = pretty print, so the file is easy to read while testing.
            string json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(FilePath, json);
        }
        catch (IOException e)
        {
            Debug.LogWarning($"Could not save leaderboard: {e.Message}");
        }
    }
}