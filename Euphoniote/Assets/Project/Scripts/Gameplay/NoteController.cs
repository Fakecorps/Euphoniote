using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NoteController : MonoBehaviour
{
    public NoteData noteData; // 这个音符的数据
    public TextMeshPro fretText;

    // 这两个值需要从外部设置，或者从一个设置管理器获取
    private float scrollSpeed = 5f; // 音符移动速度
    private float judgmentLineX = -6f; // 判定线的X坐标

    public void Initialize(NoteData data)
    {
        noteData = data;
        // 更新音符外观
        if (noteData.requiredFrets.Count > 0)
        {
            fretText.text = noteData.requiredFrets[0].ToString(); // 简单起见，先只显示第一个字母
        }
        // TODO: 在这里根据 noteData.strumType 和 isSpecial 改变箭头的Sprite和特效
    }

    void Update()
    {
        if (TimingManager.Instance == null) return;

        // 根据时间计算位置
        float currentSongTime = TimingManager.Instance.SongPosition;
        float targetX = judgmentLineX + (noteData.time - currentSongTime) * scrollSpeed;

        transform.position = new Vector2(targetX, transform.position.y); // Y轴位置在生成时设定

        // 如果音符已经飘过判定线太远，就销毁自己 (简单的Miss处理)
        if (transform.position.x < judgmentLineX - 2f)
        {
            // TODO: 在这里通知JudgmentManager这是一个Miss
            Destroy(gameObject);
        }
    }
}
