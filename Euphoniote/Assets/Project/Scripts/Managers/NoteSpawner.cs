// _Project/Scripts/Managers/NoteSpawner.cs (最终修复版 - 统一速度计算)

using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    public static NoteSpawner Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject notePrefab;
    public GameObject holdNotePrefab;

    [Header("引用")]
    public ChartLoader chartLoader;

    [Header("轨道与生成设置")]
    [Tooltip("基础滚动速度，将与玩家设置的Hi-Speed相乘")]
    public float scrollSpeed = 5f;
    public float judgmentLineX = -6f;
    public float spawnX = 20f;
    public float spawnY = 0f;

    private List<NoteData> notesToSpawn;
    private int nextNoteIndex = 0;

    // --- 我们将不再缓存 spawnAheadTime，而是在Update中实时计算 ---
    // private float spawnAheadTime; 

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void StartSpawning()
    {
        if (chartLoader.CurrentChart == null)
        {
            Debug.LogError("谱面未加载，无法开始生成！", this.gameObject);
            return;
        }

        notesToSpawn = new List<NoteData>(chartLoader.CurrentChart.notes);
        notesToSpawn.Sort((a, b) => a.time.CompareTo(b.time));
        nextNoteIndex = 0;

        Debug.Log($"NoteSpawner 已准备就绪，Hi-Speed: {GameSettings.HiSpeed}");
    }

    void Update()
    {
        // 如果 TimingManager 不存在或音乐未播放，则不执行任何操作
        if (TimingManager.Instance == null || !TimingManager.Instance.musicSource.isPlaying)
        {
            return;
        }

        if (notesToSpawn == null || nextNoteIndex >= notesToSpawn.Count)
        {
            return;
        }

        // --- 核心修改：在 Update 中实时计算所有需要的参数 ---
        float finalScrollSpeed = this.scrollSpeed * GameSettings.HiSpeed*0.1f;
        float spawnAheadTime = (spawnX - judgmentLineX) / finalScrollSpeed;

        if (spawnAheadTime <= 0)
        {
            // 如果计算出错，立即停止以防无限生成
            Debug.LogError("Spawn Ahead Time 计算结果为0或负数！", this.gameObject);
            this.enabled = false; // 禁用自身
            return;
        }

        float songPosition = TimingManager.Instance.SongPosition;

        // --- 核心修改：确保只有在需要时才生成音符 ---
        // 增加一个检查，确保我们不会生成已经“过时”的音符
        // 循环检查，以处理高密度谱面或卡顿时一次性生成多个音符的情况
        while (nextNoteIndex < notesToSpawn.Count &&
               songPosition >= notesToSpawn[nextNoteIndex].time - spawnAheadTime)
        {
            NoteData noteToSpawnData = notesToSpawn[nextNoteIndex];

            // 安全检查：如果音符已经严重超时，就直接跳过它，不生成
            if (noteToSpawnData.time < songPosition)
            {
                Debug.LogWarning($"跳过一个已超时的音符，时间: {noteToSpawnData.time}");
                nextNoteIndex++;
                continue; // 继续检查下一个
            }

            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
            GameObject noteObject;

            if (noteToSpawnData.duration > 0)
            {
                noteObject = NotePoolManager.Instance.GetFromPool("HoldNote");
                if (noteObject == null) { nextNoteIndex++; continue; }
                noteObject.transform.position = spawnPosition;

                HoldNoteController controller = noteObject.GetComponent<HoldNoteController>();
                controller.Initialize(noteToSpawnData, finalScrollSpeed, judgmentLineX);
            }
            else
            {
                noteObject = NotePoolManager.Instance.GetFromPool("TapNote");
                if (noteObject == null) { nextNoteIndex++; continue; }
                noteObject.transform.position = spawnPosition;

                NoteController controller = noteObject.GetComponent<NoteController>();
                controller.Setup(finalScrollSpeed, judgmentLineX);
                controller.Initialize(noteToSpawnData);
            }

            nextNoteIndex++;
        }
    }
}