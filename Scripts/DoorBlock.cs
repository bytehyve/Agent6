using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for <see cref="DoorControl"/> to prevent closing when obstructed.
/// </summary>
public class DoorBlock : MonoBehaviour
{
    private DoorControl parent => transform.parent.GetComponent<DoorControl>();

    public void OnTriggerEnter(Collider other) {
        if (WorldManager.Instance.settings.playerMask == (WorldManager.Instance.settings.playerMask | (1 << other.gameObject.layer))
          || WorldManager.Instance.settings.targetHitMask == (WorldManager.Instance.settings.targetHitMask | (1 << other.gameObject.layer))) {
            parent.isBlocked = true;
        }
    }
    public void OnTriggerExit(Collider other) {
        parent.isBlocked = false;
    }
}
