// _Project/Scripts/Gameplay/NoteController.cs (最终修改版 - 分离消失逻辑)

using UnityEngine;
using System.Collections;

public class NoteController : BaseNoteController
{
    [Header("核心模块")]
    [Tooltip("对头部模块的引用")]
    public NoteHeadController headController;

    private bool isBeingReleased = false; // 状态锁，防止重复执行回收逻辑

    public override void Initialize(NoteData data)
    {
        base.Initialize(data);
        // 为对象池重用做准备，重置状态
        PrepareForPooling();

        if (headController == null)
        {
            Debug.LogError("NoteController 上的 Head Controller 字段没有赋值!", this.gameObject);
            return;
        }
        headController.Initialize(data);
    }

    /// <summary>
    /// 为对象池做准备的清理方法。
    /// </summary>
    public void PrepareForPooling()
    {
        isBeingReleased = false;
        ResetJudgedState();
        // 恢复所有视觉组件的透明度
        if (headController != null)
        {
            SpriteRenderer[] allRenderers = headController.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in allRenderers)
            {
                if (renderer != null)
                    renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f);
            }
        }
    }

    /// <summary>
    /// [成功时调用] 立即消失并返回对象池。
    /// </summary>
    public void ReleaseImmediately()
    {
        if (isBeingReleased) return;
        isBeingReleased = true;

        SetJudged();
        PrepareForPooling();
        NotePoolManager.Instance.ReturnToPool("TapNote", gameObject);
    }

    /// <summary>
    /// [失败时调用] 播放Miss动画后返回对象池。
    /// </summary>
    public void PlayMissAnimationAndRelease()
    {
        if (isBeingReleased) return;
        isBeingReleased = true;

        SetJudged();
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        if (headController != null)
        {
            SpriteRenderer[] allRenderers = headController.GetComponentsInChildren<SpriteRenderer>();
            float fadeDuration = 0.2f;
            float timer = 0f;
            Color[] initialColors = new Color[allRenderers.Length];
            for (int i = 0; i < allRenderers.Length; i++) { if (allRenderers[i] != null) initialColors[i] = allRenderers[i].color; }

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                for (int i = 0; i < allRenderers.Length; i++)
                {
                    if (allRenderers[i] != null)
                        allRenderers[i].color = new Color(initialColors[i].r, initialColors[i].g, initialColors[i].b, alpha);
                }
                yield return null;
            }
        }

        // 动画播放完毕后，清理并返回对象池
        PrepareForPooling();
        NotePoolManager.Instance.ReturnToPool("TapNote", gameObject);
    }

    /// <summary>
    /// Update方法现在只负责移动和检测超时Miss。
    /// </summary>
    protected override void Update()
    {
        // 如果已经被标记为处理中，则不执行任何操作
        if (IsJudged) return;

        base.Update(); // 调用基类的移动逻辑

        // 检查超时Miss
        if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
        {
            // 自己调用Miss动画
            PlayMissAnimationAndRelease();
            // 仍然需要通过JudgmentManager广播Miss事件给StatsManager等系统
            JudgmentManager.Instance.BroadcastMissEvent(this);
        }
    }
}