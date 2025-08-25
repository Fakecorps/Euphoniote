// _Project/Scripts/Managers/NoteCleanupManager.cs

using UnityEngine;

public class NoteCleanupManager : MonoBehaviour
{
    public static NoteCleanupManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void OnEnable()
    {
        HoldNoteController.OnShrinkAnimationComplete += HandleShrinkComplete;
    }

    void OnDisable()
    {
        HoldNoteController.OnShrinkAnimationComplete -= HandleShrinkComplete;
    }

    private void HandleShrinkComplete(HoldNoteController noteControllerToRecycle)
    {
        if (noteControllerToRecycle != null && noteControllerToRecycle.gameObject.activeInHierarchy)
        {
            // --- 核心修改点 (成功路径) ---
            // 1. 先命令音符清理自己
            noteControllerToRecycle.PrepareForPooling();

            // 2. 再把它返回对象池
            NotePoolManager.Instance.ReturnToPool("HoldNote", noteControllerToRecycle.gameObject);
        }
    }
}