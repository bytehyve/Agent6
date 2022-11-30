using UnityEngine;
using UnityEngine.Events;
using JaimesUtilities;

/*
 *  Uses the PassiveGhostManager to be enabled, 
 *  Then setActive when player sees it, 
 *  Then setDeactive after certain time.
 */

/// <summary>
/// Appearing and dissapearing object. Depended on parent <see cref="PassiveGhostManager"/>.
/// </summary>
public class AIGhostPassive : MonoBehaviour
{
    const float MINDISTANCE = 3;

    public enum State {
        Deactive,
        Pending,
        Active
    }

    [SerializeField] private UnityEvent onStartEvent;
    [SerializeField] private UnityEvent onPendingEvent;
    [SerializeField] private UnityEvent onActiveEvent;
    [SerializeField] private UnityEvent onDeactivateEvent;
    [SerializeField] private bool ignoreManager = false;
    [SerializeField] private bool ignoreConditions = false;
    [SerializeField] private bool autoDestroy = false;
    [SerializeField] private Transform parentBody;

    private State state;
    private float activeTime;

    private Vector3 PlayerVector => PlayerController.instance.cameraBody.position - (transform.position + Vector3.up * 1.5f);
    private bool Condition_MinDistance => PlayerVector.magnitude > MINDISTANCE;
    private bool Condition_InFoV => Vector3.Angle(-PlayerVector, PlayerController.instance.cameraBody.forward) < PlayerController.cameraFOV / 1.3f;
    private bool Condition_InView => !Physics.Raycast(transform.position + Vector3.up * 1.5f, PlayerVector.normalized, PlayerVector.magnitude, WorldManager.Instance.settings.collideMask);

    private void Start() {
        parentBody.gameObject.SetActive(false);
        if (onStartEvent != null) onStartEvent.Invoke();
    }

    private void FixedUpdate() {
        UpdateState();

        if (state != State.Deactive) {
            transform.rotation = Quaternion.Euler(0, PlayerVector.AngleAxis(Vector3.forward, Vector3.up), 0);
        }
    }

    private void UpdateState() {
        switch (state) {
            case State.Pending:
                activeTime -= Time.deltaTime / 20f;

                if (!ignoreConditions && (!Condition_MinDistance || !Condition_InView)) {
                    SetDeactive();
                    return;
                }
                if (Condition_InFoV && Condition_InView) {
                    SetActive(activeTime);
                    return;
                }
                
                break;

            case State.Active:
                activeTime -= Time.deltaTime;

                if (activeTime < 0) {
                    SetDeactive();
                    return;
                }
                break;
        }
    }

    public bool CanAppear() {
        return !ignoreManager && state == State.Deactive && Condition_MinDistance && Condition_InView && (!Condition_InFoV || RandomBlinkControl.IsActive);
    }

    public void SetDeactive() {
        state = State.Deactive;
        if (!ignoreManager) PassiveGhostManager.GhostActive = false;
        parentBody.gameObject.SetActive(false);

        if (onDeactivateEvent != null) onDeactivateEvent.Invoke();

        if (autoDestroy) {
            if (PassiveGhostManager.GhostList.Contains(this)) PassiveGhostManager.GhostList.Remove(this);
            Destroy(gameObject);
        }
    }
    public void SetPending(float activeTime) {
        this.activeTime = activeTime;
        state = State.Pending;
        parentBody.gameObject.SetActive(true);

        if (!ignoreManager) PassiveGhostManager.GhostActive = true;

        if (onPendingEvent != null) onPendingEvent.Invoke();
    }
    private void SetActive(float activeTime) {
        this.activeTime = activeTime;
        state = State.Active;

        if (onActiveEvent != null) onActiveEvent.Invoke();
    }
}
