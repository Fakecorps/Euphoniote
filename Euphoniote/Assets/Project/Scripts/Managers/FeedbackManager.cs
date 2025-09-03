// _Project/Scripts/Managers/FeedbackManager.cs (修正坐标转换后的最终版)

using UnityEngine;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("判定光环特效")]
    [Tooltip("判定光环特效在对象池中的标签")]
    public string haloEffectTag1 = "HaloEffect1";
    public string haloEffectTag2 = "HaloEffect2";


    public Transform HaloTransform1; 
    public Transform HaloTransform2;


    private Camera mainCamera;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // 在游戏开始时获取主相机的引用，比每帧都调用Camera.main性能更好
        mainCamera = Camera.main;
    }

    public void Initialize()
    {
        JudgmentManager.OnNoteJudged += HandleNoteJudged;
        Debug.Log("FeedbackManager Initialized and subscribed to JudgmentManager events.");
    }

    private void OnDisable()
    {
        if (JudgmentManager.Instance != null)
        {
            JudgmentManager.OnNoteJudged -= HandleNoteJudged;
        }
    }

    private void HandleNoteJudged(JudgmentResult result)
    {
        if (result.Type < JudgmentType.Miss)
        {
            TriggerHaloEffect1();
            TriggerHaloEffect2();
        }
    }

    /// <summary>
    /// 触发判定光环特效的核心逻辑
    /// </summary>
    public void TriggerHaloEffect1()
    {

        // 安全检查
        if (string.IsNullOrEmpty(haloEffectTag1) || HaloTransform1 == null)
        {
            Debug.LogWarning("判定光环特效的配置不完整，无法生成。请检查FeedbackManager上的引用。", this.gameObject);
            return;
        }

        // 1. 从对象池获取一个特效实例
        GameObject haloInstance = NotePoolManager.Instance.GetFromPool(haloEffectTag1);
        if (haloInstance == null) return;

        Vector3 spawnPosition = HaloTransform1.position;
        spawnPosition.z -= 0.1f; 

        haloInstance.transform.position = spawnPosition;

        haloInstance.transform.rotation = Quaternion.identity;
    }

    public void TriggerHaloEffect2()
    {

        // 安全检查
        if (string.IsNullOrEmpty(haloEffectTag2) || HaloTransform2 == null)
        {
            Debug.LogWarning("判定光环特效的配置不完整，无法生成。请检查FeedbackManager上的引用。", this.gameObject);
            return;
        }

        GameObject haloInstance = NotePoolManager.Instance.GetFromPool(haloEffectTag2);
        if (haloInstance == null) return;

        Vector3 spawnPosition = HaloTransform2.position;
        spawnPosition.z -= 0.1f;

        haloInstance.transform.position = spawnPosition;

        haloInstance.transform.rotation = Quaternion.identity;
    }
}