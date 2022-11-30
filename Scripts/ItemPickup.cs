using UnityEngine;

// This script needs a custom editor.

/// <summary>
/// Gives the player an item when colliding.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public enum Type { Equipment, BodyArmor, Ammo }
    public Type type;
    [Tooltip("Can be left empty if type != Equipment")]
    public WeaponInfo equipment;

    private bool destroyed;

    private void Start() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 5f, WorldManager.Instance.settings.collideMask)) {
            transform.position = hitInfo.point;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!destroyed && WorldManager.Instance.settings.playerMask == (WorldManager.Instance.settings.playerMask | (1 << other.gameObject.layer))) {
            switch (type) {
                case Type.Equipment:
                    PlayerController.instance.ObtainEquipment(equipment);
                    break;

                case Type.BodyArmor:
                    PlayerController.instance.ObtainArmor(true);
                    break;

                case Type.Ammo:
                    PlayerController.instance.ObtainAmmo(true);
                    break;
            }

            GetComponent<AudioSource>().Play();
            GetComponent<Collider>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);

            Destroy(gameObject, 2f);
        }
    }
}
