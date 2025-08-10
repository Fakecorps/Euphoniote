using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class ChartLoader : MonoBehaviour
{
    public ChartData CurrentChart { get; private set; }

    public void LoadChart(string chartFileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Charts", chartFileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            // 使用 Newtonsoft.Json 进行反序列化
            CurrentChart = JsonConvert.DeserializeObject<ChartData>(json);
            Debug.Log($"谱面 '{CurrentChart.songName}' 加载成功 (使用Newtonsoft)，包含 {CurrentChart.notes.Count} 个音符。");
        }
        else
        {
            Debug.LogError($"找不到谱面文件: {path}");
        }
    }
}
