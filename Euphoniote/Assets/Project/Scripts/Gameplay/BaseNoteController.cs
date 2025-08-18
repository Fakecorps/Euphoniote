// _Project/Scripts/Gameplay/BaseNoteController.cs (精简版)

using UnityEngine;
using System.Collections.Generic;

public abstract class BaseNoteController : MonoBehaviour
{
    [Header("组件引用")]
    public NoteData noteData;
    public SpriteRenderer arrowSprite;
    public Transform fretContainer;

    [Header("配置")]
    public GameObject fretSpritePrefab;

    public bool IsJudged { get; private set; } = false;

    // 统一由NoteSpawner传入
    protected float scrollSpeed;
    protected float judgmentLineX;

    protected static SpriteAtlas spriteAtlas;

    public void Setup(float speed, float lineX)
    {
        this.scrollSpeed = speed;
        this.judgmentLineX = lineX;
    }

    public virtual void Initialize(NoteData data)
    {
        if (spriteAtlas == null)
        {
            spriteAtlas = Resources.Load<SpriteAtlas>("GameSpriteAtlas");
            if (spriteAtlas != null) { spriteAtlas.Initialize(); }
            else { Debug.LogError("无法在Resources文件夹中找到'GameSpriteAtlas'!"); return; }
        }

        this.noteData = data;

        // 只负责设置箭头
        arrowSprite.sprite = spriteAtlas.GetStrumSprite(data.strumType);

        // 清理旧的字母 (这一步依然保留，很重要)
        foreach (Transform child in fretContainer)
        {
            Destroy(child.gameObject);
        }

        

    }

    public void SetJudged() { IsJudged = true; }

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