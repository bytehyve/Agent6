using UnityEngine;

/// <summary>
/// When OnHit is called, a signal is send to <see cref="Damageable"/>.
/// </summary>
public class Hitbox : MonoBehaviour
{
    [SerializeField] private Damageable parent;
    [SerializeField] private float multiplier;
    [SerializeField] private bool doubleExplosiveDamage;

    public void OnHit(int damage, bool explosive = false) {
        parent.OnHit(Mathf.CeilToInt(damage * multiplier) * (doubleExplosiveDamage ? 2 : 1));
    }
}