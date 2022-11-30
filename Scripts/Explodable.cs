using UnityEngine;
using JaimesUtilities;

/// <summary>
/// Derives from <see cref="Damageable"/>. Will hit all <see cref="Hitbox"/> in range on expire.
/// </summary>
public class Explodable : Damageable
{
    [Header("Explosion")]
    public int damage;
    public float radius;
    public bool ignoreVerticalDamage;

    protected override void OnExpire() {
        base.OnExpire();
        Vector3 thisPosition = transform.position;

        // Get other hitboxes
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Hittable")) {
            if (ignoreVerticalDamage) { 
                if ((g.transform.position - thisPosition).Horizontal().magnitude < radius) {
                    g.GetComponent<Hitbox>().OnHit(damage, true);
                }
            }
            else {
                if ((g.transform.position - thisPosition).magnitude < radius) {
                    g.GetComponent<Hitbox>().OnHit(damage, true);
                }
            }
        }

        // Get Player
        if (ignoreVerticalDamage) {
            if ((PlayerController.instance.transform.position - thisPosition).Horizontal().magnitude < radius) PlayerController.instance.OnDamage(transform.position, damage / 5);
        }
        else if ((PlayerController.instance.transform.position - thisPosition).magnitude < radius) PlayerController.instance.OnDamage(transform.position, damage / 5);
    }
}
