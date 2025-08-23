// _Project/Scripts/Gameplay/NoteController.cs (组件化修改版)

using UnityEngine;
using System.Collections;

public class NoteController : BaseNoteController
{
    [Header("核心模块")]
    [Tooltip("对头部模块的引用")]
    public NoteHeadController headController;

    private bool isFadingOut = false;

    public override void Initialize(NoteData data)
    {
        // 调用基类方法，只负责设置 noteData 和其他基础状态
        base.Initialize(data);

        // 检查头部模块引用是否存在
        if (headController == null)
        {
            Debug.LogError("NoteController 上的 Head Controller 字段没有赋值!", this.gameObject);
            return;
        }

        // 将初始化视觉表现的任务完全委托给头部模块
        headController.Initialize(data);
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

        // 现在我们只改变头部模块的透明度
        if (headController != null)
        {
            SpriteRenderer[] allRenderers = headController.GetComponentsInChildren<SpriteRenderer>();
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
        }

        Destroy(gameObject);
    }
}