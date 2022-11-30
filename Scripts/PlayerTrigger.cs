using UnityEngine;

/// <summary>
/// Triggers a single time <seealso cref="Execute"/> when colliding with player. 
/// </summary>
public class PlayerTrigger : MonoBehaviour
{
    [SerializeField] private float waitTimer;

    private float waitTimerCurr;
    private bool isExecuting;
    
    private void FixedUpdate() {
        if (isExecuting && waitTimerCurr < Time.time) Execute();
    }

    private void OnTriggerStay(Collider other) {
        if (enabled && !isExecuting && WorldManager.Instance.settings.playerMask == (WorldManager.Instance.settings.playerMask | (1 << other.gameObject.layer))) {
            waitTimerCurr = Time.time + waitTimer;
            isExecuting = true;
            return;
        }
    }

    protected virtual void Execute() {
        enabled = false;
    }
}
