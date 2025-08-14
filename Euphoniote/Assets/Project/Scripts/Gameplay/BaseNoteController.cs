// _Project/Scripts/Gameplay/BaseNoteController.cs (完整最终版)

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

    public bool IsJudged { get; private set; } = false; // 新增状态，默认为false

    // 公共的配置
    protected float scrollSpeed = 5f;
    protected float judgmentLineX = -6f;

    protected static SpriteAtlas spriteAtlas;

    public virtual void Initialize(NoteData data)
    {
        if (spriteAtlas == null)
        {
            spriteAtlas = Resources.Load<SpriteAtlas>("GameSpriteAtlas");
            if (spriteAtlas != null) { spriteAtlas.Initialize(); }
            else { Debug.LogError("无法在Resources文件夹中找到'GameSpriteAtlas'!"); return; }
        }

        this.noteData = data;

        arrowSprite.sprite = spriteAtlas.GetStrumSprite(data.strumType);

        foreach (Transform child in fretContainer) { Destroy(child.gameObject); }

        if (data.requiredFrets != null && data.requiredFrets.Count > 0)
        {
            float spriteHeight = 2.2f;
            int noteCount = data.requiredFrets.Count;
            float totalHeight = noteCount * spriteHeight;
            float startY = (totalHeight / 2f) - (spriteHeight / 2f);

            for (int i = 0; i < noteCount; i++)
            {
                GameObject fretObj = Instantiate(fretSpritePrefab, fretContainer);
                SpriteRenderer fretRenderer = fretObj.GetComponent<SpriteRenderer>();
                fretRenderer.sprite = spriteAtlas.GetFretSprite(data.requiredFrets[i]);
                fretObj.transform.localPosition = new Vector3(0, startY - i * spriteHeight, 0);
            }
        }
    }

    /// <summary>
    /// 提供一个公共方法来将音符标记为已判定。
    /// </summary>
    public void SetJudged()
    {
        IsJudged = true;
    }

    protected virtual void OnEnable()
    {
        JudgmentManager.RegisterNote(this);
    }

    protected virtual void OnDisable()
    {
        JudgmentManager.UnregisterNote(this);
    }

    protected virtual void Update()
    {
        // 仅当音符未被判定时才移动
        if (!IsJudged && TimingManager.Instance != null)
        {
            float currentSongTime = TimingManager.Instance.SongPosition;
            float targetX = judgmentLineX + (noteData.time - currentSongTime) * scrollSpeed;
            transform.position = new Vector2(targetX, transform.position.y);
        }
    }
}