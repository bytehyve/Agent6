using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animates the weapon of AI soldiers.
/// </summary>
public class AIWeaponAnimator : MonoBehaviour
{
    [HideInInspector]
    public float moveInput;
    [HideInInspector]
    public float aimInput;
    [HideInInspector]
    public bool isAiming;

    [SerializeField] private Vector3 rotation_idle;
    [SerializeField] private Vector3 rotation_walking;
    [SerializeField] private Vector3 rotation_standingAim;
    [SerializeField] private Vector3 rotation_crouchAim;

    private void FixedUpdate() {
        Quaternion targetRotation = !isAiming
            ? Quaternion.Slerp(Quaternion.Euler(rotation_idle), Quaternion.Euler(rotation_walking), moveInput)
            : Quaternion.Slerp(Quaternion.Euler(rotation_standingAim), Quaternion.Euler(rotation_crouchAim), aimInput);

        transform.localRotation= Quaternion.Slerp(transform.localRotation, targetRotation, 20f * Time.deltaTime);
    }
}
