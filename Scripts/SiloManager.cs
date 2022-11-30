using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Exclusive script for level 2 and 4.
/// </summary>
public class SiloManager : MonoBehaviour
{
    public int siloCount;
    public UnityEvent onDestroyAll;

    public void OnSiloDestroy() {
        siloCount--;

        if (siloCount <= 0) {
            onDestroyAll.Invoke();
        }
    }
}
