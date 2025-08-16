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
        statsManager.Initialize(); // 假设你已在GameManager中引用了StatsManager

        // 2. 判定系统后准备，这样它广播事件时，数值系统肯定已经在监听了
        judgmentManager.Initialize(); // 假设你已在GameManager中引用了JudgmentManager

        // 3. 开始游戏流程
        chartLoader.LoadChart(testChartFileName);
        noteSpawner.StartSpawning();

        float bpm = chartLoader.CurrentChart.bpm;
        timingManager.PlaySong(testSong, bpm);
    }
}
