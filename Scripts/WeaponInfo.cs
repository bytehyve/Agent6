using UnityEngine;

/// <summary>
/// Derives from <see cref="EquipmentInfo"/>. Data class for weapons.
/// </summary>
[CreateAssetMenu()]
public class WeaponInfo : EquipmentInfo
{
    [Header("Stats")]
    public bool automatic = false;
    [Range(0, 100)]
    public int damage = 10;
    [Range(1, 60)]
    public int fireRate = 1;
    [Range(0, 1f)]
    public float bloom = 0;
    public int clipSize = 10;
    [Range(1f, 4f)]
    public float zoomRate = 1;
    public bool isSilenced;
    public bool useAutoAim;
    public GameObject customEmitter;
    public Sprite ammoIcon;

    [HideInInspector]
    public int ammoClip;
    [HideInInspector]
    public int ammoStash;
    public Vector3 EndPoint => ingamePrefab == null ? Vector3.zero : ingamePrefab.transform.Find("EndPoint").position;
    public GameObject MuzzleFlash => ingamePrefab == null ? null : ingamePrefab.transform.Find("EndPoint").Find("MuzzleFlash").gameObject;

    public override void Initialize(GameObject ingamePrefab) {
        base.Initialize(ingamePrefab);
        ammoClip = clipSize;
        ammoStash = 0;
    }

    public void AddAmmo(int ammo) {
        ammoStash += ammo;
    }
    public void Reload() {
        ammoStash += ammoClip;
        ammoStash -= clipSize;
        ammoClip = Mathf.Min(clipSize, clipSize + ammoStash);
        if (ammoStash < 0) ammoStash = 0;
    }
}
