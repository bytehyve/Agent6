using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers exclusively <see cref="DoorControl"/>. Can only open when looking at it. 
/// </summary>
public class DoorSwitch : MonoBehaviour
{
    [SerializeField] private string hintText = "Activate switch with [Action]";
    private DoorControl parent => transform.parent.GetComponent<DoorControl>();
    private bool canOpen;

    private void Update() {
        if (!LookingAt()) canOpen = false;

        if (enabled && canOpen) {
            if (JaimesUtilities.SaveLoad.SettingsManager.GetInput("Action", out KeyCode actionKey) && Input.GetKeyDown(actionKey)) {
                parent.Unlock();
                        
                if (parent.isSecret) PlayerController.instance.DisplayMessage("Secret door unlocked.");
                else PlayerController.instance.DisplayMessage("Door unlocked.");
            }
        }
    }

    private bool LookingAt() {
        Vector3 vector = transform.position - PlayerController.instance.cameraBody.position;
        if (Vector3.Angle(vector, PlayerController.instance.cameraBody.forward) < 30f) return true;
        return false;
    }

    public void OnTriggerStay(Collider other) {
        if (enabled && LookingAt() && WorldManager.Instance.settings.playerMask == (WorldManager.Instance.settings.playerMask | (1 << other.gameObject.layer))) {
            PlayerController.instance.DisplayHint(hintText);
            canOpen = true;
        }
    }
    public void OnTriggerExit(Collider other) {
        if (WorldManager.Instance.settings.playerMask == (WorldManager.Instance.settings.playerMask | (1 << other.gameObject.layer))) {
            canOpen = false;
        }
    }
}
