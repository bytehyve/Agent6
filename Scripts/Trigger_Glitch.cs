using UnityEngine;

/// <summary>
/// Derives from <see cref="PlayerTrigger"/>. Glitches the screen when player collides. 
/// </summary>
public class Trigger_Glitch : PlayerTrigger
{
    [SerializeField] private float magnitude;

    protected override void Execute() {
        base.Execute();
        WorldManager.ExecuteScreenGlitch(magnitude);
    }
}
