using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ChartLoader chartLoader;
    public TimingManager timingManager;
    public NoteSpawner noteSpawner;
    public StatsManager statsManager;
    public JudgmentManager judgmentManager;
    public SkillManager skillManager;
    public NotePoolManager notePoolManager;

    public AudioClip testSong; // 拖入一首测试音乐
    public string testChartFileName = "Sample.json";

    private bool isGameOver = false;

    void Awake()
    {
        // 在Awake中订阅事件，确保能在第一时间收到
        StatsManager.OnGameOver += HandleGameOver;
    }

    void OnDestroy()
    {
        // 在对象销毁时取消订阅
        StatsManager.OnGameOver -= HandleGameOver;
    }
    void Start()
    {
        isGameOver = false;
        // 1. 确保监听者(StatsManager, UIManager等)先初始化并订阅事件
        if (statsManager != null) statsManager.Initialize();
        // if (uiManager != null) uiManager.Initialize();

        // 2. 确保事件广播者(JudgmentManager)后初始化
        if (judgmentManager != null) judgmentManager.Initialize();

        // 3. 开始游戏流程
        chartLoader.LoadChart(testChartFileName);
        noteSpawner.StartSpawning();

        float bpm = chartLoader.CurrentChart.bpm;
        timingManager.PlaySong(testSong, bpm);

        skillManager.Initialize();
    }

    private void HandleGameOver()
    {
        // 防止重复执行
        if (isGameOver) return;
        isGameOver = true;

        // 在这里执行强制结束游戏的逻辑
        Debug.Log("GameManager 收到了游戏结束信号，正在执行结束流程...");

        // 【TODO】后续在这里添加你的具体逻辑
        // 1. 停止音乐
        if (timingManager != null)
        {
            timingManager.musicSource.Stop();
        }

        // 2. 停止生成音符
        if (noteSpawner != null)
        {
            noteSpawner.enabled = false;
        }

        // 3. 弹出失败UI / 结算界面
        // UIManager.Instance.ShowGameOverScreen();

        // 4. 切换到结算场景
        // SceneManager.LoadScene("ResultsScene");
    }
}
