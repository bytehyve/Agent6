using UnityEngine;

/// <summary>
/// Displays a hintText when player collides. 
/// </summary>
public class Trigger_Hint : MonoBehaviour
{
    [SerializeField] private string hintText;

    public void OnTriggerStay(Collider other) {
        if (enabled && WorldManager.Instance.settings.playerMask == (WorldManager.Instance.settings.playerMask | (1 << other.gameObject.layer))) {
            PlayerController.instance.DisplayHint(hintText);
        }
    }

    public void DestroyObject() {
        enabled = false;
    }
}
