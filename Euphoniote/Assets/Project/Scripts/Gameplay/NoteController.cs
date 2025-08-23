// _Project/Scripts/Gameplay/NoteController.cs (实现垂直排列)

using UnityEngine;
using System.Collections;

public class NoteController : BaseNoteController
{
    private bool isFadingOut = false;

    [Header("Note 特有组件")]
    [Tooltip("对九宫格容器背景的SpriteRenderer的引用")]
    public SpriteRenderer containerRenderer;

    [Header("布局参数")]
    [Tooltip("单个字母在容器中所占的高度")]
    public float heightPerLetter = 0.8f;
    [Tooltip("容器上下的额外内边距")]
    public float verticalPadding = 0.2f;

    public override void Initialize(NoteData data)
    {
        base.Initialize(data); // 先执行公共初始化（设置箭头等）

        if (containerRenderer == null)
        {
            Debug.LogError("NoteController 上的 Container Renderer 字段没有赋值!", this.gameObject);
            return;
        }

        // --- 核心逻辑：先选模板，再计算拉伸 ---

        // 1. 根据是否为特殊音符，从 SpriteAtlas 获取正确的【模板图】
        Sprite containerTemplate = spriteAtlas.GetContainerTemplate(data.isSpecial);
        if (containerTemplate != null)
        {
            containerRenderer.sprite = containerTemplate;
        }
        else
        {
            Debug.LogError($"在SpriteAtlas中找不到 isSpecial={data.isSpecial} 对应的容器模板！");
            return;
        }

        // 2. 计算字母数量
        // 规则：即使没有字母(Open Note)，也按1个字母的高度来显示容器
        int fretCount = (data.requiredFrets != null) ? data.requiredFrets.Count : 0;
        int containerSizeLevel = (fretCount == 0) ? 1 : fretCount; // 用于计算高度的级别

        // 3. 计算并设置容器背景的拉伸高度
        float targetHeight = (containerSizeLevel * heightPerLetter) + (verticalPadding * 2);
        containerRenderer.size = new Vector2(containerRenderer.size.x, targetHeight);

        // 4. 在容器内垂直排列字母 (只有在 fretCount > 0 时才执行)
        if (fretCount > 0)
        {
            float totalLetterHeight = fretCount * heightPerLetter;
            float startY = (totalLetterHeight / 2f) - (heightPerLetter / 2f);

            for (int i = 0; i < fretCount; i++)
            {
                GameObject fretObj = Instantiate(fretSpritePrefab, fretContainer);
                fretObj.GetComponent<SpriteRenderer>().sprite = spriteAtlas.GetFretSprite(data.requiredFrets[i]);
                fretObj.transform.localPosition = new Vector3(0, startY - i * heightPerLetter, 0);
            }
        }
    }

    protected override void Update()
    {
        if (IsJudged)
        {
            if (!isFadingOut) { StartCoroutine(FadeOutAndDestroy()); }
            return;
        }
        base.Update();
        if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
        {
            JudgmentManager.Instance.ProcessMiss(this);
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isFadingOut = true;
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        float fadeDuration = 0.2f;
        float timer = 0f;
        Color[] initialColors = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++) { initialColors[i] = allRenderers[i].color; }

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            for (int i = 0; i < allRenderers.Length; i++)
            {
                allRenderers[i].color = new Color(initialColors[i].r, initialColors[i].g, initialColors[i].b, alpha);
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}