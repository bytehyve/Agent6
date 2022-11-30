using UnityEngine;
using UnityEngine.Events;
using JaimesUtilities;

/// <summary>
/// Derives from <see cref="AIController"/>. Follows player on active.
/// </summary>
public class AIGhostHostile : AIController
{
    private enum AnimationTypes {
        ArmCrawling,
        Crawling,
        Walking
    }

    static bool IsActive;

    [SerializeField] private AudioSource activeAudioPlayer;
    [SerializeField] private UnityEvent onAttackEvent;
    [SerializeField] private AnimationTypes animationType;
    [SerializeField] private bool ignoreOtherActives;
    [SerializeField] private bool activateOnAwake;
    [SerializeField] private float acceleration;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    private Vector3 moveInputLerped;
    private float targetMoveSpeed;
    private bool seenPlayer;

    protected override void Awake() {
        base.Awake();

        parentBody.gameObject.SetActive(false);
        animator.SetInteger("AnimationIndex", (int)animationType);
        IsActive = false;

        targetMoveSpeed = moveSpeed;
    }
    protected override void FixedUpdate() {
        base.FixedUpdate();
        animator.SetFloat("Speed", moveSpeed);

        if (!IsExpired) { 
            UpdateStateMachine();
            agent.SetStatus(seenPlayer ? JaimesUtilities.AStarManual.PathfindingAgent.Status.Active : JaimesUtilities.AStarManual.PathfindingAgent.Status.Sleep);

            if (seenPlayer) { 
                UpdateVelocity();
                UpdateRotation();
            }
        }
    }

    private void UpdateStateMachine() {
        // If active
        if (seenPlayer) {
            // Check if in attack range
            if ((transform.position - PlayerController.instance.cameraBody.position).Horizontal().magnitude < 2f) {
                PlayerController.instance.OnDamage(transform.position, 15);
                if (onAttackEvent != null) onAttackEvent.Invoke();
                OnHit(9999);
            }
            return;
        }

        // If inactive
        // Set conditions
        Vector3 heightOffset = Vector3.up * 1.5f;
        Vector3 playerVector = PlayerController.instance.cameraBody.position - (transform.position + heightOffset);
        float playerDistance = playerVector.magnitude;
        
        bool condition_InRange = playerDistance < maxDistance && playerDistance > minDistance;
        bool condition_InView = !Physics.Raycast(transform.position + heightOffset, playerVector.normalized, playerVector.magnitude, WorldManager.Instance.settings.collideMask);
        bool condition_OutOfFOV = Vector3.Angle(-playerVector, PlayerController.instance.cameraBody.forward) > PlayerController.cameraFOV * 1.1f;

        // Set active if all conditions are met
        if ((!IsActive || ignoreOtherActives) && (activateOnAwake || condition_InRange && condition_InView && condition_OutOfFOV)) {
            activeAudioPlayer.Play();

            parentBody.gameObject.SetActive(true);

            if (!ignoreOtherActives) IsActive = true;
            seenPlayer = true;
        }
    }
    protected override void GetInput() {
        base.GetInput();

        if (seenPlayer) targetMoveSpeed += acceleration * Time.deltaTime;
        moveSpeed = RandomBlinkControl.IsActive ? targetMoveSpeed * 3f : targetMoveSpeed;

        moveInputLerped = Vector3.Lerp(moveInputLerped, seenPlayer ? agent.GetTargetDirection() : Vector3.zero, 10f * Time.deltaTime);
    }
    private void UpdateVelocity() {
        UpdateVelocity((-Vector3.Cross(groundNormalLerped, Vector3.right) * moveInputLerped.z + Vector3.Cross(groundNormalLerped, Vector3.forward) * moveInputLerped.x));
    }
    private void UpdateRotation() {
        UpdateRotation((PlayerController.instance.cameraBody.position - transform.position).Horizontal().AngleAxis(Vector3.forward, Vector3.up));
    }

    protected override void OnExpire() {
        base.OnExpire();

        if (!ignoreOtherActives) IsActive = false;
        activeAudioPlayer.Stop();
    }
}
