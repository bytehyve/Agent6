using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Derives from <see cref="Explodable"/>. Will expire on impact.
/// </summary>
public class ExplodableGrenade : Explodable
{
    [SerializeField] private float force;
    [SerializeField] private float gravity;

    [HideInInspector] public Vector3 direction;

    protected void FixedUpdate() {
        if (Physics.Raycast(transform.position, direction, force, WorldManager.Instance.settings.collideMask)) {
            force = 0;
            OnHit(999);
        }

        transform.position += direction * force;
        direction += Vector3.down * gravity;
        direction = direction.normalized;
    }
}
