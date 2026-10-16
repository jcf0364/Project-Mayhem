using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Drives the profile pick screen. Writes the chosen ids into GameSession,
/// which is what survives the trip to the Battle scene.
public class ProfileSelectUI : MonoBehaviour
{
    [Header("Player 1")]
    [SerializeField] private TMP_Dropdown dropdownP1;

    [Header("Player 2")]
    [SerializeField] private TMP_Dropdown dropdownP2;
    [SerializeField] private Toggle guestToggle;

    [Header("New Profile")]
    [SerializeField] private GameObject newProfilePanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text guestLabel;   // optional
    [SerializeField] private TMP_Text errorLabel;

    [Header("Flow")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject GameModePanel;
    [SerializeField] private GameObject DifficultyPanel;
    [SerializeField] private SceneLoader sceneLoader;

    private List<PlayerProfile> profiles;

    // Which slot asked for a new profile, so we can select it afterwards.
    private int slotBeingCreated;

    private void Start()
    {
        // Every panel starts hidden. Only one is ever shown at a time.
        if (newProfilePanel != null) newProfilePanel.SetActive(false);
        if (GameModePanel != null) GameModePanel.SetActive(false);
        if (DifficultyPanel != null) DifficultyPanel.SetActive(false);
        if (errorLabel != null) errorLabel.text = "";

        RefreshDropdowns();
        OnGuestToggled(guestToggle != null && guestToggle.isOn);
    }

    /// Rebuilds both dropdown lists from the saved profiles.
    /// Called on load and again after a profile is created.
    private void RefreshDropdowns()
    {
        profiles = ProfileStore.GetAll();

        var names = profiles.Select(p => p.displayName).ToList();

        FillDropdown(dropdownP1, names);
        FillDropdown(dropdownP2, names);

        UpdateConfirmState();
    }

    private void FillDropdown(TMP_Dropdown dropdown, List<string> names)
    {
        if (dropdown == null) return;

        int previous = dropdown.value;

        dropdown.ClearOptions();
        dropdown.AddOptions(names);

        // Keep the previous pick where possible, so creating a profile for
        // slot two doesn't reset slot one.
        dropdown.value = Mathf.Clamp(previous, 0, Mathf.Max(names.Count - 1, 0));
        dropdown.RefreshShownValue();

        // Nothing to pick from on a fresh install.
        dropdown.interactable = names.Count > 0;
    }

    // --- Button handlers ---

    public void OnNewProfileP1() => OpenNewProfile(1);
    public void OnNewProfileP2() => OpenNewProfile(2);

    private void OpenNewProfile(int slot)
    {
        slotBeingCreated = slot;

        if (newProfilePanel != null) newProfilePanel.SetActive(true);
        if (nameInput != null)
        {
            nameInput.text = "";
            nameInput.Select();
        }
        if (errorLabel != null) errorLabel.text = "";
    }

    public void OnCancelNewProfile()
    {
        if (newProfilePanel != null) newProfilePanel.SetActive(false);
    }

    public void OnCreateProfile()
    {
        string name = nameInput != null ? nameInput.text.Trim() : "";

        if (string.IsNullOrEmpty(name))
        {
            ShowError("Name cannot be empty.");
            return;
        }

        if (name.Length > 12)
        {
            // Keeps the leaderboard readable.
            ShowError("Name must be 12 characters or fewer.");
            return;
        }

        if (ProfileStore.NameTaken(name))
        {
            ShowError("That name is already taken.");
            return;
        }

        ProfileStore.Create(name);
        RefreshDropdowns();

        // Point the slot that asked at the profile just made — it's last
        // in the list, since Create appends.
        int lastIndex = profiles.Count - 1;
        if (slotBeingCreated == 1 && dropdownP1 != null)
        {
            dropdownP1.value = lastIndex;
            dropdownP1.RefreshShownValue();
        }
        else if (slotBeingCreated == 2 && dropdownP2 != null)
        {
            dropdownP2.value = lastIndex;
            dropdownP2.RefreshShownValue();
        }

        if (newProfilePanel != null) newProfilePanel.SetActive(false);
    }

    public void OnGuestToggled(bool isGuestFromEvent)
    {
        // Read the toggle directly instead of trusting the event argument.
        // If this ever gets wired to a Static parameter in the Inspector,
        // the argument is a fixed value and always wrong. The toggle isn't.
        bool isGuest = guestToggle != null ? guestToggle.isOn : isGuestFromEvent;

        bool hasProfiles = profiles != null && profiles.Count > 0;

        // Show the dropdown OR the guest label. Never both, never neither.
        if (dropdownP2 != null)
        {
            dropdownP2.gameObject.SetActive(!isGuest);
            dropdownP2.interactable = !isGuest && hasProfiles;
        }
        if (guestLabel != null)
            guestLabel.gameObject.SetActive(isGuest);

        // Set BOTH values every time, in both directions. Only clearing
        // the id on tick left stale state behind on the next untick.
        if (GameSession.Instance != null)
        {
            GameSession.Instance.player2IsGuest = isGuest;

            if (isGuest)
                GameSession.Instance.player2ProfileId = "";
            else if (hasProfiles && dropdownP2 != null)
                GameSession.Instance.player2ProfileId = profiles[dropdownP2.value].id;
        }

        UpdateConfirmState();
    }

    public void OnConfirm()
    {
        if (GameSession.Instance == null) return;
        if (profiles.Count == 0) return;

        GameSession.Instance.player1ProfileId = profiles[dropdownP1.value].id;

        bool guest = guestToggle != null && guestToggle.isOn;
        GameSession.Instance.player2IsGuest = guest;
        GameSession.Instance.player2ProfileId =
            guest ? "" : profiles[dropdownP2.value].id;

        // Guest means nobody is sitting at the second controller.
        // Ask what to do with that slot before starting the match.
        if (guest && GameModePanel != null)
        {
            GameModePanel.SetActive(true);
            return;                      // stop here, the panel takes over
        }

        // Two real profiles: a normal local match.
        if (sceneLoader != null) sceneLoader.LoadCharacterSelect();
    }

    public void OnBack()
    {
        if (sceneLoader != null) sceneLoader.LoadMainMenu();
    }

    // --- Game mode panel ---

    public void OnModeVersusAI()
    {
        // The difficulty buttons load the Battle AI scene themselves,
        // so this only swaps which panel is showing.
        if (GameModePanel != null) GameModePanel.SetActive(false);
        if (DifficultyPanel != null) DifficultyPanel.SetActive(true);
    }

    public void OnModeLocalGuest()
    {
        // A human friend on the second controller, just not signed in.
        if (GameModePanel != null) GameModePanel.SetActive(false);
        if (sceneLoader != null) sceneLoader.LoadCharacterSelect();
    }

    public void OnModesBack()
    {
        // Back out to change the profile choice.
        if (GameModePanel != null) GameModePanel.SetActive(false);
    }

    public void OnDifficultyBack()
    {
        // Back from difficulty to the mode choice.
        if (DifficultyPanel != null) DifficultyPanel.SetActive(false);
        if (GameModePanel != null) GameModePanel.SetActive(true);
    }

    // --- Helpers ---

    private void UpdateConfirmState()
    {
        if (confirmButton == null) return;

        bool guest = guestToggle != null && guestToggle.isOn;

        // Solo needs one profile. Two-player needs two different ones.
        bool ready = guest ? profiles.Count > 0 : profiles.Count > 1;

        confirmButton.interactable = ready;
    }

    private void ShowError(string message)
    {
        if (errorLabel != null) errorLabel.text = message;
    }
}