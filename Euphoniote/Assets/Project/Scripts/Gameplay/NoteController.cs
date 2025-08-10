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
    void OnEnable()
    {
        // 当音符被创建并激活时，向JudgmentManager注册自己
        JudgmentManager.RegisterNote(this);
    }

    void OnDisable()
    {
        // 当音符被销毁或禁用时，从JudgmentManager注销自己
        JudgmentManager.UnregisterNote(this);
    }
    public void Initialize(NoteData data)
    {
        noteData = data;
        if (noteData.requiredFrets != null && noteData.requiredFrets.Count > 0)
        {
            string displayText = "";
            for (int i = 0; i < noteData.requiredFrets.Count; i++)
            {
                Debug.Log(noteData.requiredFrets[i]);
                displayText += noteData.requiredFrets[i].ToString();
                // 如果不是最后一个字母，就在后面加上一个连接符，比如空格或加号
                if (i < noteData.requiredFrets.Count - 1)
                {
                    displayText += " "; // 
                }
            }
            fretText.text = displayText;
        }
        else
        {
            // 如果这个音符不需要按键（虽然不太可能，但做好防御性编程）
            fretText.text = "";
        }
        // TODO: 在这里根据 noteData.strumType 和 isSpecial 改变箭头的Sprite和特效
    }

    void Update()
    {
        if (TimingManager.Instance == null) return;

        float currentSongTime = TimingManager.Instance.SongPosition;
        float targetX = judgmentLineX + (noteData.time - currentSongTime) * scrollSpeed;
        transform.position = new Vector2(targetX, transform.position.y);

        // 修改Miss判定的逻辑
        if (noteData.time < currentSongTime - JudgmentManager.Instance.goodWindow)
        {
            Debug.Log("Miss");
            // TODO: 在这里通知ScoreManager连击断了
            Destroy(gameObject); // OnDisable会自动调用UnregisterNote
        }
    }
}
