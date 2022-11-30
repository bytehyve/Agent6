using UnityEngine;

// This class can be removed when storing the objectives somewhere else!

/// <summary>
/// Data class for scene settings. 
/// </summary>
[CreateAssetMenu(fileName = "Scene Settings", menuName = "Scene Settings")]
public class SceneSettings : ScriptableObject
{
    public Objective[] objectives;

    [System.Serializable]
    public class Objective
    {
        public string description;
    }
}
