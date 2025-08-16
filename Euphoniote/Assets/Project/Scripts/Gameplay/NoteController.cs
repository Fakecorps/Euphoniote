// _Project/Scripts/Gameplay/NoteController.cs (实现垂直排列)

using UnityEngine;
using System.Collections;

public class NoteController : BaseNoteController
{
    private bool isFadingOut = false;

    public override void Initialize(NoteData data)
    {
        base.Initialize(data); // 先执行公共初始化（设置箭头等）

        // --- TapNote专属：垂直排列字母 ---
        if (data.requiredFrets != null && data.requiredFrets.Count > 0)
        {
            float spriteHeight = 2.4f; // 可根据美术资源调整
            int noteCount = data.requiredFrets.Count;
            float totalHeight = noteCount * spriteHeight;
            float startY = (totalHeight / 2f) - (spriteHeight / 2f);

            for (int i = 0; i < noteCount; i++)
            {
                GameObject fretObj = Instantiate(fretSpritePrefab, fretContainer);
                fretObj.GetComponent<SpriteRenderer>().sprite = spriteAtlas.GetFretSprite(data.requiredFrets[i]);
                fretObj.transform.localPosition = new Vector3(0, startY - i * spriteHeight, 0);
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