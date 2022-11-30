using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// Triggers exclusively <see cref="DoorControl"/>. Can be opened and closed. 
/// </summary>
public class Trigger_Door : MonoBehaviour
{
    [SerializeField] private bool autoUnlockOnEnter;
    [SerializeField] private UnityEvent onInteractEvent;

    private DoorControl parent => transform.parent.GetComponent<DoorControl>();
    private List<Collider> collidingObjects;
    private bool collidingWithAI;

    private void Start() {
        collidingObjects = new List<Collider>();
    }

    private void Update() {
        for (int i = collidingObjects.Count - 1; i >= 0; i--) {
            if (collidingObjects[i] == null) collidingObjects.RemoveAt(i);
        }

        if (!parent.IsLocked && !autoUnlockOnEnter) { 
            if (collidingObjects.Any(x => WorldManager.Instance.settings.targetHitMask == (WorldManager.Instance.settings.targetHitMask | (1 << x.gameObject.layer)))) {
                parent.Open();
                collidingWithAI = true;
            }
            else {
                if (collidingWithAI) {
                    parent.Close();
                }
                collidingWithAI = false;
            }
        }

        if (collidingObjects.Any(x => WorldManager.Instance.settings.playerMask == (WorldManager.Instance.settings.playerMask | (1 << x.gameObject.layer)))) {
            if (autoUnlockOnEnter) {
                parent.Unlock();
            }
            else if (JaimesUtilities.SaveLoad.SettingsManager.GetInput("Action", out KeyCode actionKey) && Input.GetKeyDown(actionKey)) {
                if (!parent.IsLocked) { 
                    if (parent.IsOpen) parent.Close();
                    else parent.Open();
                    if (onInteractEvent != null) onInteractEvent.Invoke();
                }
                else {
                    PlayerController.instance.DisplayMessage("Door is locked");
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other) {
        collidingObjects.Add(other);
    }
    public void OnTriggerExit(Collider other) {
        collidingObjects.Remove(other);
    }
}
