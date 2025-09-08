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

    // --- 我们不再需要这些测试字段了 ---
    // public AudioClip testSong;
    // public string testChartFileName;

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

        // --- 核心修改：从 GameFlowManager 获取当前关卡数据 ---
        LevelData currentLevel = GameFlowManager.CurrentLevelData;
        if (currentLevel != null)
        {
            Debug.Log($"正在准备加载关卡: {currentLevel.name}");
            // 订阅加载完成事件
            chartLoader.OnChartLoadComplete += HandleChartLoaded;
            // 使用 LevelData 中的谱面文件名开始加载
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

        // --- 核心修改：使用 LevelData 中的音乐文件 ---
        LevelData currentLevel = GameFlowManager.CurrentLevelData;

        if (noteSpawner != null)
        {
            noteSpawner.StartSpawning();
        }

        if (timingManager != null && chartLoader.CurrentChart != null && currentLevel.musicClip != null)
        {
            float bpm = chartLoader.CurrentChart.bpm;
            // 使用 LevelData 中配置的音乐来播放
            timingManager.PlaySong(currentLevel.musicClip, bpm);
        }
        else
        {
            Debug.LogError("TimingManager, CurrentChart 或 LevelData.musicClip 为空，无法播放音乐！", this.gameObject);
        }
    }

    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("GameManager 收到了游戏结束信号，正在执行结束流程...");

        if (timingManager != null && timingManager.musicSource != null)
        {
            timingManager.musicSource.Stop();
        }
        if (noteSpawner != null)
        {
            noteSpawner.enabled = false;
        }

        // 游戏结束后，通知 GameFlowManager
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GameplayFinished();
        }
    }
}