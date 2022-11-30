using UnityEngine;

/// <summary>
/// Derives from <see cref="PlayerTrigger"/>. Displays a glitching screenMessage when player collides.
/// </summary>
public class Trigger_ScreenMessage : PlayerTrigger
{
    [SerializeField] private string displayMessage;

    protected override void Execute() {
        base.Execute();
        WorldManager.PlayScreenMessage(displayMessage);
    }
}
