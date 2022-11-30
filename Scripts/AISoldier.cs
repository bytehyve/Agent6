using UnityEngine;
using JaimesUtilities;
using JaimesUtilities.AStarManual;

/// <summary>
/// Derives from <see cref="AIController"/>. Uses statemachines to select behaviour.
/// </summary>
public class AISoldier : AIController
{
    // Enums
    public enum State {
        Idle,
        Searching,
        Attacking
    }
    public enum PerformingAction {
        Noone,
        Shooting
    }

    // Events
    public delegate void OnShootNearby(Vector3 position);
    public static event OnShootNearby OnShootNearbyEvent;

    // Main settings
    [SerializeField] private float targetWalkDistanceToPlayer;
    [SerializeField] private float targetViewDistanceToPlayer;
    [SerializeField] private float waitTimeToSearch;
    [SerializeField] private float reactTime;
    [SerializeField] private AIWeaponAnimator weaponAnim;
    [SerializeField] private Transform muzzleFlashBody;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject equipmentDrop;
    [SerializeField] private AudioSource shootAudioPlayer;

    // Stats
    [Header("Stats")]
    [SerializeField] private int magazineSize;
    [SerializeField] private float bloom;
    [Range(0, 1f)]
    [SerializeField] private float hitChance;
    [SerializeField] private float fireRate;
    [SerializeField] private float attackRate;

    // Input
    private State currentState;
    private float lastSeenTime;
    private Vector3 moveInputLerped;
    private float crouchInput;
    private float crouchInputLerped;

    // Update
    private float aggression;
    private float reactWaitTime;
    private float fireWaitTime;
    private float attackWaitTime;
    private int firesLeft = 0;
    private float muzzleFlashTime;
    private int animationDeathIndex = 0;
    private bool canWalk;

    protected override void Awake() {
        base.Awake();

        currentState = State.Idle;
        crouchInput = Random.Range(0, 2);
        aggression = Random.Range(1f, 1.5f);

    }
    protected override void Start() {
        base.Start();
        OnShootNearbyEvent += CheckShootNearby;
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if (!IsExpired) { 
            UpdateStateMachine();
            UpdateVelocity();
            UpdateRotation();
        
            if (currentState != State.Idle) MusicManager.OnAction();
        }
        UpdateAnimations();
    }

