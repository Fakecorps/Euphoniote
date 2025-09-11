// _Project/Scripts/Managers/TimingManager.cs (最终修改版 - 增加结束检测)

using UnityEngine;

public class TimingManager : MonoBehaviour
{
    public static TimingManager Instance { get; private set; }

    [Header("组件引用")]
    public AudioSource musicSource;
    public GameManager gameManager; // <<-- 新增：对 GameManager 的引用

    [Header("歌曲信息")]
    [Tooltip("当前歌曲的BPM (每分钟节拍数)")]
    public float bpm = 120f;

    // --- 核心属性 ---
    public float SongPosition => musicSource.time;
    public float SongPositionInBeats { get; private set; }

    // --- 内部变量 ---
    private float secPerBeat;
    private float lastBeat = 0f;
    private bool songHasStarted = false; // <<-- 新增：一个状态标志，防止在歌曲开始前就误判结束

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (!songHasStarted) return; // 如果歌曲还没开始播放，就什么都不做

        if (musicSource.isPlaying)
        {
            // 持续计算当前在第几拍
            SongPositionInBeats = SongPosition / secPerBeat;
        }
        else
        {
            // --- 核心修改点：检测歌曲是否结束 ---
            // 如果 songHasStarted 为 true，但 isPlaying 变为 false，
            // 这就意味着歌曲刚刚播放完毕。

            // 确保只调用一次
            songHasStarted = false;

            if (gameManager != null)
            {
                // 通知 GameManager 歌曲已结束
                gameManager.OnSongFinished();
            }
            else
            {
                Debug.LogError("TimingManager 中的 GameManager 引用没有设置！无法报告歌曲结束。", this.gameObject);
            }
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

        SongPositionInBeats = 0;
        lastBeat = 0;

        musicSource.Play();
        songHasStarted = true; // <<-- 在这里标记歌曲已开始播放
    }

    public int GetPassedBeats()
    {
        if (!songHasStarted) return 0;

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
        // 如果音乐还没开始，就不要重置 beat tracking，防止出错
        if (!songHasStarted) return;
        lastBeat = SongPositionInBeats;
    }
}