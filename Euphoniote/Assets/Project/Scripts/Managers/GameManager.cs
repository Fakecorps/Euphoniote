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

    void Start()
    {
        // 游戏开始时的流程
        // 1. 加载谱面
        chartLoader.LoadChart(testChartFileName);

        // 2. 告诉NoteSpawner准备生成
        noteSpawner.StartSpawning();

        // 3. 播放音乐
        timingManager.PlaySong(testSong);
    }
}
