// _Project/Scripts/Managers/GameManager.cs (多场景最终版 - 从LevelData加载)

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
    /// 当玩家血量耗尽时，由 StatsManager 的 OnGameOver 事件调用
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

        // 打包数据，标记为“失败”
        statsManager.FinalizeResults(false);

        // 通知流程管理器，游戏已结束
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GameplayFinished();
        }
    }

    /// <summary>
    /// 当歌曲正常播放完毕时，应该被调用。
    /// 你需要从 TimingManager 中调用此方法。
    /// </summary>
    public void OnSongFinished()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("歌曲播放完毕（成功），正在执行结束流程...");

        // 停止 note 生成等
        if (noteSpawner != null)
        {
            noteSpawner.enabled = false;
        }

        // 打包数据，标记为“成功”
        statsManager.FinalizeResults(true);

        // 通知流程管理器，游戏已结束
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GameplayFinished();
        }
    }
}