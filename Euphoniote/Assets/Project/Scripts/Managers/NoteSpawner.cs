// _Project/Scripts/Managers/NoteSpawner.cs (最终重构版 - 报告结束时间)

using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 需要这个来使用 Linq

public class NoteSpawner : MonoBehaviour
{
    public static NoteSpawner Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject notePrefab;
    public GameObject holdNotePrefab;

    [Header("引用")]
    public ChartLoader chartLoader;

    [Header("轨道与生成设置")]
    public float scrollSpeed = 5f;
    public float judgmentLineX = -6f;
    public float spawnX = 20f;
    public float spawnY = 0f;

    [Header("游戏结束设置")]
    [Tooltip("最后一个音符结束后，再等待多少秒才算游戏成功")]
    public float endDelay = 2.0f;

    // --- 公共属性，供 GameManager 查询 ---
    public bool AllNotesSpawned { get; private set; } = false;
    public float GameEndTime { get; private set; } = float.MaxValue;

    private List<NoteData> notesToSpawn;
    private int nextNoteIndex = 0;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void StartSpawning()
    {
        if (chartLoader.CurrentChart == null || chartLoader.CurrentChart.notes.Count == 0)
        {
            Debug.LogError("谱面未加载或没有音符，无法开始生成！", this.gameObject);
            GameEndTime = 0;
            AllNotesSpawned = true;
            return;
        }

        notesToSpawn = new List<NoteData>(chartLoader.CurrentChart.notes);
        notesToSpawn.Sort((a, b) => a.time.CompareTo(b.time));
        nextNoteIndex = 0;
        AllNotesSpawned = false;

        // 计算游戏内容的结束时间
        NoteData lastNote = notesToSpawn.Last();
        GameEndTime = lastNote.time + lastNote.duration + endDelay;

        Debug.Log($"NoteSpawner 已准备就绪。最后一个音符的结束时间是 {lastNote.time + lastNote.duration}s, 游戏将在 {GameEndTime}s 后判定成功。");
    }

    void Update()
    {
        if (PauseManager.IsPaused || PauseManager.IsCountingDown) return;

        if (AllNotesSpawned) return;

        if (TimingManager.Instance == null || !TimingManager.Instance.musicSource.isPlaying)
        {
            return;
        }

        float finalScrollSpeed = this.scrollSpeed * GameSettings.HiSpeed*0.1f;
        float spawnAheadTime = (spawnX - judgmentLineX) / finalScrollSpeed;

        if (spawnAheadTime <= 0)
        {
            Debug.LogError("Spawn Ahead Time 计算结果为0或负数！", this.gameObject);
            this.enabled = false;
            return;
        }

        float songPosition = TimingManager.Instance.SongPosition;

        while (nextNoteIndex < notesToSpawn.Count &&
               songPosition >= notesToSpawn[nextNoteIndex].time - spawnAheadTime)
        {
            NoteData noteToSpawnData = notesToSpawn[nextNoteIndex];

            if (noteToSpawnData.time < songPosition)
            {
                Debug.LogWarning($"跳过一个已超时的音符，时间: {noteToSpawnData.time}");
                nextNoteIndex++;
                continue;
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

        if (nextNoteIndex >= notesToSpawn.Count)
        {
            AllNotesSpawned = true;
            Debug.Log("所有音符已生成完毕。");
        }
    }
}