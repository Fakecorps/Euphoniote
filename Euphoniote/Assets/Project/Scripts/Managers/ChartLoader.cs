using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ChartLoader : MonoBehaviour
{
    public ChartData CurrentChart { get; private set; }

    public void LoadChart(string chartFileName)
    {

        string path = Path.Combine(Application.streamingAssetsPath, "Charts", chartFileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            CurrentChart = JsonUtility.FromJson<ChartData>(json);
            Debug.Log($"谱面 '{CurrentChart.songName}' 加载成功，包含 {CurrentChart.notes.Count} 个音符。");
        }
        else
        {
            Debug.LogError($"找不到谱面文件: {path}");
        }
    }
}
