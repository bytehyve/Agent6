using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays random sounds around the parented object.
/// </summary>
public class RandomAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float timeBetween = 12f;
    [SerializeField] private float volume;
    [SerializeField] private bool isEnabled;

    private float timer;
    private float waitMoveTimer;

    private void Start() {
        SetTimer();
    }

    private void FixedUpdate() {
        if (!isEnabled) return;

        if (waitMoveTimer < Time.time) audioSource.transform.localPosition = Random.insideUnitSphere * 10;

        if (timer < Time.time) {
            waitMoveTimer = Time.time + timeBetween / 2f;
            SetTimer();
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.volume = Random.Range(0.9f, 1.1f) * volume;
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
    private void SetTimer() {
        timer = Time.time + Random.Range(0.8f, 1.5f) * timeBetween;
    }
}
