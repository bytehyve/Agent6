using UnityEngine;

/// <summary>
/// Derives from <see cref="PlayerTrigger"/>. Glitches objects when player collides. 
/// </summary>
public class Trigger_MaterialGlitch : PlayerTrigger
{
    [SerializeField] private float strength;
    
    protected override void Execute() {
        base.Execute();
        WorldManager.SetSoldierGlitchStrength(strength);
    }
}
