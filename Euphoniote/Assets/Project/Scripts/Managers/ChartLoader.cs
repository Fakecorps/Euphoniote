// _Project/Scripts/Managers/ChartLoader.cs (最终精简版)

using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO; // Path.Combine 需要

public class ChartLoader : MonoBehaviour
{
    public ChartData CurrentChart { get; private set; }
    public event System.Action OnChartLoadComplete;

    /// <summary>
    /// 公开的加载入口，启动协程来异步加载谱面。
    /// </summary>
    /// <param name="chartFileName">谱面文件名，例如 "Sample.json"</param>
    public void LoadChart(string chartFileName)
    {
        StartCoroutine(LoadChartCoroutine(chartFileName));
    }

    /// <summary>
    /// 使用 UnityWebRequest 异步加载谱面文件的协程，兼容所有平台。
    /// </summary>
    private IEnumerator LoadChartCoroutine(string chartFileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Charts", chartFileName);

        // 为PC/Editor平台添加 "file://" 协议头，以确保 UnityWebRequest 能正确处理本地文件路径
        if (Application.platform != RuntimePlatform.WebGLPlayer && Application.platform != RuntimePlatform.Android)
        {
            path = "file://" + path;
        }

        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            // 反序列化为 SimpleBeatmapData，它使用 int[] 列表
            SimpleBeatmapData beatmap = JsonConvert.DeserializeObject<SimpleBeatmapData>(json);
            if (beatmap == null)
            {
                Debug.LogError($"无法解析谱面文件: {chartFileName}", this.gameObject);
                yield break;
            }

            // 将解析出的数据转换为游戏运行时使用的 ChartData 格式
            CurrentChart = ConvertBeatmapToChart(beatmap);
            Debug.Log($"谱面 '{CurrentChart.songName}' (最终精简版) 加载并转换成功，包含 {CurrentChart.notes.Count} 个音符。");

            // 广播事件，通知其他系统（如GameManager）加载已完成
            OnChartLoadComplete?.Invoke();
        }
        else
        {
            Debug.LogError($"找不到或无法加载谱面文件: {path}\n错误: {request.error}", this.gameObject);
        }
    }

    /// <summary>
    /// 将 SimpleBeatmapData (谱面蓝图) 转换为 ChartData (可执行的游戏计划)。
    /// 这个方法是读谱器的核心翻译逻辑。
    /// </summary>
    private ChartData ConvertBeatmapToChart(SimpleBeatmapData beatmap)
    {
        ChartData chart = new ChartData();
        chart.songName = beatmap.songName;
        chart.bpm = beatmap.bpm;
        chart.notes = new List<NoteData>();

        float secPerBeat = 60f / beatmap.bpm;
        float secPerSubdivision = secPerBeat / beatmap.ticksPerBeat;

        foreach (var beatNote in beatmap.notes)
        {
            // 安全检查：一个音符至少需要5个元素
            if (beatNote.Length < 5) continue;

            NoteData note = new NoteData();

            // --- 前5个元素是所有音符共有的 ---
            int beat = beatNote[0];
            int subdivision = beatNote[1];
            StrumType strumType = (StrumType)beatNote[2];
            int fretMask = beatNote[3];
            bool isSpecial = beatNote[4] == 1;

            // 1. 计算时间 (time)
            float totalSubdivisions = (beat - 1) * beatmap.ticksPerBeat + subdivision;
            note.time = beatmap.offset + totalSubdivisions * secPerSubdivision;

            // 2. 转换 StrumType, FretMask, isSpecial
            note.strumType = strumType;
            note.requiredFrets = DecodeFretMask(fretMask);
            note.isSpecial = isSpecial;

            // --- 检查数组长度来判断是Tap还是Hold ---
            if (beatNote.Length >= 6)
            {
                // 这是 HoldNote，第6个元素是它的持续时间（以细分刻度为单位）
                int durationInSubdivisions = beatNote[5];
                note.duration = durationInSubdivisions * secPerSubdivision;
            }
            else
            {
                // 这是 TapNote，持续时间为0
                note.duration = 0;
            }

            chart.notes.Add(note);
        }

        return chart;
    }

    /// <summary>
    /// 将一个整数位掩码 (FretMask) 解码为 FretKey 的列表。
    /// </summary>
    /// <param name="mask">代表和弦的整数</param>
    /// <returns>一个包含所需FretKey的列表</returns>
    private List<FretKey> DecodeFretMask(int mask)
    {
        var frets = new List<FretKey>();
        // 使用位与 (&) 运算来检查每一位是否为1
        // Q:1, W:2, E:4, A:8, S:16, D:32
        if ((mask & 1) != 0) frets.Add(FretKey.Q);
        if ((mask & 2) != 0) frets.Add(FretKey.W);
        if ((mask & 4) != 0) frets.Add(FretKey.E);
        if ((mask & 8) != 0) frets.Add(FretKey.A);
        if ((mask & 16) != 0) frets.Add(FretKey.S);
        if ((mask & 32) != 0) frets.Add(FretKey.D);
        return frets;
    }
}