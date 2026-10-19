using System.Text;
using TMPro;
using UnityEngine;

/// Shows the top 5. Sits on the leaderboard panel and refreshes
/// every time the panel opens, so it always shows the latest results.
public class LeaderboardUI : MonoBehaviour
{
    [Tooltip("One text box that lists all 5 rows.")]
    [SerializeField] private TMP_Text entriesText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (entriesText == null) return;

        var entries = LeaderboardStore.GetAll();
        var sb = new StringBuilder();

        // <pos=..%> lines the columns up inside one text box.
        sb.AppendLine("<b>#<pos=10%>NAME<pos=55%>SCORE<pos=75%>DATE</b>");

        for (int i = 0; i < LeaderboardStore.MaxEntries; i++)
        {
            if (i < entries.Count)
            {
                var e = entries[i];
                sb.AppendLine($"{i + 1}<pos=10%>{e.playerName}<pos=55%>{e.score}<pos=75%>{e.date}");
            }
            else
            {
                sb.AppendLine($"{i + 1}<pos=10%>---");
            }
        }

        entriesText.text = sb.ToString();
    }

    // --- Test helpers: right-click the component header to use these ---

    [ContextMenu("Test: Fill With 5 Entries")]
    private void FillTestEntries()
    {
        LeaderboardStore.Clear();
        LeaderboardStore.Add(new LeaderboardEntry("TEST A", 900));
        LeaderboardStore.Add(new LeaderboardEntry("TEST B", 800));
        LeaderboardStore.Add(new LeaderboardEntry("TEST C", 50));
        LeaderboardStore.Add(new LeaderboardEntry("TEST D", 40));
        LeaderboardStore.Add(new LeaderboardEntry("TEST E", 30));
        Refresh();
        Debug.Log("Leaderboard filled with 5 test entries.");
    }

    [ContextMenu("Test: Clear Leaderboard")]
    private void ClearEntries()
    {
        LeaderboardStore.Clear();
        Refresh();
        Debug.Log("Leaderboard cleared.");
    }
}