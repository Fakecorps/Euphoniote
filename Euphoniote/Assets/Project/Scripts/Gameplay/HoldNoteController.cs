// _Project/Scripts/Gameplay/HoldNoteController.cs (最终修正版)

using UnityEngine;

public class HoldNoteController : BaseNoteController
{
    [Header("Hold Note 特定组件")]
    [Tooltip("长条的SpriteRenderer，请确保它的Draw Mode设置为Sliced")]
    public SpriteRenderer holdTrailRenderer;

    private bool isBeingHeld = false;

    public override void Initialize(NoteData data)
    {
        base.Initialize(data);

        if (holdTrailRenderer != null)
        {
            Sprite holdTemplateSprite = spriteAtlas.GetHoldNoteSprite(data.strumType);
            if (holdTemplateSprite != null) { holdTrailRenderer.sprite = holdTemplateSprite; }
            else { Debug.LogWarning($"SpriteAtlas中找不到 '{data.strumType}' 对应的Hold Note模板！", this.gameObject); holdTrailRenderer.sprite = null; }

            if (holdTrailRenderer.drawMode != SpriteDrawMode.Sliced) { Debug.LogWarning("HoldTrailRenderer的Draw Mode不是Sliced！", this.gameObject); }

            float trailWidth = data.duration * scrollSpeed;
            holdTrailRenderer.size = new Vector2(trailWidth, holdTrailRenderer.size.y);
            holdTrailRenderer.transform.localPosition = new Vector3(trailWidth / 2f, 0, 0);
        }
        else
        {
            Debug.LogError("HoldNoteController上的 holdTrailRenderer 引用为空！", this.gameObject);
        }
    }

    protected override void Update()
    {
        if (isBeingHeld)
        {
            transform.position = new Vector2(judgmentLineX, transform.position.y);

            float songPosition = TimingManager.Instance.SongPosition;
            float elapsedTime = songPosition - noteData.time;
            if (elapsedTime < 0) elapsedTime = 0;

            float consumedWidth = elapsedTime * scrollSpeed;
            float originalTotalWidth = noteData.duration * scrollSpeed;
            float remainingWidth = originalTotalWidth - consumedWidth;
            if (remainingWidth < 0) remainingWidth = 0;

            if (holdTrailRenderer != null)
            {
                holdTrailRenderer.size = new Vector2(remainingWidth, holdTrailRenderer.size.y);
                // 修正后的位置补偿逻辑
                holdTrailRenderer.transform.localPosition = new Vector3(remainingWidth / 2f, 0, 0);
            }
            float endTime = noteData.time + noteData.duration;

            if (TimingManager.Instance.SongPosition > endTime + 0.2f)
            {
                // 算作一个 Late Release Miss，或者直接算成功也可以，取决于你的设计
                Debug.Log("Hold Release (Timeout)");
                Destroy(gameObject);
            }
        }
        else
        {
            if (IsJudged)
            {
                Destroy(gameObject);
                return;
            }

            base.Update();
            if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
            {
                JudgmentManager.Instance.ProcessMiss(this);
            }
        }
    }

    public void SetHeldState(bool held)
    {
        isBeingHeld = held;
        if (isBeingHeld)
        {
            SetJudged();
        }
    }
}