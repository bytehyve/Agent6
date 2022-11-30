using UnityEngine;

/// <summary>
/// Derives from <see cref="EquipmentAnimator"/>. Animates a knife via code.
/// </summary>
public class KnifeAnimator : EquipmentAnimator
{
    [SerializeField] private Transform[] movingParts;
    
    private bool inAction;
    private float actionX;

    private void FixedUpdate() {
        if (inAction) {
            actionX += Time.deltaTime * 2f;
            if (actionX > 1) {
                actionX = 0;
                inAction = false;
            }
        }

        foreach (Transform mp in movingParts) {
            float perc = 1f - Mathf.Abs(Mathf.Sqrt(actionX) * 2f - 1f);

            mp.localPosition = new Vector3(0, Mathf.Pow(perc, 3f) / 15f, perc / 5f);
            mp.localRotation = Quaternion.Euler(perc * -20f, 0, 0);
        }
    }

    public override void OnActionFire() {
        base.OnActionFire();
        inAction = true;
        actionX = 0;
    }
}
