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
        notesToSpawn = new List<NoteData>();
        foreach (var originalNoteData in chartLoader.CurrentChart.notes)
        {
            // 创建一个全新的 NoteData 实例
            NoteData newNoteData = new NoteData();

            // 复制值类型字段
            newNoteData.time = originalNoteData.time;
            newNoteData.duration = originalNoteData.duration;
            newNoteData.isSpecial = originalNoteData.isSpecial;
            newNoteData.strumType = originalNoteData.strumType;

            // 为引用类型字段 (List) 创建一个全新的实例并复制内容
            if (originalNoteData.requiredFrets != null)
            {
                newNoteData.requiredFrets = new List<FretKey>(originalNoteData.requiredFrets);
            }
            else
            {
                newNoteData.requiredFrets = new List<FretKey>();
            }

            // 将这个完全独立的新对象添加到列表中
            notesToSpawn.Add(newNoteData);
        }
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
