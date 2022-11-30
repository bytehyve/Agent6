using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Link to a <see cref="Hitbox"/>. Will disable all childTransforms on expire.
/// </summary>
public class Damageable : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private bool disableChildrenOnExpire = true;
    [SerializeField] private Transform onExpireFxParent;
    [SerializeField] private UnityEvent onExpireEvent;

    public bool IsExpired => isExpired;
    private bool isExpired = false;

    public virtual void OnHit(int damage) {
        if (health <= 0) return;
        health -= damage;

        if (health <= 0) {
            OnExpire();
        }
    }

    protected virtual void OnExpire() {
        isExpired = true;

        // (De)activate correct objects
        if (disableChildrenOnExpire) { 
            for (int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        foreach (Hitbox hitbox in GetComponentsInChildren<Hitbox>()) hitbox.gameObject.SetActive(false);

        // Enable Fx
        if (onExpireFxParent != null) {
            onExpireFxParent.parent = null;
            onExpireFxParent.gameObject.SetActive(true);
        }

        // Invoke event
        if (onExpireEvent != null) onExpireEvent.Invoke();

        // AutoDestroy
        if (destroyTime >= 0) { 
            Destroy(gameObject, destroyTime);
            if (onExpireFxParent != null) Destroy(onExpireFxParent.gameObject, destroyTime);
        }
    }
}
