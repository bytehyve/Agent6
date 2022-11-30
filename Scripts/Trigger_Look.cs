using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Triggers a UnityEvent when player looks at it. 
/// </summary>
public class Trigger_Look : MonoBehaviour
{
    [SerializeField] private float waitTimer;
    [SerializeField] private float minimumDistance;
    [SerializeField] private float maxAngle;
    [SerializeField] private UnityEvent action;

    private float waitTimerCurr;
    private bool isExecuting;

    void FixedUpdate() {
        if (isExecuting) { 
            if (waitTimerCurr < Time.time) Execute();
            return;
        }

        Vector3 playerVector = PlayerController.instance.cameraBody.position - transform.position;

        bool condition_InView = !Physics.Raycast(transform.position, playerVector.normalized, playerVector.magnitude, WorldManager.Instance.settings.collideMask);
        bool condition_OutOfFOV = Vector3.Angle(-playerVector, PlayerController.instance.cameraBody.forward) > maxAngle;
        bool condition_MinDistance = playerVector.magnitude < minimumDistance;

        if (condition_InView && !condition_OutOfFOV && condition_MinDistance) {
            isExecuting = true;
        }
    }

    protected virtual void Execute() {
        if (action != null) action.Invoke();
        enabled = false;
    }
}
