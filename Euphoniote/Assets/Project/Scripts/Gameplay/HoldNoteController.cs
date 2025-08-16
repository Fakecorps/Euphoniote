using UnityEngine;

public class HoldNoteController : BaseNoteController
{
    [Header("Hold Note 特定组件")]
    public SpriteRenderer holdTrailRenderer;
    private bool isBeingHeld = false;

    public float HeadTimeDiff { get; private set; }
    public void SetHeadTimeDiff(float diff)
    {
        HeadTimeDiff = Mathf.Abs(diff);
    }

    public override void Initialize(NoteData data)
    {
        base.Initialize(data);

        if (data.requiredFrets != null && data.requiredFrets.Count > 0)
        {
            float spriteWidth = 0.5f;
            int noteCount = data.requiredFrets.Count;
            float totalWidth = noteCount * spriteWidth;
            float startX = -(totalWidth / 2f) + (spriteWidth / 2f);
            for (int i = 0; i < noteCount; i++)
            {
                GameObject fretObj = Instantiate(fretSpritePrefab, fretContainer);
                fretObj.GetComponent<SpriteRenderer>().sprite = spriteAtlas.GetFretSprite(data.requiredFrets[i]);
                fretObj.transform.localPosition = new Vector3(startX + i * spriteWidth, 0, 0);
            }
        }

        if (holdTrailRenderer != null)
        {
            Sprite holdTemplateSprite = spriteAtlas.GetHoldNoteSprite(data.strumType);
            if (holdTemplateSprite != null) { holdTrailRenderer.sprite = holdTemplateSprite; }
            else { Debug.LogWarning($"SpriteAtlas中找不到 '{data.strumType}' 对应的模板！", this.gameObject); holdTrailRenderer.sprite = null; }
            if (holdTrailRenderer.drawMode != SpriteDrawMode.Sliced) { Debug.LogWarning("Draw Mode不是Sliced！", this.gameObject); }
            float trailWidth = data.duration * scrollSpeed;
            holdTrailRenderer.size = new Vector2(trailWidth, holdTrailRenderer.size.y);
            holdTrailRenderer.transform.localPosition = new Vector3(trailWidth / 2f, 0, 0);
        }
        else { Debug.LogError("holdTrailRenderer 引用为空！", this.gameObject); }
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
                holdTrailRenderer.transform.localPosition = new Vector3(remainingWidth / 2f, 0, 0);
            }
        }
        else
        {
            // 如果 IsJudged 为 true，说明它经历了一次判定（Miss 或 HoldBreak）
            // 此时它应该自我销毁。
            if (IsJudged)
            {
                Destroy(gameObject);
                return;
            }

            // 如果还未被判定，则正常移动和检查超时Miss
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