// _Project/Scripts/Managers/GameManager.cs (修正异步流程后的完整版)

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
    public SoundFeedbackManager soundFeedbackManager;
    public PauseManager pauseManager;
    public NotePoolManager notePoolManager;

    [Header("测试关卡配置")]
    public AudioClip testSong;
    public string testChartFileName = "Sample_SuperSimple.json"; // 确保文件名正确

    private bool isGameOver = false;

    /// <summary>
    /// 在对象创建时立即执行，非常适合订阅事件。
    /// </summary>
    void Awake()
    {
        StatsManager.OnGameOver += HandleGameOver;
    }

    /// <summary>
    /// 在对象销毁时执行，用于取消订阅，防止内存泄漏。
    /// </summary>
    void OnDestroy()
    {
        StatsManager.OnGameOver -= HandleGameOver;

        // 确保 chartLoader 存在时才取消订阅
        if (chartLoader != null)
        {
            chartLoader.OnChartLoadComplete -= HandleChartLoaded;
        }
    }

    /// <summary>
    /// 在第一帧更新前执行，用于初始化和启动游戏流程。
    /// </summary>
    void Start()
    {
        isGameOver = false;

        // 1. 初始化所有不依赖于谱面数据的管理器
        if (statsManager != null) statsManager.Initialize();
        if (skillManager != null) skillManager.Initialize();
        if (soundFeedbackManager != null) soundFeedbackManager.Initialize();
        if (judgmentManager != null) judgmentManager.Initialize();
        // PauseManager 和 NotePoolManager 的 Start/Awake 会自动初始化，无需手动调用

        // --- 异步加载流程 ---

        // 2. 订阅 ChartLoader 的加载完成事件
        //    确保 chartLoader 引用已在Inspector中设置
        if (chartLoader != null)
        {
            chartLoader.OnChartLoadComplete += HandleChartLoaded;

            // 3. 开始异步加载谱面，之后就等待事件回调
            chartLoader.LoadChart(testChartFileName);
        }
        else
        {
            Debug.LogError("GameManager 中的 ChartLoader 引用没有设置！请在Inspector中拖拽赋值。", this.gameObject);
        }
    }

    /// <summary>
    /// 当且仅当 ChartLoader 成功加载并转换完谱面后，此方法才会被事件回调调用。
    /// </summary>
    private void HandleChartLoaded()
    {
        Debug.Log("GameManager 收到谱面加载完成信号，正在启动游戏核心玩法...");

        // 在这里执行所有依赖于谱面数据的后续操作
        if (noteSpawner != null)
        {
            noteSpawner.StartSpawning();
        }
        else
        {
            Debug.LogError("GameManager 中的 NoteSpawner 引用没有设置！", this.gameObject);
        }

        if (timingManager != null && chartLoader.CurrentChart != null)
        {
            float bpm = chartLoader.CurrentChart.bpm;
            timingManager.PlaySong(testSong, bpm);
        }
        else
        {
            Debug.LogError("GameManager 中的 TimingManager 或 CurrentChart 为空！", this.gameObject);
        }
    }

    /// <summary>
    /// 当 StatsManager 广播游戏结束事件时，此方法被调用。
    /// </summary>
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

        // 【TODO】未来在这里添加弹出结算界面等逻辑
        // UIManager.Instance.ShowGameOverScreen();
    }
}