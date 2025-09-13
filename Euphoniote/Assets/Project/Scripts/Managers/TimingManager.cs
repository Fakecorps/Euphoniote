// _Project/Scripts/Managers/TimingManager.cs (最终简化版 - 纯计时)

using UnityEngine;

public class TimingManager : MonoBehaviour
{
    public static TimingManager Instance { get; private set; }

    [Header("组件引用")]
    public AudioSource musicSource;

    [Header("歌曲信息")]
    public float bpm = 120f;

    // --- 核心属性 ---
    public float SongPosition => (musicSource != null) ? musicSource.time : 0f;
    public float SongPositionInBeats { get; private set; }

    // --- 内部变量 ---
    private float secPerBeat;
    private float lastBeat = 0f;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        // 在暂停或倒计时期间，这个 Update 实际上没有影响，因为 SongPosition 不会变
        if (PauseManager.IsPaused || PauseManager.IsCountingDown)
        {
            return;
        }

        // 只需要在音乐播放时计算节拍即可
        if (musicSource != null && musicSource.isPlaying)
        {
            SongPositionInBeats = SongPosition / secPerBeat;
        }
    }

    /// <summary>
    /// 播放歌曲并设置BPM。
    /// </summary>
    public void PlaySong(AudioClip clip, float chartBpm)
    {
        if (musicSource == null)
        {
            Debug.LogError("TimingManager 的 musicSource 没有被赋值！", this.gameObject);
            return;
        }

        musicSource.clip = clip;
        this.bpm = chartBpm;

        secPerBeat = 60f / bpm;

        // 重置状态
        SongPositionInBeats = 0;
        lastBeat = 0;

        musicSource.Play();
    }

    public int GetPassedBeats()
    {
        if (musicSource == null || !musicSource.isPlaying) return 0;

        int currentBeatInt = Mathf.FloorToInt(SongPositionInBeats);
        int lastBeatInt = Mathf.FloorToInt(lastBeat);

        if (currentBeatInt > lastBeatInt)
        {
            int passedBeats = currentBeatInt - lastBeatInt;
            lastBeat = SongPositionInBeats;
            return passedBeats;
        }
        return 0;
    }

    public void ResetBeatTracking()
    {
        if (musicSource == null || !musicSource.isPlaying) return;
        lastBeat = SongPositionInBeats;
    }
}