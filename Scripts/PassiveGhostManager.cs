using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages the spawning of <see cref="AIGhostPassive"/>.
/// </summary>
public class PassiveGhostManager : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private float activeTime = 0.1f;

    public static bool GhostActive;
    public static List<AIGhostPassive> GhostList;
    
    private float cooldownTimer;

    private void Awake() {
        cooldownTimer = cooldownTime;
        GhostActive = false;
        GhostList = new List<AIGhostPassive>(GetComponentsInChildren<AIGhostPassive>());
    }

    private void FixedUpdate() {
        if (!GhostActive && cooldownTimer < Time.time) {
            List<AIGhostPassive> openSet = GhostList.Where(x => x.CanAppear()).ToList();

            if (openSet.Count > 0) {
                openSet[Random.Range(0, openSet.Count)].SetPending(activeTime * Random.Range(0.9f, 1.1f));
                cooldownTimer = Time.time + cooldownTime * Random.Range(0.8f, 1.2f) + activeTime;
            }
            else cooldownTimer = Time.time + cooldownTime * Random.Range(0.2f, 0.5f) + activeTime;
        }
    }
}
