// _Project/Scripts/Managers/ChartLoader.cs (重构最终版)

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
            BeatmapData beatmap = JsonConvert.DeserializeObject<BeatmapData>(json);
            if (beatmap == null)
            {
                Debug.LogError($"无法解析谱面文件: {chartFileName}", this.gameObject);
                return;
            }

            CurrentChart = ConvertBeatmapToChart(beatmap);

            Debug.Log($"谱面 '{CurrentChart.songName}' 加载并转换成功，包含 {CurrentChart.notes.Count} 个音符。");
        }
        else
        {
            Debug.LogError($"找不到谱面文件: {path}", this.gameObject);
        }
    }

    private ChartData ConvertBeatmapToChart(BeatmapData beatmap)
    {
        ChartData chart = new ChartData();
        chart.songName = beatmap.songName;
        chart.bpm = beatmap.bpm;
        chart.notes = new System.Collections.Generic.List<NoteData>();

        float secPerBeat = 60f / beatmap.bpm;

        foreach (var beatNote in beatmap.notes)
        {
            NoteData note = new NoteData();

            // --- 【核心转换逻辑重构】 ---

            // 1. 计算音符的开始总拍数 (startTotalBeat)
            float startTotalBeat = (beatNote.beat - 1) + ((float)beatNote.subdivision / beatmap.ticksPerBeat);

            // 2. 根据开始总拍数，计算开始时间 time (秒)
            note.time = beatmap.offset + startTotalBeat * secPerBeat;

            // 3. 检查这是否是一个HoldNote
            if (beatNote.endBeat > 0) // 我们约定endBeat > 0 表示是HoldNote
            {
                // 4. 计算音符的结束总拍数 (endTotalBeat)
                float endTotalBeat = (beatNote.endBeat - 1) + ((float)beatNote.endSubdivision / beatmap.ticksPerBeat);

                // 5. 根据结束总拍数，计算结束时间 endTime (秒)
                float endTime = beatmap.offset + endTotalBeat * secPerBeat;

                // 6. duration = 结束时间 - 开始时间
                note.duration = endTime - note.time;
            }
            else
            {
                // 如果是TapNote，duration为0
                note.duration = 0;
            }

            // --- 转换结束 ---

            // 复制其他数据
            note.requiredFrets = beatNote.requiredFrets;
            note.strumType = beatNote.strumType;
            note.isSpecial = beatNote.isSpecial;

            chart.notes.Add(note);
        }

        return chart;
    }
}