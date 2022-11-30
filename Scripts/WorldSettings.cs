using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Data class to store most used variables.
/// </summary>
[CreateAssetMenu()]
public class WorldSettings : ScriptableObject
{
    public float gravity = 0.75f;
    public LayerMask staticCollideMask;
    public LayerMask collideMask;
    public LayerMask targetHitMask;
    public LayerMask enemyMask;
    public LayerMask playerMask;
    public Material screenMaterial;
    public Material soldierMaterial;
    public Material ghostMaterial;
    public AudioMixer audioMixer;

    public SceneSettings[] sceneSettings;

}
