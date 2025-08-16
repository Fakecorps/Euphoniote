// _Project/Scripts/Managers/NoteSpawner.cs (完整最终版)

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

    [Header("生成设置")]
    [Tooltip("音符滚动速度，这个值会传递给所有生成的音符")]
    public float scrollSpeed = 5f;
    [Tooltip("判定线的X坐标，同样会传递给所有音符")]
    public float judgmentLineX = -6f;

    private List<NoteData> notesToSpawn;
    private int nextNoteIndex = 0;

    private float spawnX;         // 音符生成的X坐标
    private float spawnAheadTime; // 根据配置动态计算出的提前生成时间

    void Start()
    {
        // 在游戏开始时计算一次生成位置和提前时间
        CalculateSpawnPoint();
    }

    /// <summary>
    /// 根据屏幕宽度和滚动速度，计算出音符应该在何处生成，以及需要提前多久生成。
    /// </summary>
    private void CalculateSpawnPoint()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("场景中找不到Tag为 'MainCamera' 的主摄像机!", this.gameObject);
            spawnX = 20f; // 使用一个默认的屏幕外坐标
            spawnAheadTime = (spawnX - judgmentLineX) / scrollSpeed;
            return;
        }

        // 获取屏幕右上角在世界坐标系中的位置
        Vector3 screenTopRight = new Vector3(Screen.width, Screen.height, mainCamera.nearClipPlane);
        Vector3 worldTopRight = mainCamera.ScreenToWorldPoint(screenTopRight);

        // 设置生成点在屏幕右边界再往右一点的位置，确保完全在视野外
        spawnX = worldTopRight.x + 1.0f;

        // 核心公式：时间 = 距离 / 速度
        spawnAheadTime = (spawnX - judgmentLineX) / scrollSpeed;
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
            return; // 谱面未加载或已全部生成
        }

        float songPosition = TimingManager.Instance.SongPosition;

        // 当歌曲时间进入“需要生成下一个音符”的窗口时
        if (songPosition >= notesToSpawn[nextNoteIndex].time - spawnAheadTime)
        {
            NoteData noteToSpawnData = notesToSpawn[nextNoteIndex];
            GameObject noteObject;
            BaseNoteController controller;

            // 根据duration判断应该生成哪种类型的音符
            if (noteToSpawnData.duration > 0)
            {
                noteObject = Instantiate(holdNotePrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);
                controller = noteObject.GetComponent<HoldNoteController>();
            }
            else
            {
                noteObject = Instantiate(notePrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);
                controller = noteObject.GetComponent<NoteController>();
            }

            // 在Initialize之前，将滚动速度等核心参数传递给NoteController
            controller.Setup(scrollSpeed, judgmentLineX);
            // 初始化音符，设置其外观
            controller.Initialize(noteToSpawnData);

            // 准备生成下一个音符
            nextNoteIndex++;
        }
    }
}