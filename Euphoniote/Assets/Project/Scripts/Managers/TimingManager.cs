// _Project/Scripts/Managers/TimingManager.cs (增强版)

using UnityEngine;

public class TimingManager : MonoBehaviour
{
    public static TimingManager Instance { get; private set; }

    [Header("组件引用")]
    public AudioSource musicSource;

    [Header("歌曲信息")]
    [Tooltip("当前歌曲的BPM (每分钟节拍数)")]
    public float bpm = 120f; // 默认值，应该由ChartLoader在加载谱面时设置

    // --- 核心属性 ---
    /// <summary>
    /// 获取当前歌曲的播放时间（秒）。
    /// </summary>
    public float SongPosition => musicSource.time;

    /// <summary>
    /// 获取当前歌曲进行到了第几拍。这是一个浮点数，可以精确到小数拍。
    /// </summary>
    public float SongPositionInBeats { get; private set; }

    // --- 内部变量 ---
    private float secPerBeat; // 每一拍持续多少秒
    private float lastBeat = 0f; // 上一次触发节拍combo的拍数

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (musicSource.isPlaying)
        {
            // 持续计算当前在第几拍
            SongPositionInBeats = SongPosition / secPerBeat;
        }
    }

    /// <summary>
    /// 播放歌曲并设置BPM。
    /// </summary>
    public void PlaySong(AudioClip clip, float chartBpm)
    {
        musicSource.clip = clip;
        this.bpm = chartBpm;

        // 计算每一拍的秒数
        secPerBeat = 60f / bpm;

        // 重置状态
        SongPositionInBeats = 0;
        lastBeat = 0;

        musicSource.Play();
    }

    /// <summary>
    /// 【新增】检查并获取自上次调用以来经过了多少个整数节拍。
    /// 用于在HoldNote期间增加Combo。
    /// </summary>
    /// <returns>经过的节拍数</returns>
    public int GetPassedBeats()
    {
        int currentBeatInt = Mathf.FloorToInt(SongPositionInBeats);
        int lastBeatInt = Mathf.FloorToInt(lastBeat);

        if (currentBeatInt > lastBeatInt)
        {
            int passedBeats = currentBeatInt - lastBeatInt;
            lastBeat = SongPositionInBeats; // 更新上一次的拍数
            return passedBeats;
        }

        return 0; // 没有经过新的整数拍
    }

    public void ResetBeatTracking()
    {
        // 将 lastBeat 更新为当前的精确拍数
        lastBeat = SongPositionInBeats;
    }
}