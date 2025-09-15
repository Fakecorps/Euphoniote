// _Project/Scripts/Managers/GameManager.cs (最终版 - 提供场景引用)

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

    [Header("场景特定引用")]
    [Tooltip("在4_Gameplay场景中，拖入技能特效的生成点")]
    public Transform skillEffectSpawnPoint;

    // --- 新增一个公共静态属性，供 FeedbackManager 查询 ---
    public static Transform SkillEffectSpawnPoint
    {
        get
        {
            // 在多场景架构中，最稳妥的方式是查找当前场景中的 GameManager 实例
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                return gm.skillEffectSpawnPoint;
            }
            // 如果找不到，返回 null，调用方需要处理这种情况
            return null;
        }
    }

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

        if (noteSpawner.AllNotesSpawned && TimingManager.Instance.SongPosition >= noteSpawner.GameEndTime)
        {
            OnSongFinished();
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