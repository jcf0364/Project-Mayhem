
using UnityEngine;

public class PlayerControlsLoader : MonoBehaviour
{
    // References to both players' movement scripts
    public LocalPlayerMovement player1Movement;
    public LocalPlayerMovement player2Movement;

    // References to both players' attack scripts
    public PlayerAttack player1Attack;
    public PlayerAttack player2Attack;

    private void OnEnable()
    {
        // Load saved controls when the Battle scene starts
        LoadPlayerControls();

        // Update controls whenever a key is remapped or reset
        ControlRemapping.OnBindingsChanged += LoadPlayerControls;
    }

    private void OnDisable()
    {
        // Stop listening when this object is disabled
        ControlRemapping.OnBindingsChanged -= LoadPlayerControls;
    }

    // Applies saved keybindings to both players
    private void LoadPlayerControls()
    {
        // Apply Player 1's saved movement and attack keys
        ControlRemapping.ApplyPlayerControls(
            1,
            player1Movement,
            player1Attack
        );

        // Apply Player 2's saved movement and attack keys
        ControlRemapping.ApplyPlayerControls(
            2,
            player2Movement,
            player2Attack
        );
    }
}
