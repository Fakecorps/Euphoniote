// _Project/Scripts/Animations/PlayAnimationOnStart.cs

using UnityEngine;

/// <summary>
/// 一个通用的脚本，用于在游戏对象启动时触发其 Animator 播放一个指定的动画。
/// 需要与 Animator 组件挂载在同一个游戏对象上。
/// </summary>
[RequireComponent(typeof(Animator))] // 确保这个对象上一定有一个Animator组件
public class PlayAnimationOnStart : MonoBehaviour
{
    [Header("动画配置")]
    [Tooltip("要播放的动画状态的名称。请确保这个名称与 Animator Controller 中的状态名完全一致。")]
    public string animationStateName;

    [Tooltip("动画所在的层级索引，通常是 0 (Base Layer)")]
    public int layerIndex = 0;

    // Animator 组件的引用
    private Animator animator;

    // Awake 在对象被创建时立即调用，适合获取组件引用
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // OnEnable 在对象被激活时调用。
    // 这比 Start() 更好，因为它也适用于通过对象池等方式重新激活的对象。
    void OnEnable()
    {
        PlayAnimation();
    }

    /// <summary>
    /// 触发动画播放的核心方法
    /// </summary>
    private void PlayAnimation()
    {
        if (animator == null)
        {
            Debug.LogError("找不到 Animator 组件！", this.gameObject);
            return;
        }

        // 检查动画状态名是否为空
        if (string.IsNullOrEmpty(animationStateName))
        {
            Debug.LogWarning("没有指定要播放的动画状态名称 (animationStateName)。", this.gameObject);
            return;
        }

        // 播放指定的动画状态
        // Play 方法的第三个参数 normalizedTime: 0f 表示从动画的开头开始播放
        animator.Play(animationStateName, layerIndex, 0f);

        Debug.Log($"正在播放动画: '{animationStateName}'", this.gameObject);
    }
}