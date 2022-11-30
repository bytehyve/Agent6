using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the tone of the music.
/// </summary>
public class MusicManager : MonoBehaviour
{
    const float ACTIONDURATION = 5f;

    [SerializeField] private AudioSource musicCalmPlayer;
    [SerializeField] private AudioSource musicActionPlayer;
    [SerializeField] private float volumeChangeRate;

    static float actionTimer;

    void Start() {
        actionTimer = 0;
        musicCalmPlayer.volume = 1;
        musicActionPlayer.volume = 0;
    }

    private void FixedUpdate() {
        actionTimer -= Time.deltaTime;

        if (actionTimer > 0)
        {
            musicActionPlayer.volume += volumeChangeRate * Time.deltaTime;
        }
        else musicActionPlayer.volume -= volumeChangeRate * Time.deltaTime;
        musicActionPlayer.volume = Mathf.Clamp01(musicActionPlayer.volume);
    }

    public static void OnAction() {
        actionTimer = ACTIONDURATION;
    }
}
