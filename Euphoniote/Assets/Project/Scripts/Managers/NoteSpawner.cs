using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab; // 引用我们创建的Note Prefab
    public ChartLoader chartLoader;

    private List<NoteData> notesToSpawn;
    private int nextNoteIndex = 0;
    private float spawnAheadTime = 3f; // 提前多少秒生成音符

    public void StartSpawning()
    {
        if (chartLoader.CurrentChart == null)
        {
            Debug.LogError("谱面未加载，无法开始生成！");
            return;
        }
        notesToSpawn = new List<NoteData>(chartLoader.CurrentChart.notes);
        // 可以选择按时间排序一下，确保万无一失
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

        // 检查下一个音符是否需要生成
        if (songPosition >= notesToSpawn[nextNoteIndex].time - spawnAheadTime)
        {
            NoteData noteToSpawnData = notesToSpawn[nextNoteIndex];

            // 实例化Prefab
            GameObject noteObject = Instantiate(notePrefab, new Vector3(20, 0, 0), Quaternion.identity); // 先生成在屏幕外很远的地方

            // 获取控制器并初始化
            NoteController controller = noteObject.GetComponent<NoteController>();
            controller.Initialize(noteToSpawnData);

            // 准备生成下一个
            nextNoteIndex++;
        }
    }
}
