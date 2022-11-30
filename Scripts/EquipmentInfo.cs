using UnityEngine;

/// <summary>
/// Base class for equipment data.
/// </summary>
[CreateAssetMenu()]
public class EquipmentInfo : ScriptableObject
{
    [Header("Base")]
    public GameObject prefab;
    public string ingameName;

    [HideInInspector]
    public GameObject ingamePrefab;
    [HideInInspector]
    public EquipmentAnimator equipmentAnimator;

    public virtual void Initialize(GameObject ingamePrefab) {
        this.ingamePrefab = ingamePrefab;
        equipmentAnimator = ingamePrefab.GetComponent<EquipmentAnimator>();
        if (ingamePrefab.transform.Find("Hands") != null) ingamePrefab.transform.Find("Hands").gameObject.SetActive(true);
    }
}
