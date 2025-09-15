// _Project/Scripts/Managers/FeedbackManager.cs (最终版 - 通过GameManager获取引用)

using UnityEngine;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("判定光环特效")]
    public string haloEffectTag1 = "HaloEffect1";
    public string haloEffectTag2 = "HaloEffect2";
    public Transform HaloTransform1;
    public Transform HaloTransform2;

    [Header("技能触发特效")]
    public GameObject skillEffectPrefab;

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
        if (JudgmentManager.Instance != null) { JudgmentManager.OnNoteJudged -= HandleNoteJudged; }
        if (SkillManager.Instance != null) { SkillManager.Instance.OnSkillTriggered -= HandleSkillTriggered; }
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
        SkillData currentSkill = SkillManager.Instance.GetEquippedSkill();
        if (currentSkill != null && currentSkill.icon != null)
        {
            TriggerSkillEffect(currentSkill.icon);
        }
    }

    public void TriggerSkillEffect(Sprite skillIcon)
    {
        if (skillEffectPrefab == null)
        {
            Debug.LogWarning("技能特效的 Prefab 没有在 FeedbackManager 中设置！", this.gameObject);
            return;
        }

        // 通过 GameManager 的静态属性获取当前场景的生成点
        Transform spawnPoint = GameManager.SkillEffectSpawnPoint;

        if (spawnPoint == null)
        {
            Debug.LogError("无法生成技能特效：在当前场景的 GameManager 上找不到 skillEffectSpawnPoint 的引用！请在 4_Gameplay 场景中拖拽赋值。", this.gameObject);
            return;
        }

        StartCoroutine(PlayAndDestroySkillEffect(skillIcon, spawnPoint));
    }

    private IEnumerator PlayAndDestroySkillEffect(Sprite skillIcon, Transform spawnPoint)
    {
        GameObject effectInstance = Instantiate(skillEffectPrefab, spawnPoint.position, spawnPoint.rotation);

        // --- 核心修改：将设置图标的操作放在最前面，并增加安全检查 ---
        // 2. 立即设置图标
        SkillEffectView effectView = effectInstance.GetComponent<SkillEffectView>();
        if (effectView != null)
        {
            effectView.SetSkillIcon(skillIcon);
        }
        else
        {
            Debug.LogWarning("技能特效Prefab上缺少 SkillEffectView 脚本！无法设置动态图标。", effectInstance);
        }

        // 3. 然后再获取 Animator 并播放动画
        Animator animator = effectInstance.GetComponent<Animator>();
        if (animator != null)
        {
            // 确保动画从头播放
            animator.Play(0, -1, 0f);

            // 等待一帧让所有状态更新完毕
            yield return null;

            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }

    public void TriggerHaloEffect1()
    {
        if (string.IsNullOrEmpty(haloEffectTag1) || HaloTransform1 == null) return;
        GameObject haloInstance = NotePoolManager.Instance.GetFromPool(haloEffectTag1);
        if (haloInstance == null) return;
        Vector3 spawnPosition = HaloTransform1.position;
        spawnPosition.z -= 0.1f;
        haloInstance.transform.position = spawnPosition;
        haloInstance.transform.rotation = Quaternion.identity;
    }

    public void TriggerHaloEffect2()
    {
        if (string.IsNullOrEmpty(haloEffectTag2) || HaloTransform2 == null) return;
        GameObject haloInstance = NotePoolManager.Instance.GetFromPool(haloEffectTag2);
        if (haloInstance == null) return;
        Vector3 spawnPosition = HaloTransform2.position;
        spawnPosition.z -= 0.1f;
        haloInstance.transform.position = spawnPosition;
        haloInstance.transform.rotation = Quaternion.identity;
    }
}