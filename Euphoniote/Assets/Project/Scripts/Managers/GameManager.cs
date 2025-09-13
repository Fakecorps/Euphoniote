// _Project/Scripts/Managers/GameManager.cs (最终重构版 - 独立的结束逻辑)

using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("核心管理器引用")]
    public ChartLoader chartLoader;
    public TimingManager timingManager;
    public NoteSpawner noteSpawner;
    public StatsManager statsManager;
    public JudgmentManager judgmentManager;
    public SkillManager skillManager;
    public FeedbackManager feedbackManager;
    public SoundFeedbackManager soundFeedbackManager;
    public PauseManager pauseManager;
    public NotePoolManager notePoolManager;

    private bool isGameOver = false;

    void Awake()
    {
        StatsManager.OnGameOver += HandleGameOver;
    }

    void OnDestroy()
    {
        StatsManager.OnGameOver -= HandleGameOver;
        if (chartLoader != null)
        {
            chartLoader.OnChartLoadComplete -= HandleChartLoaded;
        }
    }

    void Start()
    {
        isGameOver = false;
        InitializeAllManagers();

        LevelData currentLevel = GameFlowManager.CurrentLevelData;
        if (currentLevel != null)
        {
            Debug.Log($"正在准备加载关卡: {currentLevel.name}");
            chartLoader.OnChartLoadComplete += HandleChartLoaded;
            chartLoader.LoadChart(currentLevel.chartFileName);
        }
        else
        {
            Debug.LogError("无法开始游戏：GameFlowManager 中没有找到当前关卡数据！请确认您是从选关界面进入的。", this.gameObject);
        }
    }

    void Update()
    {
        if (isGameOver || PauseManager.IsPaused || PauseManager.IsCountingDown)
        {
            return;
        }

        if (noteSpawner == null || TimingManager.Instance == null) return;

        // 新的游戏成功结束条件
        if (noteSpawner.AllNotesSpawned && TimingManager.Instance.SongPosition >= noteSpawner.GameEndTime)
        {
            OnSongFinished();
        }

        if (noteSpawner.AllNotesSpawned && !TimingManager.Instance.musicSource.isPlaying)
        {
            // 我们需要一个额外的检查，确保不是在游戏刚开始音乐还没播放时就触发
            // 检查 SongPosition 是否大于0，可以作为一个简单的启动判断
            if (TimingManager.Instance.SongPosition > 0)
            {
                Debug.LogWarning("音乐提前结束，但所有音符已生成。强制判定为游戏成功。");
                OnSongFinished();
                return;

            }
        }
    }

    private void InitializeAllManagers()
    {
        if (statsManager != null) statsManager.Initialize();
        if (skillManager != null) skillManager.Initialize();
        if (feedbackManager != null) feedbackManager.Initialize();
        if (soundFeedbackManager != null) soundFeedbackManager.Initialize();
        if (judgmentManager != null) judgmentManager.Initialize();
    }

    private void HandleChartLoaded()
    {
        Debug.Log("GameManager 收到谱面加载完成信号，正在启动游戏核心玩法...");

        LevelData currentLevel = GameFlowManager.CurrentLevelData;

        if (noteSpawner != null)
        {
            noteSpawner.StartSpawning();
        }

        if (timingManager != null && chartLoader.CurrentChart != null && currentLevel.musicClip != null)
        {
            float bpm = chartLoader.CurrentChart.bpm;
            timingManager.PlaySong(currentLevel.musicClip, bpm);
        }
        else
        {
            Debug.LogError("TimingManager, CurrentChart 或 LevelData.musicClip 为空，无法播放音乐！", this.gameObject);
        }
    }

    /// <summary>
    /// 当玩家血量耗尽时，由 StatsManager 的 OnGameOver 事件调用 (失败路径)
    /// </summary>
    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("GameManager 收到了游戏结束信号（失败），正在执行结束流程...");

        if (timingManager != null && timingManager.musicSource != null)
        {
            timingManager.musicSource.Stop();
        }
        if (noteSpawner != null)
        {
            noteSpawner.enabled = false;
        }

        statsManager.FinalizeResults(false);

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GameplayFinished();
        }
    }

    /// <summary>
    /// 当 Update 检测到游戏成功完成时调用 (成功路径)
    /// </summary>
    public void OnSongFinished()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("歌曲内容播放完毕（成功），正在执行结束流程...");

        if (noteSpawner != null)
        {
            noteSpawner.enabled = false;
        }

        statsManager.FinalizeResults(true);

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GameplayFinished();
        }
    }
}