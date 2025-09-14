// _Project/Scripts/Managers/FeedbackManager.cs (最终版 - 世界空间特效)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("判定光环特效")]
    public string haloEffectTag1 = "HaloEffect1";
    public string haloEffectTag2 = "HaloEffect2";
    public Transform HaloTransform1;
    public Transform HaloTransform2;

    [Header("技能触发特效")]
    [Tooltip("在Inspector中直接拖入技能特效的 Prefab")]
    public GameObject skillEffectPrefab; // 直接引用世界空间的 Prefab

    [Header("特效生成位置")]
    [Tooltip("技能特效生成的世界坐标参考点 (例如，场景中心的一个空对象)")]
    public Transform skillEffectSpawnPoint;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void Initialize()
    {
        JudgmentManager.OnNoteJudged += HandleNoteJudged;
        SkillManager.Instance.OnSkillTriggered += HandleSkillTriggered;
        Debug.Log("FeedbackManager Initialized and subscribed to events.");
    }

    private void OnDisable()
    {
        if (JudgmentManager.Instance != null)
        {
            JudgmentManager.OnNoteJudged -= HandleNoteJudged;
        }
        if (SkillManager.Instance != null)

        {
            SkillManager.Instance.OnSkillTriggered -= HandleSkillTriggered;
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

    private void HandleSkillTriggered()
    {
        TriggerSkillEffect();
    }

    // --- 核心修改：技能特效的独立【世界空间】生成与销毁逻辑 ---
    public void TriggerSkillEffect()
    {
        if (skillEffectPrefab == null || skillEffectSpawnPoint == null)
        {
            Debug.LogWarning("技能特效的 Prefab 或生成点没有在 FeedbackManager 中设置！", this.gameObject);
            return;
        }

        StartCoroutine(PlayAndDestroySkillEffect());
    }

    private IEnumerator PlayAndDestroySkillEffect()
    {
        // 1. 【实例化】 直接在指定的世界坐标点创建实例，不设置父对象
        GameObject effectInstance = Instantiate(skillEffectPrefab, skillEffectSpawnPoint.position, skillEffectSpawnPoint.rotation);

        // 2. 触发并等待动画
        Animator animator = effectInstance.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(0, -1, 0f);

            yield return null;

            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);
        }
        else
        {
            Debug.LogWarning($"技能特效 {skillEffectPrefab.name} 上没有找到 Animator 组件，将只显示0.5秒。", effectInstance);
            yield return new WaitForSeconds(0.5f);
        }

        // 3. 【销毁】 动画播放完毕后，销毁这个实例
        Destroy(effectInstance);
    }


    // --- 您原有的光环特效方法保持不变，它们已经是世界空间的了 ---
    public void TriggerHaloEffect1()
    {
        if (string.IsNullOrEmpty(haloEffectTag1) || HaloTransform1 == null)
        {
            Debug.LogWarning("左侧光环特效配置不完整。", this.gameObject);
            return;
        }

        GameObject haloInstance = NotePoolManager.Instance.GetFromPool(haloEffectTag1);
        if (haloInstance == null) return;

        Vector3 spawnPosition = HaloTransform1.position;
        spawnPosition.z -= 0.1f;
        haloInstance.transform.position = spawnPosition;
        haloInstance.transform.rotation = Quaternion.identity;
    }

    public void TriggerHaloEffect2()
    {
        if (string.IsNullOrEmpty(haloEffectTag2) || HaloTransform2 == null)
        {
            Debug.LogWarning("右侧光环特效配置不完整。", this.gameObject);
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