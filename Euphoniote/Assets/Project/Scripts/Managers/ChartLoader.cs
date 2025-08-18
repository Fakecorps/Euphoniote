// _Project/Scripts/Managers/ChartLoader.cs (升级版)

using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class ChartLoader : MonoBehaviour
{
    // 对外暴露的依然是基于秒的 ChartData
    public ChartData CurrentChart { get; private set; }

    public void LoadChart(string chartFileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Charts", chartFileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // 1. 将JSON反序列化为临时的、基于节拍的 BeatmapData
            BeatmapData beatmap = JsonConvert.DeserializeObject<BeatmapData>(json);
            if (beatmap == null)
            {
                Debug.LogError($"无法解析谱面文件: {chartFileName}", this.gameObject);
                return;
            }

            // 2. 执行转换逻辑
            CurrentChart = ConvertBeatmapToChart(beatmap);

            Debug.Log($"谱面 '{CurrentChart.songName}' 加载并转换成功，包含 {CurrentChart.notes.Count} 个音符。");
        }
        else
        {
            Debug.LogError($"找不到谱面文件: {path}", this.gameObject);
        }
    }

    /// <summary>
    /// 核心转换函数：将基于节拍的谱面数据转换为游戏使用的、基于秒的数据。
    /// </summary>
    private ChartData ConvertBeatmapToChart(BeatmapData beatmap)
    {
        // 创建一个新的 ChartData 实例来存放转换后的结果
        ChartData chart = new ChartData();
        chart.songName = beatmap.songName;
        chart.bpm = beatmap.bpm;
        chart.notes = new System.Collections.Generic.List<NoteData>();

        // 计算转换所需的基本参数
        float secPerBeat = 60f / beatmap.bpm;

        // 遍历所有基于节拍的音符
        foreach (var beatNote in beatmap.notes)
        {
            // 创建一个新的、基于秒的音符数据
            NoteData note = new NoteData();

            // --- 核心转换公式 ---
            // 1. 计算总拍数
            float totalBeat = (beatNote.beat - 1) + ((float)beatNote.subdivision / beatmap.ticksPerBeat);
            // 2. 转换为秒
            note.time = beatmap.offset + totalBeat * secPerBeat;
            // 3. 转换时长
            note.duration = beatNote.length * secPerBeat;
            // --- 转换结束 ---

            // 复制其他数据
            note.requiredFrets = beatNote.requiredFrets;
            note.strumType = beatNote.strumType;
            note.isSpecial = beatNote.isSpecial;

            // 将转换后的音符添加到最终的列表中
            chart.notes.Add(note);
        }

        return chart;
    }
}