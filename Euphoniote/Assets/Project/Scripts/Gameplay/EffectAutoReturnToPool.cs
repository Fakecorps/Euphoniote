// _Project/Scripts/Feedback/EffectAutoReturnToPool.cs

using UnityEngine;

public class EffectAutoReturnToPool : MonoBehaviour
{
    [Tooltip("这个特效在对象池中的标签")]
    public string poolTag = "HaloEffect";

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // OnEnable 在对象从池中取出并激活时被调用
    private void OnEnable()
    {
        // 确保动画从头开始播放
        if (animator != null)
        {
            animator.Play(0, -1, 0f);
            StartCoroutine(CheckAnimationComplete());
        }
    }

    private System.Collections.IEnumerator CheckAnimationComplete()
    {
        // 等待一帧，确保动画状态已更新
        yield return null;

        // 等待当前动画播放完毕
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // 动画结束后，将自己返回对象池
        if (gameObject.activeInHierarchy)
        {
            NotePoolManager.Instance.ReturnToPool(poolTag, gameObject);
        }
    }
}