    private void UpdateStateMachine() {
        // Deactivate agent when not walking
        agent.SetStatus(canWalk ? PathfindingAgent.Status.Active : PathfindingAgent.Status.Sleep);
        canWalk = false;

        // Set conditions
        Vector3 heightOffset = Vector3.up * (crouchInput == 1 && currentState == State.Attacking ? 1f : 1.5f);
        Vector3 playerVector = PlayerController.instance.cameraBody.position - (transform.position + heightOffset);
        
        bool condition_InView = !Physics.Raycast(transform.position + heightOffset, playerVector.normalized, playerVector.magnitude, WorldManager.Instance.settings.collideMask);
        bool condition_InAttackRange = agent.LengthInPath < targetWalkDistanceToPlayer * aggression;
        bool condition_InViewRange = playerVector.magnitude < targetViewDistanceToPlayer;
        bool condition_InVision = condition_InView && Vector3.Angle(playerVector, parentBody.forward) < 70f;

        // Set state depending on conditions
        switch (currentState) {
            case State.Idle:
                if (condition_InVision && condition_InViewRange) {
                    lastSeenTime = Time.time;
                    currentState = State.Searching;
                    reactWaitTime = Time.time + reactTime / aggression;
                }
                break;

            case State.Searching:
                if (condition_InView && condition_InViewRange) {
                    lastSeenTime = Time.time;
                    if (reactWaitTime < Time.time) currentState = State.Attacking;
                }
                if (condition_InAttackRange && Time.time - lastSeenTime < waitTimeToSearch / aggression) {
                    if (reactWaitTime < Time.time) currentState = State.Attacking;
                }

                if (!condition_InView || !condition_InAttackRange) {
                    canWalk = true;
                }
                firesLeft = 0;
                break;
            
            case State.Attacking:
                if (condition_InView && condition_InViewRange) {
                    lastSeenTime = Time.time;
                }
                else {
                    if (crouchInput == 1) crouchInput = 0;
                }

                if (!condition_InViewRange || Time.time - lastSeenTime > waitTimeToSearch / aggression) {
                    currentState = State.Searching;
                }

                if (attackWaitTime < Time.time && firesLeft == 0) firesLeft = Mathf.CeilToInt(magazineSize * Random.Range(0.75f, 1f));
                if (attackWaitTime > Time.time) firesLeft = 0;

                if (firesLeft > 0) {
                    if (fireWaitTime < Time.time) {
                        fireWaitTime = Time.time + 1f / fireRate;
                        firesLeft--;
                        if (firesLeft == 0) attackWaitTime = Time.time + 1f / attackRate * Random.Range(2f, 5f) / aggression * (1f + (transform.position - PlayerController.instance.cameraBody.position).magnitude / 3f) / 5f;
                        
                        if (condition_InVision) OnShoot();
                    }
                }
                break;
        }

        // Set attackWaitTime when not seeing player
        if (!condition_InView || !condition_InViewRange) attackWaitTime = Mathf.Min(attackWaitTime, 
            Time.time + 1f / attackRate / aggression * (1f + (PlayerController.instance.transform.position - transform.position).magnitude / 3f));
    }
    protected override void GetInput() {
        base.GetInput();

        moveInputLerped = Vector3.Lerp(moveInputLerped, canWalk ? agent.GetTargetDirection() : Vector3.zero, 10f * Time.deltaTime);

        if (Random.Range(0, 250) == 0) crouchInput = 1f - crouchInput;
        crouchInputLerped = Mathf.Lerp(crouchInputLerped, crouchInput, 10f * Time.deltaTime);
    }
    private void UpdateVelocity() {
        UpdateVelocity((-Vector3.Cross(groundNormalLerped, Vector3.right) * moveInputLerped.z + Vector3.Cross(groundNormalLerped, Vector3.forward) * moveInputLerped.x));
    }
    private void UpdateRotation() {
        float rotateYAngle = parentBody.rotation.eulerAngles.y; // Get default/current rotation
    
        if (currentState != State.Idle) rotateYAngle = moveInputLerped.magnitude > 0.5f
            ? moveInputLerped.AngleAxis(Vector3.forward, Vector3.up)
            : (PlayerController.instance.transform.position - transform.position).Horizontal().normalized.AngleAxis(Vector3.forward, Vector3.up);

        if (currentState == State.Attacking) rotateYAngle += 50f; // Rotate 50 deg to face the player when in shooting animation

        UpdateRotation(rotateYAngle);
    }
    private void UpdateAnimations() {
        bool isAiming = currentState == State.Attacking;
        float aimInput = crouchInputLerped;
        float zInput = parentBody.InverseTransformDirection(moveInputLerped).z.Sign();

        animator.SetFloat("MoveInput", zInput);
        animator.SetFloat("AimInput", aimInput);
        animator.SetBool("IsAiming", isAiming);
        animator.SetBool("IsDead", IsExpired);
        animator.SetInteger("DeathIndex", animationDeathIndex);

        weaponAnim.isAiming = isAiming;
        weaponAnim.aimInput = aimInput;
        weaponAnim.moveInput = zInput;

        muzzleFlashBody.gameObject.SetActive(muzzleFlashTime > Time.time);
        if (muzzleFlashTime > Time.time) muzzleFlashBody.LookAt(PlayerController.instance.cameraBody, Vector3.up);
    }
    private void OnShoot() {
        // Set bullet direction
        bool hitPlayer = Random.Range(0, 100f * (1f + PlayerController.instance.GetComponent<Rigidbody>().velocity.magnitude / 5f) * (1f + (PlayerController.instance.transform.position - transform.position).magnitude / 5f)) / 70f < hitChance;
        Vector3 direction = (PlayerController.instance.transform.position - muzzleFlashBody.position).normalized;
        if (!hitPlayer) {
            Vector2 offset = Random.insideUnitCircle.normalized;
            Vector3 axis = Vector3.Cross(direction.Horizontal(), Vector3.up) * offset.x + Vector3.up * offset.y;
            direction = Quaternion.AngleAxis(Random.Range(0, bloom) * 60, axis) * direction;
        }

        // Create bulletObj
        GameObject bulletObj = Instantiate(bulletPrefab, muzzleFlashBody.position, Quaternion.identity);
        bulletObj.GetComponent<BulletControl>().targetPosition = hitPlayer ? PlayerController.instance.transform.position : muzzleFlashBody.position + direction * 20f;
        Destroy(bulletObj, 2f);
        muzzleFlashTime = Time.time + 0.1f;

        // HitPlayer
        if (hitPlayer) {
            PlayerController.instance.OnDamage(transform.position, 5);
        }

        // Play audio
        shootAudioPlayer.PlayOneShot(shootAudioPlayer.clip);
    }

    public override void OnHit(int damage) {
        base.OnHit(damage);
        
        if (currentState == State.Idle) {
            currentState = State.Searching;
            reactWaitTime = Time.time + reactTime / aggression;
        }
    }
    protected override void OnExpire() {
        OnShootNearbyEvent -= CheckShootNearby;
        
        base.OnExpire();

        Instantiate(equipmentDrop, transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
        animationDeathIndex = Random.Range(0, 3);
    }

    private void CheckShootNearby(Vector3 position) {
        if (currentState != State.Idle) return;

        if ((transform.position - position).magnitude < targetWalkDistanceToPlayer * aggression / 2f) {
            lastSeenTime = Time.time;
            currentState = State.Searching;
            reactWaitTime = Time.time + reactTime / aggression;
        }
    }
    public static void InvokeOnShootNearby(Vector3 position) {
        if (OnShootNearbyEvent != null) OnShootNearbyEvent.Invoke(position);
    }

    public static void ClearOnShootNearbyEvent() {
        if (OnShootNearbyEvent == null) return;
        foreach (OnShootNearby d in OnShootNearbyEvent.GetInvocationList()) {
            OnShootNearbyEvent -= d;
        }
    }
}
