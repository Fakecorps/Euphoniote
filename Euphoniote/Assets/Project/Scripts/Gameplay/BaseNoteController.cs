// _Project/Scripts/Gameplay/BaseNoteController.cs (最终组件化版)

using UnityEngine;
using System.Collections.Generic;

public abstract class BaseNoteController : MonoBehaviour, INoteController
{
    // 不再需要直接引用视觉组件
    public NoteData noteData;
    public bool IsJudged { get; private set; } = false;

    protected float scrollSpeed;
    protected float judgmentLineX;

    public NoteData GetNoteData() => noteData;
    public GameObject GetGameObject() => gameObject;

    // Setup方法保持不变
    public void Setup(float speed, float lineX)
    {
        this.scrollSpeed = speed;
        this.judgmentLineX = lineX;
    }

    // Initialize现在非常简单
    public virtual void Initialize(NoteData data)
    {
        this.noteData = data;
    }

    public void SetJudged() { IsJudged = true; }

    protected virtual void OnEnable() { JudgmentManager.RegisterNote(this); }
    protected virtual void OnDisable() { JudgmentManager.UnregisterNote(this); }

    protected virtual void Update()
    {
        // 移动逻辑保持不变，它移动的是整个Note的根对象
        if (!IsJudged && TimingManager.Instance != null)
        {
            float currentSongTime = TimingManager.Instance.SongPosition;
            float targetX = judgmentLineX + (noteData.time - currentSongTime) * scrollSpeed;
            transform.position = new Vector2(targetX, transform.position.y);
        }
    }
}