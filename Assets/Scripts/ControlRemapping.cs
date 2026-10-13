
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControlRemapping : MonoBehaviour
{
    // References to players (optional in MainMenu scene)
    public LocalPlayerMovement player1Movement;
    public LocalPlayerMovement player2Movement;
    public PlayerAttack player1Attack;
    public PlayerAttack player2Attack;

    // Menu panels
    public GameObject controlsPanel;
    public GameObject optionsPanel;

    // Keybind button texts
    // Order: Left, Right, Jump, Attack
    public TMP_Text[] player1KeyTexts;
    public TMP_Text[] player2KeyTexts;

    // Tracks the selected control
    private int selectedPlayer = -1;
    private int selectedAction = -1;

    // Default keyboard controls
    private static readonly Key[,] defaultKeys =
    {
        { Key.A, Key.D, Key.W, Key.F },
        { Key.LeftArrow, Key.RightArrow, Key.UpArrow, Key.RightShift }
    };

    // Notifies gameplay scripts when controls change
    public static event System.Action OnBindingsChanged;

    private void Start()
    {
        RefreshAllKeyTexts();
    }

    private void OnEnable()
    {
        RefreshAllKeyTexts();
    }

    private void OnDisable()
    {
        // Cancel remapping when the panel closes
        selectedPlayer = -1;
        selectedAction = -1;
    }

    private void Update()
    {
        // Only detect keys when remapping
        if (selectedPlayer == -1 || Keyboard.current == null)
            return;

        // Check for a pressed key
        foreach (Key key in System.Enum.GetValues(typeof(Key)))
        {
            if (key == Key.None)
                continue;

            if (Keyboard.current[key].wasPressedThisFrame)
            {
                // Reserve Escape for menu navigation
                if (key == Key.Escape)
                {
                    RestoreSelectedKeyText();
                    selectedPlayer = -1;
                    selectedAction = -1;
                    return;
                }

                AssignKey(key);
                break;
            }
        }
    }

    // Starts remapping the selected action
    public void StartRemapping(int player, int action)
    {
        if (player < 1 || player > 2 || action < 0 || action > 3)
            return;

        RestoreSelectedKeyText();

        selectedPlayer = player;
        selectedAction = action;

        TMP_Text label = GetLabel(player, action);

        if (label != null)
            label.text = "PRESS KEY";
    }

    // Assigns and saves the new key
    private void AssignKey(Key newKey)
    {
        int player = selectedPlayer;
        int action = selectedAction;

        // Store the selected key
        string saveName = GetSaveName(player, action);

        PlayerPrefs.SetInt(saveName, (int)newKey);
        PlayerPrefs.Save();

        // Apply to existing players, if available
        ApplyControlsToPlayers();

        selectedPlayer = -1;
        selectedAction = -1;

        // Inform gameplay components about the change
        OnBindingsChanged?.Invoke();

        RefreshAllKeyTexts();
    }

    // Generates a unique save name
    private static string GetSaveName(int player, int action)
    {
        return "P" + player + "_Control_" + action;
    }

    // Gets a saved key or its default
    public static Key GetSavedKey(int player, int action)
    {
        if (player < 1 || player > 2 || action < 0 || action > 3)
            return Key.None;

        string saveName = GetSaveName(player, action);

        Key defaultKey = defaultKeys[player - 1, action];

        if (!PlayerPrefs.HasKey(saveName))
            return defaultKey;

        Key savedKey = (Key)PlayerPrefs.GetInt(saveName);

        // Ignore invalid saved values
        if (savedKey == Key.None ||
            !System.Enum.IsDefined(typeof(Key), savedKey))
            return defaultKey;

        return savedKey;
    }

    // Applies saved controls when player objects exist
    private void ApplyControlsToPlayers()
    {
        ApplyPlayerControls(1, player1Movement, player1Attack);
        ApplyPlayerControls(2, player2Movement, player2Attack);
    }

    // Updates movement and attack keys
    public static void ApplyPlayerControls(
        int player,
        LocalPlayerMovement movement,
        PlayerAttack attack)
    {
        if (movement != null)
        {
            movement.leftKey = GetSavedKey(player, 0);
            movement.rightKey = GetSavedKey(player, 1);
            movement.jumpKey = GetSavedKey(player, 2);
        }

        if (attack != null)
        {
            attack.SetAttackKey(GetSavedKey(player, 3));
        }
    }

    // Returns the correct text component
    private TMP_Text GetLabel(int player, int action)
    {
        TMP_Text[] texts = player == 1
            ? player1KeyTexts
            : player2KeyTexts;

        if (texts == null || action >= texts.Length)
            return null;

        return texts[action];
    }

    // Formats keys for the UI
    private string FormatKey(Key key)
    {
        switch (key)
        {
            case Key.LeftArrow:
                return "←";

            case Key.RightArrow:
                return "→";

            case Key.UpArrow:
                return "↑";

            case Key.DownArrow:
                return "↓";

            case Key.RightShift:
                return "RShift";

            case Key.LeftShift:
                return "LShift";

            case Key.Space:
                return "Space";

            default:
                return key.ToString();
        }
    }

    // Refreshes all eight keybind labels
    private void RefreshAllKeyTexts()
    {
        for (int player = 1; player <= 2; player++)
        {
            for (int action = 0; action < 4; action++)
            {
                TMP_Text label = GetLabel(player, action);

                if (label != null)
                {
                    Key currentKey = GetSavedKey(player, action);
                    label.text = FormatKey(currentKey);
                }
            }
        }
    }

    // Restores the text when remapping is cancelled
    private void RestoreSelectedKeyText()
    {
        if (selectedPlayer == -1 || selectedAction == -1)
            return;

        TMP_Text label = GetLabel(selectedPlayer, selectedAction);

        if (label != null)
        {
            Key currentKey = GetSavedKey(
                selectedPlayer,
                selectedAction
            );

            label.text = FormatKey(currentKey);
        }
    }

    // Resets both players to default controls
    public void ResetToDefaults()
    {
        selectedPlayer = -1;
        selectedAction = -1;

        // Remove all saved custom keybinds
        for (int player = 1; player <= 2; player++)
        {
            for (int action = 0; action < 4; action++)
            {
                PlayerPrefs.DeleteKey(GetSaveName(player, action));
            }
        }

        PlayerPrefs.Save();

        // Apply the default keys
        ApplyControlsToPlayers();

        OnBindingsChanged?.Invoke();

        RefreshAllKeyTexts();
    }

    // Player 1 remapping buttons
    public void RemapP1Left()
    {
        StartRemapping(1, 0);
    }

    public void RemapP1Right()
    {
        StartRemapping(1, 1);
    }

    public void RemapP1Jump()
    {
        StartRemapping(1, 2);
    }

    public void RemapP1Attack()
    {
        StartRemapping(1, 3);
    }

    // Player 2 remapping buttons
    public void RemapP2Left()
    {
        StartRemapping(2, 0);
    }

    public void RemapP2Right()
    {
        StartRemapping(2, 1);
    }

    public void RemapP2Jump()
    {
        StartRemapping(2, 2);
    }

    public void RemapP2Attack()
    {
        StartRemapping(2, 3);
    }

    // Opens the Controls menu
    public void OpenControls()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        RefreshAllKeyTexts();
    }

    // Returns to the Options menu
    public void ReturnToOptions()
    {
        RestoreSelectedKeyText();

        selectedPlayer = -1;
        selectedAction = -1;

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }
}
