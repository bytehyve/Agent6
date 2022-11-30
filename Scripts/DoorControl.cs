using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls an object as a door.
/// </summary>
public class DoorControl : MonoBehaviour
{
    private const float moveSpeed = 2.5f;

    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] public bool isSecret;
    [SerializeField] public bool autoOpenOnUnlock;
    [SerializeField] private Transform movingPart;
    [SerializeField] private Vector3 targetOpenPosition;
    [SerializeField] private bool showGizmos;

    [Header("Events")]
    [SerializeField] private UnityEvent OnOpen;

    private Vector3 defaultPosition;
    
    [HideInInspector] public bool isBlocked;
    public bool IsOpen => isOpen;
    public bool IsLocked => isLocked;

    private void Start() {
        defaultPosition = movingPart.localPosition;
    }

    private void FixedUpdate() {
        Vector3 targetPos = isOpen ? targetOpenPosition : defaultPosition;
        Vector3 vec = (targetPos - movingPart.localPosition);
        Vector3 dir = vec.normalized;
        float forc = Mathf.Min(moveSpeed * Time.deltaTime, vec.magnitude);

        if (isOpen || !isBlocked) movingPart.localPosition += dir * forc;
    }

    public void Open() {
        isOpen = true;
        if (OnOpen != null) OnOpen.Invoke();
    }
    public void Close() {
        isOpen = false;
    }
    public void Unlock() {
        isLocked = false;
        if (autoOpenOnUnlock) Open();
    }
    public void Lock() {
        isLocked = true;
        Close();
    }

    private void OnDrawGizmos() {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(movingPart.position, movingPart.position + transform.TransformDirection(targetOpenPosition));
        Gizmos.DrawSphere(movingPart.position + transform.TransformDirection(isOpen ? defaultPosition : targetOpenPosition), 0.3f);
    }
}
