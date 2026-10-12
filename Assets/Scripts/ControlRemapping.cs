
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControlRemapping : MonoBehaviour
{
    // References to both players' movement scripts
    public LocalPlayerMovement player1Movement;
    public LocalPlayerMovement player2Movement;

    // References to both players' attack scripts
    public PlayerAttack player1Attack;
    public PlayerAttack player2Attack;

    // References to the menu panels
    public GameObject controlsPanel;
    public GameObject optionsPanel;

    // Text displayed on each player's control buttons
    // Order: Left, Right, Jump, Attack
    public TMP_Text[] player1KeyTexts;
    public TMP_Text[] player2KeyTexts;

    // Tracks the control currently being remapped
    // -1 means no control is selected
    private int selectedPlayer = -1;
    private int selectedAction = -1;

    // Prevent both menu panels from loading saved controls twice
    private static bool controlsLoaded = false;

    // Default controls for both players
    private readonly Key[,] defaultKeys = new Key[2, 4]
    {
        { Key.A, Key.D, Key.W, Key.F },
        { Key.LeftArrow, Key.RightArrow, Key.UpArrow, Key.RightShift }
    };

    private void Awake()
    {
        // Load saved controls once when the game starts
        if (!controlsLoaded)
        {
            LoadControls();
            controlsLoaded = true;
        }
    }

    private void Start()
    {
        // Display the current keybindings
        RefreshAllKeyTexts();
    }

    private void OnEnable()
    {
        // Update button labels whenever this panel is opened
        if (player1Movement != null && player2Movement != null &&
            player1Attack != null && player2Attack != null)
        {
            RefreshAllKeyTexts();
        }
    }

    private void Update()
    {
        // Only check keyboard input while remapping
        if (selectedPlayer == -1 || Keyboard.current == null)
            return;

        // Check which key the player pressed
        foreach (Key key in System.Enum.GetValues(typeof(Key)))
        {
            if (key == Key.None)
                continue;

            if (Keyboard.current[key].wasPressedThisFrame)
            {
                // Assign the new key
                AssignKey(key);
                break;
            }
        }
    }

    // Called when the player clicks a keybind button
    public void StartRemapping(int player, int action)
    {
        // Check that the player and action are valid
        if (player < 1 || player > 2 || action < 0 || action > 3)
            return;

        // Restore any previously selected button
        RestoreSelectedKeyText();

        // Store the selected player and action
        selectedPlayer = player;
        selectedAction = action;

        // Find the correct button text
        TMP_Text label = player == 1
            ? player1KeyTexts[action]
            : player2KeyTexts[action];

        // Ask the player to press a new key
        label.text = "PRESS KEY";
    }

    // Assigns a new key to the selected action
    private void AssignKey(Key newKey)
    {
        int player = selectedPlayer;
        int action = selectedAction;

        // Apply the new key to the selected player
        SetKey(player, action, newKey);

        // Save the custom keybinding
        SaveKey(player, action, newKey);

        // Finish remapping
        selectedPlayer = -1;
        selectedAction = -1;

        // Update the labels
        RefreshAllKeyTexts();
    }

    // Changes a player's movement or attack key
    private void SetKey(int player, int action, Key newKey)
    {
        LocalPlayerMovement movement = player == 1
            ? player1Movement
            : player2Movement;

        PlayerAttack attack = player == 1
            ? player1Attack
            : player2Attack;

        switch (action)
        {
            case 0:
                movement.leftKey = newKey;
                break;

            case 1:
                movement.rightKey = newKey;
                break;

            case 2:
                movement.jumpKey = newKey;
                break;

            case 3:
                attack.SetAttackKey(newKey);
                break;
        }
    }

    // Saves a custom keybinding
    private void SaveKey(int player, int action, Key key)
    {
        string saveName = "P" + player + "_Control_" + action;

        // Store the key as an integer
        PlayerPrefs.SetInt(saveName, (int)key);

        // Save changes
        PlayerPrefs.Save();
    }

    // Loads saved controls for both players
    private void LoadControls()
    {
        for (int player = 1; player <= 2; player++)
        {
            for (int action = 0; action < 4; action++)
            {
                string saveName = "P" + player + "_Control_" + action;

                // Use the default key if no custom key was saved
                Key key = defaultKeys[player - 1, action];

                if (PlayerPrefs.HasKey(saveName))
                {
                    Key savedKey = (Key)PlayerPrefs.GetInt(saveName);

                    // Only load valid keyboard keys
                    if (savedKey != Key.None &&
                        System.Enum.IsDefined(typeof(Key), savedKey))
                    {
                        key = savedKey;
                    }
                }

                // Apply the loaded or default key
                SetKey(player, action, key);
            }
        }
    }

    // Gets the currently assigned key
    private Key GetCurrentKey(int player, int action)
    {
        LocalPlayerMovement movement = player == 1
            ? player1Movement
            : player2Movement;

        PlayerAttack attack = player == 1
            ? player1Attack
            : player2Attack;

        switch (action)
        {
            case 0:
                return movement.leftKey;

            case 1:
                return movement.rightKey;

            case 2:
                return movement.jumpKey;

            case 3:
                return attack.GetAttackKey();
        }

        return Key.None;
    }

    // Converts key names into shorter labels for the UI
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

    // Updates all the displayed control buttons
    private void RefreshAllKeyTexts()
    {
        if (player1Movement == null || player2Movement == null ||
            player1Attack == null || player2Attack == null)
            return;

        for (int action = 0; action < 4; action++)
        {
            // Update Player 1 labels
            if (player1KeyTexts != null &&
                action < player1KeyTexts.Length &&
                player1KeyTexts[action] != null)
            {
                player1KeyTexts[action].text =
                    FormatKey(GetCurrentKey(1, action));
            }

            // Update Player 2 labels
            if (player2KeyTexts != null &&
                action < player2KeyTexts.Length &&
                player2KeyTexts[action] != null)
            {
                player2KeyTexts[action].text =
                    FormatKey(GetCurrentKey(2, action));
            }
        }
    }

    // Restores the displayed key if remapping is cancelled
    private void RestoreSelectedKeyText()
    {
        if (selectedPlayer == -1 || selectedAction == -1)
            return;

        TMP_Text label = selectedPlayer == 1
            ? player1KeyTexts[selectedAction]
            : player2KeyTexts[selectedAction];

        label.text = FormatKey(
            GetCurrentKey(selectedPlayer, selectedAction)
        );
    }

    // -----------------------------------------
    // RESET DEFAULT CONTROLS
    // -----------------------------------------

    // Restores both players to their original controls
    public void ResetToDefaults()
    {
        // Cancel any unfinished remapping
        selectedPlayer = -1;
        selectedAction = -1;

        // Reset every control for both players
        for (int player = 1; player <= 2; player++)
        {
            for (int action = 0; action < 4; action++)
            {
                // Restore the original key
                Key defaultKey = defaultKeys[player - 1, action];

                SetKey(player, action, defaultKey);

                // Remove the saved custom keybinding
                string saveName = "P" + player + "_Control_" + action;
                PlayerPrefs.DeleteKey(saveName);
            }
        }

        // Save the reset settings
        PlayerPrefs.Save();

        // Update the button labels
        RefreshAllKeyTexts();
    }

    // -----------------------------------------
    // PLAYER 1 CONTROL BUTTONS
    // -----------------------------------------

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

    // -----------------------------------------
    // PLAYER 2 CONTROL BUTTONS
    // -----------------------------------------

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

    // -----------------------------------------
    // MENU NAVIGATION
    // -----------------------------------------

    // Opens Controls from the Options menu
    public void OpenControls()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        RefreshAllKeyTexts();
    }

    // Returns from Controls to Options
    public void ReturnToOptions()
    {
        // Restore the label if remapping was cancelled
        RestoreSelectedKeyText();

        selectedPlayer = -1;
        selectedAction = -1;

        // Show Options and hide Controls
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }
}
