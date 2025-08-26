// _Project/Scripts/Gameplay/BaseNoteController.cs (最终组件化版)

using UnityEngine;
using System.Collections.Generic;

public abstract class BaseNoteController : MonoBehaviour, INoteController
{
    public NoteData noteData;
    public bool IsJudged { get; set; } = false;

    protected float scrollSpeed;
    protected float judgmentLineX;

    public NoteData GetNoteData() => noteData;
    public GameObject GetGameObject() => gameObject;

    public void Setup(float speed, float lineX)
    {
        this.scrollSpeed = speed;
        this.judgmentLineX = lineX;
    }

    public virtual void Initialize(NoteData data)
    {
        this.noteData = data;
    }

    public void SetJudged() { IsJudged = true; }

    // --- 新增：一个公共方法，用于重置判定状态 ---
    public void ResetJudgedState()
    {
        IsJudged = false;
    }

    protected virtual void OnEnable() { JudgmentManager.RegisterNote(this); }
    protected virtual void OnDisable() { JudgmentManager.UnregisterNote(this); }

    protected virtual void Update()
    {
        if (!IsJudged && TimingManager.Instance != null)
        {
            float currentSongTime = TimingManager.Instance.SongPosition;
            float targetX = judgmentLineX + (noteData.time - currentSongTime) * scrollSpeed;
            transform.position = new Vector2(targetX, transform.position.y);
        }
    }
}