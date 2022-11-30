using UnityEngine;

/// <summary>
/// Targetable for <see cref="PlayerController"/> to aim.
/// </summary>
public class AimTarget : MonoBehaviour
{
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private bool showGizmos = false;

    private void Start() {
        PlayerController.instance.aimTargets.Add(this);
    }

    public Vector3 GetAimPoint() {
        return transform.TransformDirection(targetOffset) + transform.position;
    }

    private void OnDisable() {
        Remove();
    }
    private void OnDestroy() {
        Remove();   
    }
    public void Remove() {
        PlayerController.instance.aimTargets.Remove(this);
    }

    private void OnDrawGizmos() {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetAimPoint(), 0.1f);
    }
}
