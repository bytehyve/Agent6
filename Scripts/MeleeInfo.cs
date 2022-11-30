using UnityEngine;

/// <summary>
/// Derives from <see cref="EquipmentInfo"/>. Data class for melee equipments.
/// </summary>
[CreateAssetMenu()]
public class MeleeInfo : EquipmentInfo
{
    [Header("Stats")]
    [Range(0, 100)]
    public int damage = 10;
    [Range(1, 60)]
    public int speed = 1;

    public override void Initialize(GameObject ingamePrefab) {
        base.Initialize(ingamePrefab);
    }
}
