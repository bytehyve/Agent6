using UnityEngine;

/// <summary>
/// Controls the bullet Fx.
/// </summary>
public class BulletControl : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [HideInInspector] public Vector3 targetPosition;

    private float defaultTrailWidth;
    private float lifeTime;

    private void Start() {
        defaultTrailWidth = GetComponentInChildren<TrailRenderer>().widthMultiplier;
        lifeTime = 0;
    }

    private void FixedUpdate() {
        Vector3 vec = (targetPosition - transform.position);
        Vector3 dir = vec.normalized;
        float forc = Mathf.Min(vec.magnitude, moveSpeed * Time.deltaTime);
        transform.position += dir * forc;

        if (forc < 0.1f) Destroy(gameObject, 0.5f);

        GetComponentInChildren<TrailRenderer>().widthMultiplier = defaultTrailWidth / (1f + lifeTime);

        if (Physics.Raycast(transform.position, dir, forc, WorldManager.Instance.settings.staticCollideMask)) Destroy(gameObject, Time.deltaTime);

        lifeTime += Time.deltaTime;
    }
}
