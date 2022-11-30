using UnityEngine;

/// <summary>
/// Derives from <see cref="Explodable"/>. Will move after explosion.
/// </summary>
public class ExplodableProp : Explodable
{
    [Header("Prop")]
    [SerializeField] private Transform groundReference;
    [SerializeField] private float explodeJumpForce;

    private float force = 0;

    private void FixedUpdate() {
        if (IsExpired && explodeJumpForce != 0) Move();
    }

    private void Move() {
        force -= 9.81f * Time.deltaTime;

        if (force < 0 && Physics.Raycast(groundReference.position, Vector3.down, Mathf.Abs(force) * 1.1f * Time.deltaTime, WorldManager.Instance.settings.collideMask)) force = 0;
        transform.position = transform.position + Vector3.up * force * Time.deltaTime;
    }

    protected override void OnExpire() {
        base.OnExpire();

        if (explodeJumpForce > 0) {
            force = explodeJumpForce * Random.Range(0.8f, 1.2f);
        }
    }
}
