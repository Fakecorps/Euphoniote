// _Project/Scripts/Managers/SkillManager.cs

using UnityEngine;
using System.Collections;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("配置")]
    [Tooltip("在检视面板中拖入一个技能资产文件来进行调试")]
    public SkillData equippedSkill; // 当前装备的技能

    // --- 技能状态查询接口 ---
    public bool IsAutoPerfectActive { get; private set; }
    public bool IsPerfectHealActive { get; private set; }
    public bool IsIgnoreFretsActive { get; private set; }

    // 内部状态
    private bool isSkillReady = true; // 简化处理，技能是否可再次触发
    private float skillCooldown = 5f; // 技能触发后的冷却时间

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void Initialize()
    {
        // 重置所有状态
        IsAutoPerfectActive = false;
        IsPerfectHealActive = false;
        IsIgnoreFretsActive = false;
        isSkillReady = true;

        JudgmentManager.OnNoteJudged += HandleJudgment;
        Debug.Log("SkillManager Initialized and Subscribed.");
    }

    private void OnDisable()
    {
        JudgmentManager.OnNoteJudged -= HandleJudgment;
    }

    /// <summary>
    /// 监听判定事件，检查是否命中了特殊音符
    /// </summary>
    private void HandleJudgment(JudgmentResult result)
    {
        // 检查是否是特殊音符，并且判定成功，并且技能已准备好
        if (result.IsSpecialNote && result.Type < JudgmentType.Miss && isSkillReady)
        {
            TriggerSkill();
        }
    }

    /// <summary>
    /// 触发当前装备的技能
    /// </summary>
    private void TriggerSkill()
    {
        if (equippedSkill == null)
        {
            Debug.LogWarning("没有装备技能，无法触发！");
            return;
        }

        Debug.Log($"<color=lightblue>技能触发: {equippedSkill.skillName}!</color>");
        isSkillReady = false; // 技能进入使用中/冷却中状态

        // 启动一个协程来处理技能的持续时间
        StartCoroutine(SkillCoroutine(equippedSkill));
    }

    private IEnumerator SkillCoroutine(SkillData skill)
    {
        // 1. 激活技能状态
        ActivateSkillEffect(skill.effectType, true);

        // 2. 等待技能持续时间
        yield return new WaitForSeconds(skill.duration);

        // 3. 结束技能状态
        ActivateSkillEffect(skill.effectType, false);
        Debug.Log($"<color=gray>技能结束: {skill.skillName}</color>");

        // 4. 等待冷却时间
        yield return new WaitForSeconds(skillCooldown);
        isSkillReady = true; // 技能冷却完毕，可以再次触发
        Debug.Log("<color=green>技能已准备就绪!</color>");
    }

    /// <summary>
    /// 统一的状态切换方法
    /// </summary>
    private void ActivateSkillEffect(SkillEffectType type, bool isActive)
    {
        switch (type)
        {
            case SkillEffectType.AutoPerfect:
                IsAutoPerfectActive = isActive;
                break;
            case SkillEffectType.PerfectHeal:
                IsPerfectHealActive = isActive;
                break;
            case SkillEffectType.IgnoreFrets:
                IsIgnoreFretsActive = isActive;
                break;
        }
    }

    // --- 效果特定方法，供其他系统调用 ---
    public float GetHealAmount()
    {
        if (IsPerfectHealActive && equippedSkill != null)
        {
            return equippedSkill.healAmount;
        }
        return 0f;
    }
}