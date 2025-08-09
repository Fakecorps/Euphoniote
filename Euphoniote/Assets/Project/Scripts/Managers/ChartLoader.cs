using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ChartLoader : MonoBehaviour
{
    public ChartData CurrentChart { get; private set; }

    public void LoadChart(string chartFileName)
    {
        // 注意：为了让这个方法工作，你需要把 _Project/Charts 文件夹整个移动到 Unity 项目根目录下的 StreamingAssets 文件夹内
        // 如果 StreamingAssets 文件夹不存在，请手动创建它。
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
