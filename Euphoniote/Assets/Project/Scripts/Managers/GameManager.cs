using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ChartLoader chartLoader;
    public TimingManager timingManager;
    public NoteSpawner noteSpawner;

    public AudioClip testSong; // 拖入一首测试音乐
    public string testChartFileName = "Sample.json";

    public StatsManager statsManager;
    public JudgmentManager judgmentManager;

    void Start()
    {
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
    }
}
