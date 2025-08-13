// _Project/Scripts/Gameplay/BaseNoteController.cs (完整版)

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;

public abstract class BaseNoteController : MonoBehaviour
{
    [Header("组件引用")]
    public NoteData noteData;
    public SpriteRenderer arrowSprite;    // 拖入用于显示箭头的SpriteRenderer
    public Transform fretContainer;       // 拖入一个空子对象，用作字母Sprite的父容器

    [Header("配置")]
    public GameObject fretSpritePrefab; // 拖入用于显示字母的Sprite Prefab

    // 公共的配置
    protected float scrollSpeed = 5f;
    protected float judgmentLineX = -6f;

    // 静态引用，避免每个音符都加载一次资源，提高效率
    private static SpriteAtlas spriteAtlas;

    // 公共的初始化和生命周期方法
    public virtual void Initialize(NoteData data)
    {
        // 第一次调用时，加载SpriteAtlas资源
        if (spriteAtlas == null)
        {
            // 确保你的SpriteAtlas资源放在 "Assets/Resources" 文件夹下，并命名为 "GameSpriteAtlas"
            spriteAtlas = Resources.Load<SpriteAtlas>("GameSpriteAtlas");
            if (spriteAtlas != null)
            {
                spriteAtlas.Initialize();
            }
            else
            {
                Debug.LogError("无法在Resources文件夹中找到'GameSpriteAtlas'!");
                return;
            }
        }

        this.noteData = data;

        // 1. 设置箭头 Sprite
        arrowSprite.sprite = spriteAtlas.GetStrumSprite(data.strumType);

        // 2. 清理旧的字母 (如果复用对象池，这一步很重要)
        foreach (Transform child in fretContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. 根据 requiredFrets 创建并排列字母 Sprite
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
        if (TimingManager.Instance == null) return;

        float currentSongTime = TimingManager.Instance.SongPosition;
        float targetX = judgmentLineX + (noteData.time - currentSongTime) * scrollSpeed;
        transform.position = new Vector2(targetX, transform.position.y);
    }
}