using UnityEngine;
using JaimesUtilities.AStarManual;

/// <summary>
/// Derives from <see cref="Damageable"/>. Uses AStar for pathfinding.
/// </summary>
[RequireComponent(typeof(PathfindingAgent))]
[RequireComponent(typeof(Rigidbody))]
public class AIController : Damageable
{
    [SerializeField] protected Transform parentBody;
    [SerializeField] protected Animator animator;
    [SerializeField] protected float moveSpeed;
    [SerializeField] private bool randomVaryMoveSpeed = true;

    protected PathfindingAgent agent;
    protected Rigidbody rb;
    protected Vector3 groundNormalLerped;

    protected virtual void Awake() {
        agent = GetComponent<PathfindingAgent>();
        rb = GetComponent<Rigidbody>();
        if (randomVaryMoveSpeed) moveSpeed *= Random.Range(0.8f, 1.2f);
    }
    protected virtual void Start() {
        if (agent.manager == null) agent.manager = WorldManager.Instance.pathfindingManager;
    }
    protected virtual void FixedUpdate() {
        if (!IsExpired) { 
            SetGroundNormal();
            GetInput();
        }
    }

    protected virtual void GetInput() {
        agent.SetTargetPosition(PlayerController.instance.transform.position);
    }

    protected void UpdateVelocity(Vector3 targetVelocity) {
        rb.AddForce(targetVelocity * moveSpeed, ForceMode.VelocityChange);
        rb.AddForce(-groundNormalLerped * WorldManager.Instance.settings.gravity * 2f, ForceMode.VelocityChange);
    }
    protected void UpdateRotation(float yRotation, float lerpSpeed = 10f) {
        parentBody.rotation = Quaternion.Slerp(parentBody.rotation, Quaternion.Euler(0, yRotation, 0), lerpSpeed * Time.deltaTime);
    }

    protected override void OnExpire() {
        if (GetComponent<AimTarget>() != null) GetComponent<AimTarget>().Remove();
        agent.SetStatus(PathfindingAgent.Status.Sleep);

        base.OnExpire();
    }

    private void SetGroundNormal() {
        Vector3 result = Vector3.up;
        if (Physics.Raycast(transform.position, Vector3.Lerp(-transform.up, agent.GetTargetDirection(), agent.GetTargetDirection().magnitude), out RaycastHit hitInfo, 2f, WorldManager.Instance.settings.staticCollideMask)) {
            float angle = Vector3.Angle(hitInfo.normal, result);
            if (angle < 48) result = hitInfo.normal;
        }

        groundNormalLerped = Vector3.Lerp(groundNormalLerped, result, 15f * Time.deltaTime);
    }
}
