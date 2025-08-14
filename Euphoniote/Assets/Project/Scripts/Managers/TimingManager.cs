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
        if (clip == null)
        {
            Debug.LogError("PlaySong被调用，但是传入的AudioClip是空的！请检查GameManager的'Test Song'字段。", this.gameObject);
            return;
        }

        if (musicSource == null)
        {
            Debug.LogError("AudioSource是空的！请检查TimingManager的'Music Source'字段。", this.gameObject);
            return;
        }

        Debug.Log($"准备播放音乐: {clip.name}", this.gameObject);
        musicSource.clip = clip;
        musicSource.Play();
        if (musicSource.isPlaying)
        {
            Debug.Log($"<color=green>音乐 '{clip.name}' 已成功开始播放！</color>");
        }
        else
        {
            Debug.LogWarning($"调用了Play()，但AudioSource.isPlaying仍然是false。请检查音量和Mute Audio按钮。");
        }
    }
}
