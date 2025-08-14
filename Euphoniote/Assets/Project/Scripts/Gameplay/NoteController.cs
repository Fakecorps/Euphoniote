// _Project/Scripts/Gameplay/NoteController.cs (完整最终版)

using UnityEngine;
using System.Collections; // 需要引入协程的命名空间

public class NoteController : BaseNoteController
{
    private bool isFadingOut = false; // 防止重复启动协程

    public override void Initialize(NoteData data)
    {
        base.Initialize(data);
    }

    protected override void Update()
    {
        // 如果音符已经被判定成功
        if (IsJudged)
        {
            // 并且还没有开始消失动画，则启动它
            if (!isFadingOut)
            {
                StartCoroutine(FadeOutAndDestroy());
            }
            return; // 判定后不再执行移动或Miss检查
        }

        // 如果未被判定，则执行基类的移动逻辑
        base.Update();

        // Miss判定
        if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
        {
            JudgmentManager.Instance.ProcessMiss(this);
        }
    }

    /// <summary>
    /// 一个简单的淡出并销毁的动画协程
    /// </summary>
    private IEnumerator FadeOutAndDestroy()
    {
        isFadingOut = true;

        // 可选：禁用碰撞体等，防止进一步交互
        // if(GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        float fadeDuration = 0.2f;
        float timer = 0f;

        // 缓存初始颜色
        Color[] initialColors = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            initialColors[i] = allRenderers[i].color;
        }

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

        // 动画结束后销毁GameObject
        Destroy(gameObject);
    }
}