// _Project/Scripts/Gameplay/HoldNoteController.cs

using UnityEngine;

public class HoldNoteController : BaseNoteController
{
    [Header("核心模块")]
    [Tooltip("对头部模块(NoteHead.prefab实例)的引用")]
    public NoteHeadController headController;
    [Tooltip("对结束帽模块(NoteHead.prefab实例)的引用")]
    public NoteHeadController endCapController;

    [Header("长条组件")]
    [Tooltip("箭头后面的长条SpriteRenderer")]
    public SpriteRenderer arrowTrailRenderer;
    [Tooltip("容器后面的长条SpriteRenderer")]
    public SpriteRenderer containerTrailRenderer;

    [Header("布局与尺寸 (手动配置)")]
    [Tooltip("头部的视觉宽度，用于计算偏移量")]
    public float headVisualWidth = 1.5f;

    // 内部状态
    private bool isBeingHeld = false;
    public float HeadTimeDiff { get; private set; }

    /// <summary>
    /// 设置头部判定的时间误差，由 JudgmentManager 调用。
    /// </summary>
    public void SetHeadTimeDiff(float diff)
    {
        HeadTimeDiff = Mathf.Abs(diff);
    }

    /// <summary>
    /// 初始化 HoldNote 的所有视觉元素。
    /// </summary>
    public override void Initialize(NoteData data)
    {
        // 调用基类方法，只负责设置 noteData
        base.Initialize(data);

        // 1. 初始化头部 (带字母)
        if (headController != null)
        {
            // true 表示需要生成字母
            headController.Initialize(data, true);
        }
        else
        {
            Debug.LogError("HoldNoteController 上的 Head Controller 引用为空！", this.gameObject);
        }

        // 2. 初始化结束帽 (不带字母)
        if (endCapController != null)
        {
            // false 表示这是一个空壳，不需要生成字母
            endCapController.Initialize(data, false);
        }
        else
        {
            Debug.LogError("HoldNoteController 上的 End Cap Controller 引用为空！", this.gameObject);
        }

        // 3. 初始化长条和结束帽的位置
        float trailLength = data.duration * scrollSpeed;

        // a. 设置长条
        if (arrowTrailRenderer != null && containerTrailRenderer != null)
        {
            // 在这里你可以从 SpriteAtlas 获取并设置长条的 Sprite (如果需要动态更换)
            // 示例: arrowTrailRenderer.sprite = spriteAtlas.GetArrowTrailTemplate();

            // 设置初始长度
            arrowTrailRenderer.size = new Vector2(trailLength, arrowTrailRenderer.size.y);
            containerTrailRenderer.size = new Vector2(trailLength, containerTrailRenderer.size.y);

            // 设置初始位置
            // 偏移量 = 头部宽度的一半 + 长条自身长度的一半
            float trailOffsetX = (headVisualWidth / 2f) + (trailLength / 2f);
            arrowTrailRenderer.transform.localPosition = new Vector3(trailOffsetX, arrowTrailRenderer.transform.localPosition.y, 0);
            containerTrailRenderer.transform.localPosition = new Vector3(trailOffsetX, containerTrailRenderer.transform.localPosition.y, 0);
        }

        // b. 设置结束帽的初始位置
        if (endCapController != null)
        {
            // 结束帽的位置 = 头部宽度的一半 + 整个长条的长度
            float endCapOffsetX = (headVisualWidth / 2f) + trailLength;
            endCapController.transform.localPosition = new Vector3(endCapOffsetX, endCapController.transform.localPosition.y, 0);
        }
    }

    /// <summary>
    /// 每帧更新音符的位置和状态。
    /// </summary>
    protected override void Update()
    {
        // 如果音符正在被按住，执行特殊的“吸附与消耗”逻辑
        if (isBeingHeld)
        {
            // a. 将音符的根对象（头部判定点）吸附在判定线上
            transform.position = new Vector2(judgmentLineX, transform.position.y);

            // b. 动态计算并更新长条和结束帽的视觉效果
            float songPosition = TimingManager.Instance.SongPosition;
            float elapsedTime = songPosition - noteData.time;
            if (elapsedTime < 0) elapsedTime = 0;

            float originalTotalLength = noteData.duration * scrollSpeed;
            float consumedLength = elapsedTime * scrollSpeed;
            float remainingLength = originalTotalLength - consumedLength;
            if (remainingLength < 0) remainingLength = 0;

            // 同时更新两条长条
            if (arrowTrailRenderer != null && containerTrailRenderer != null)
            {
                // 更新长度
                arrowTrailRenderer.size = new Vector2(remainingLength, arrowTrailRenderer.size.y);
                containerTrailRenderer.size = new Vector2(remainingLength, containerTrailRenderer.size.y);

                // 更新位置，以实现从左到右消失的效果
                // 新的偏移量 = 已消耗长度 + 剩余长度的一半
                float trailOffsetX = consumedLength + (remainingLength / 2f);
                arrowTrailRenderer.transform.localPosition = new Vector3(trailOffsetX, arrowTrailRenderer.transform.localPosition.y, 0);
                containerTrailRenderer.transform.localPosition = new Vector3(trailOffsetX, containerTrailRenderer.transform.localPosition.y, 0);
            }

            // 结束帽的位置始终固定在长条的理论终点
            if (endCapController != null)
            {
                float endCapOffsetX = originalTotalLength;
                endCapController.transform.localPosition = new Vector3(endCapOffsetX, endCapController.transform.localPosition.y, 0);
            }
        }
        else // 如果音符没有被按住
        {
            // 如果 IsJudged 为 true，说明它经历了最终判定（Miss 或 HoldBreak）
            // 此时它应该自我销毁，完成其生命周期
            if (IsJudged)
            {
                Destroy(gameObject);
                return;
            }

            // 如果还未被判定，则执行基类中的正常移动逻辑
            base.Update();
            // 并检查是否超时Miss
            if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
            {
                JudgmentManager.Instance.ProcessMiss(this);
            }
        }
    }

    /// <summary>
    /// 由 JudgmentManager 调用，用于更新音符的“被按住”状态。
    /// </summary>
    public void SetHeldState(bool held)
    {
        isBeingHeld = held;
    }
}