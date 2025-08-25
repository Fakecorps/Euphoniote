// _Project/Scripts/Managers/NoteSpawner.cs

using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    // 添加一个单例，方便 JudgmentManager 获取 judgmentLineX
    public static NoteSpawner Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject notePrefab;
    public GameObject holdNotePrefab;

    [Header("引用")]
    public ChartLoader chartLoader;

    [Header("轨道与生成设置 (手动配置)")]
    public float scrollSpeed = 5f;
    public float judgmentLineX = -6f;
    public float spawnX = 20f;
    public float spawnY = 0f;

    private List<NoteData> notesToSpawn;
    private int nextNoteIndex = 0;
    private float spawnAheadTime;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        spawnAheadTime = (spawnX - judgmentLineX) / scrollSpeed;
        if (spawnAheadTime <= 0)
        {
            Debug.LogError("Spawn Ahead Time 计算结果为0或负数！请确保 Spawn X 远大于 Judgment Line X。", this.gameObject);
        }
    }

    public void StartSpawning()
    {
        if (chartLoader.CurrentChart == null)
        {
            Debug.LogError("谱面未加载，无法开始生成！", this.gameObject);
            return;
        }

        // 深度复制逻辑
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

        if (songPosition >= notesToSpawn[nextNoteIndex].time - spawnAheadTime)
        {
            NoteData noteToSpawnData = notesToSpawn[nextNoteIndex];
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, -0.1f);
            GameObject noteObject;

            if (noteToSpawnData.duration > 0)
            {
                noteObject = NotePoolManager.Instance.GetFromPool("HoldNote");
                if (noteObject == null) return; // 安全检查，防止标签写错

                noteObject.transform.position = spawnPosition;
                noteObject.transform.rotation = Quaternion.identity;

                HoldNoteController controller = noteObject.GetComponent<HoldNoteController>();
                controller.Initialize(noteToSpawnData, scrollSpeed, judgmentLineX);
            }
            else
            {
                noteObject = NotePoolManager.Instance.GetFromPool("TapNote");
                if (noteObject == null) return; // 安全检查

                noteObject.transform.position = spawnPosition;
                noteObject.transform.rotation = Quaternion.identity;

                NoteController controller = noteObject.GetComponent<NoteController>();
                controller.Setup(scrollSpeed, judgmentLineX);
                controller.Initialize(noteToSpawnData);
            }

            nextNoteIndex++;
        }
    }
}