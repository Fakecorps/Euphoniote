// _Project/Scripts/Managers/NoteSpawner.cs (手动配置版)

using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("普通的单击音符Prefab")]
    public GameObject notePrefab;
    [Tooltip("长按音符的Prefab")]
    public GameObject holdNotePrefab;

    [Header("引用")]
    [Tooltip("谱面加载器")]
    public ChartLoader chartLoader;

    // --- 【核心修改】 ---
    [Header("轨道与生成设置 (手动配置)")]
    [Tooltip("音符在轨道上的滚动速度 (单位/秒)，这个值会传递给所有生成的音符")]
    public float scrollSpeed = 5f;

    [Tooltip("判定线的X坐标，同样会传递给所有音符")]
    public float judgmentLineX = -6f;

    [Tooltip("音符生成的X坐标，确保这个值在屏幕右侧视野外")]
    public float spawnX = 20f;

    [Tooltip("音符生成的Y坐标，可以调整音符轨道在屏幕上的垂直位置")]
    public float spawnY = 0f;
    // --- 修改结束 ---

    private List<NoteData> notesToSpawn;
    private int nextNoteIndex = 0;
    private float spawnAheadTime; // 提前生成的时间

    void Start()
    {
        spawnAheadTime = (spawnX - judgmentLineX) / scrollSpeed;

        // 添加一个安全检查
        if (spawnAheadTime <= 0)
        {
            Debug.LogError("Spawn Ahead Time 计算结果为0或负数！请确保 Spawn X 远大于 Judgment Line X。", this.gameObject);
        }
    }

    /// <summary>
    /// 由GameManager调用，开始谱面的生成流程。
    /// </summary>
    public void StartSpawning()
    {
        if (chartLoader.CurrentChart == null)
        {
            Debug.LogError("谱面未加载，无法开始生成！", this.gameObject);
            return;
        }

        // 深度复制谱面数据，确保我们使用的是一份干净的副本
        notesToSpawn = new List<NoteData>();
        foreach (var originalNoteData in chartLoader.CurrentChart.notes)
        {
            NoteData newNoteData = new NoteData();
            newNoteData.time = originalNoteData.time;
            newNoteData.duration = originalNoteData.duration;
            newNoteData.isSpecial = originalNoteData.isSpecial;
            newNoteData.strumType = originalNoteData.strumType;
            if (originalNoteData.requiredFrets != null)
                newNoteData.requiredFrets = new List<FretKey>(originalNoteData.requiredFrets);
            else
                newNoteData.requiredFrets = new List<FretKey>();
            notesToSpawn.Add(newNoteData);
        }

        // 按时间排序并重置索引
        notesToSpawn.Sort((a, b) => a.time.CompareTo(b.time));
        nextNoteIndex = 0;
    }

    void Update()
    {
        if (notesToSpawn == null || nextNoteIndex >= notesToSpawn.Count)
        {
            return;
        }

        float songPosition = TimingManager.Instance.SongPosition;

        // 当歌曲时间进入“需要生成下一个音符”的窗口时
        if (songPosition >= notesToSpawn[nextNoteIndex].time - spawnAheadTime)
        {
            NoteData noteToSpawnData = notesToSpawn[nextNoteIndex];

            // 使用你手动设置的 spawnX 来生成音符
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, -0.2f);

            GameObject noteObject;
            BaseNoteController controller;

            if (noteToSpawnData.duration > 0)
            {
                noteObject = Instantiate(holdNotePrefab, spawnPosition, Quaternion.identity);
                controller = noteObject.GetComponent<HoldNoteController>();
            }
            else
            {
                noteObject = Instantiate(notePrefab, spawnPosition, Quaternion.identity);
                controller = noteObject.GetComponent<NoteController>();
            }

            // 在Initialize之前，将手动设置的参数传递给NoteController
            controller.Setup(scrollSpeed, judgmentLineX);
            controller.Initialize(noteToSpawnData);

            nextNoteIndex++;
        }
    }
}