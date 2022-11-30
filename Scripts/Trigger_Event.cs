using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Derives from <see cref="PlayerTrigger"/>. Triggers a UnityEvent when player collides. 
/// </summary>
public class Trigger_Event : PlayerTrigger
{
    [SerializeField] private UnityEvent action;

    protected override void Execute() {
        base.Execute();
        if (action != null) action.Invoke();
    }
}
