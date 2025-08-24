// _Project/Scripts/Managers/NoteCleanupManager.cs

using UnityEngine;

public class NoteCleanupManager : MonoBehaviour
{
    // 使用单例模式，确保场景中只有一个实例
    public static NoteCleanupManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // 订阅 HoldNoteController 的静态事件
        HoldNoteController.OnShrinkAnimationComplete += HandleShrinkComplete;
        Debug.Log("NoteCleanupManager subscribed to OnShrinkAnimationComplete event.");
    }

    void OnDisable()
    {
        // 在对象被销毁或禁用时，务必取消订阅，防止内存泄漏
        HoldNoteController.OnShrinkAnimationComplete -= HandleShrinkComplete;
        Debug.Log("NoteCleanupManager unsubscribed from OnShrinkAnimationComplete event.");
    }

    /// <summary>
    /// 当收到 HoldNote 完成收缩的事件时，此方法被调用。
    /// </summary>
    /// <param name="noteControllerToDestroy">由事件发送方（HoldNoteController）传入的自身引用</param>
    private void HandleShrinkComplete(HoldNoteController noteControllerToDestroy)
    {
        // 安全检查，确保传入的 note 不是 null
        if (noteControllerToDestroy != null)
        {
            // 由这个外部管理器执行销毁操作
            Destroy(noteControllerToDestroy.gameObject);
            Debug.Log($"Successfully destroyed a HoldNote: {noteControllerToDestroy.name}");
        }
    }
}