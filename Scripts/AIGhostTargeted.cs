using UnityEngine;
using JaimesUtilities;

/// <summary>
/// Derives from <see cref="AIController"/>. This will move to a target destination and autoDestroy.
/// </summary>
public class AIGhostTargeted : AIController
{
    private enum AnimationTypes {
        ArmCrawling,
        Crawling,
        Walking
    }

    [SerializeField] private AnimationTypes animationType;
    [SerializeField] private Transform targetPoint;

    private Vector3 moveInputLerped;

    protected override void Awake() {
        base.Awake();
        animator.SetInteger("AnimationIndex", (int)animationType);
    }
    protected override void FixedUpdate() {
        animator.SetFloat("Speed", moveSpeed);
        base.FixedUpdate();

        if (!IsExpired) { 
            UpdateVelocity();
            UpdateRotation();

            // Destroy when at destination
            if ((transform.position - targetPoint.position).Horizontal().magnitude < 1f) Destroy(gameObject);
        }
    }
    protected override void GetInput() {
        agent.SetTargetPosition(targetPoint.position);
        moveInputLerped = Vector3.Lerp(moveInputLerped, agent.GetTargetDirection(), 20f * Time.deltaTime);
    }
    private void UpdateVelocity() {
        UpdateVelocity((-Vector3.Cross(groundNormalLerped, Vector3.right) * moveInputLerped.z + Vector3.Cross(groundNormalLerped, Vector3.forward) * moveInputLerped.x));
    }
    private void UpdateRotation() {
        UpdateRotation(moveInputLerped.AngleAxis(Vector3.forward, Vector3.up), 60f);
    }
}
