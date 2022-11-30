using UnityEngine;

/// <summary>
/// Derives from <see cref=" EquipmentAnimator"/>. Animates a PT8 via code.
/// </summary>
public class PT8Animator : EquipmentAnimator
{
    [SerializeField] private Transform topPart;
    private float posx;

    void Start() {
        posx = 0;
    }
    void FixedUpdate() {
        posx = Mathf.Lerp(posx, 0, 6f * Time.deltaTime);
        topPart.localPosition = new Vector3(posx, 0, 0);
    }

    public override void OnActionFire() {
        base.OnActionFire();
        posx = -0.2f;
    }
}
