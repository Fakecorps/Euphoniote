using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingManager : MonoBehaviour
{
    public static TimingManager Instance { get; private set; }

    public AudioSource musicSource;
    public float SongPosition => musicSource.time; // 公开属性，返回当前音乐播放时间

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void PlaySong(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
}
