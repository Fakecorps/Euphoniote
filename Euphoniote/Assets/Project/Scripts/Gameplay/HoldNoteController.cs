// _Project/Scripts/Gameplay/HoldNoteController.cs

using UnityEngine;

public class HoldNoteController : BaseNoteController
{
    [Header("Hold Note 特定组件")]
    [Tooltip("长条的SpriteRenderer，请确保它的Draw Mode设置为Sliced")]
    public SpriteRenderer holdTrailRenderer;

    // 内部状态，追踪音符是否正被玩家按住
    private bool isBeingHeld = false;

    /// <summary>
    /// 初始化HoldNote，在基类初始化的基础上，额外设置长条的长度。
    /// </summary>
    /// <param name="data">音符的数据</param>
    public override void Initialize(NoteData data)
    {
        // 首先调用父类的Initialize方法，完成公共部分的初始化（如箭头、按键字母等）
        base.Initialize(data);

        // 检查长条的SpriteRenderer引用是否存在
        if (holdTrailRenderer != null)
        {
            // 确保Draw Mode被正确设置为Sliced，否则九宫格拉伸会失效
            if (holdTrailRenderer.drawMode != SpriteDrawMode.Sliced)
            {
                Debug.LogWarning("HoldTrailRenderer的Draw Mode不是Sliced！长条可能无法正确显示。", this.gameObject);
            }

            // 根据音符的持续时间和滚动速度，计算长条在游戏世界中的宽度
            float trailWidth = data.duration * scrollSpeed;

            // 设置Sliced Sprite的宽度，Unity会自动处理拉伸
            // 我们只修改X轴的size，Y轴保持其原始高度
            holdTrailRenderer.size = new Vector2(trailWidth, holdTrailRenderer.size.y);
        }
        else
        {
            Debug.LogError("HoldNoteController上的 holdTrailRenderer 引用为空！请在HoldNote Prefab中为该字段赋值。", this.gameObject);
        }
    }

    /// <summary>
    /// 每帧更新音符状态
    /// </summary>
    protected override void Update()
    {
        // 调用父类的Update方法，处理音符的移动
        base.Update();

        // 只有当音符没有被按住时，才需要检查它是否飘过判定区
        if (!isBeingHeld)
        {
            // 检查音符是否已经完全飘过判定窗口而未被按下
            if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
            {
                // 通知JudgmentManager这是一个Miss
                JudgmentManager.Instance.ProcessMiss(this);
            }
        }
    }

    /// <summary>
    /// 由JudgmentManager调用，用于更新音符被按住的状态。
    /// </summary>
    /// <param name="held">是否被按住</param>
    public void SetHeldState(bool held)
    {
        isBeingHeld = held;

        // 在这里可以添加视觉反馈，比如让音符高亮或改变颜色来表示它正在被成功按住
        if (isBeingHeld)
        {

        }
        else
        {

        }
    }
}