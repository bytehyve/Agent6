using UnityEngine;

/// <summary>
/// Makes screen black at random intervals.
/// </summary>
public class RandomBlinkControl : MonoBehaviour
{
    [SerializeField] private GameObject imageParent;
    [SerializeField] private float intervalLength;
    [SerializeField] private float intervalDuration;

    private float cooldown;
    private float duration;

    public static bool IsActive;

    private void Awake() {
        IsActive = false;
        cooldown = intervalLength;
    }
    private void FixedUpdate() {
        if (duration > 0) {
            duration -= Time.deltaTime;
        }
        if (cooldown < Time.time) {
            ActivateRandomized(intervalDuration);
        }
        IsActive = duration > 0;
    }
    private void Update() {
        imageParent.SetActive(IsActive && !GameManager.Paused);
    }
    
    public void Activate(float time) {
        duration = time;
        cooldown = Time.time + intervalLength + duration;
    }
    public void ActivateRandomized(float time) {
        duration = time * Random.Range(0.5f, 1.5f);
        cooldown = Time.time + intervalLength * Random.Range(0.5f, 2.5f) + duration;
    }
    public void Deactivate() {
        duration = 0;
    }
}
