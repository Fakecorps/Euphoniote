// _Project/Scripts/Managers/SkillManager.cs (最终修复版 - 统一状态管理)

using UnityEngine;
using System.Collections;
using System;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public event Action OnSkillTriggered;

    public bool IsAutoPerfectActive { get; private set; }
    public bool IsPerfectHealActive { get; private set; }
    public bool IsIgnoreFretsActive { get; private set; }

    // --- 我们只用这一个变量来管理状态 ---
    private Coroutine activeSkillCoroutine;
    private SkillData equippedSkill;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void Initialize()
    {
        equippedSkill = GameSettings.SelectedSkill;
        if (equippedSkill != null)
        {
            Debug.Log($"<color=cyan>已装备技能: {equippedSkill.skillName}</color>");
        }
        else
        {
            Debug.Log("<color=cyan>没有装备技能。</color>");
        }

        // 停止任何可能残留的旧协程
        if (activeSkillCoroutine != null)
        {
            StopCoroutine(activeSkillCoroutine);
            activeSkillCoroutine = null;
        }

        // 重置所有状态
        IsAutoPerfectActive = false;
        IsPerfectHealActive = false;
        IsIgnoreFretsActive = false;

        JudgmentManager.OnNoteJudged -= HandleJudgment;
        JudgmentManager.OnNoteJudged += HandleJudgment;
        Debug.Log("SkillManager Initialized and Subscribed.");
    }

    private void OnDisable()
    {
        if (JudgmentManager.Instance != null)
        {
            JudgmentManager.OnNoteJudged -= HandleJudgment;
        }
    }

    private void HandleJudgment(JudgmentResult result)
    {
        // 触发条件：是特殊音符且判定成功
        if (result.IsSpecialNote && result.Type < JudgmentType.Miss)
        {
            TriggerSkill();
        }
    }

    private void TriggerSkill()
    {
        if (equippedSkill == null) return;

        // 1. 广播特效/音效事件（总是在最前面）
        OnSkillTriggered?.Invoke();

        // 2. 如果上一个技能效果还在持续，先停掉它的协程
        if (activeSkillCoroutine != null)
        {
            StopCoroutine(activeSkillCoroutine);
        }

        // 3. 启动新的技能效果协程
        activeSkillCoroutine = StartCoroutine(SkillCoroutine(equippedSkill));
    }

    private IEnumerator SkillCoroutine(SkillData skill)
    {
        Debug.Log($"<color=lightblue>技能效果开始: {skill.skillName}!</color>");

        // 1. 激活技能状态
        ActivateSkillEffect(skill.effectType, true);

        // 2. 等待技能持续时间
        yield return new WaitForSeconds(skill.duration);

        // 3. 结束技能状态
        Debug.Log($"<color=gray>技能效果结束: {skill.skillName}</color>");
        ActivateSkillEffect(skill.effectType, false);

        // 4. 清理协程引用，表示技能已结束
        activeSkillCoroutine = null;
    }

    private void ActivateSkillEffect(SkillEffectType type, bool isActive)
    {
        switch (type)
        {
            case SkillEffectType.AutoPerfect: IsAutoPerfectActive = isActive; break;
            case SkillEffectType.PerfectHeal: IsPerfectHealActive = isActive; break;
            case SkillEffectType.IgnoreFrets: IsIgnoreFretsActive = isActive; break;
        }
    }

    public float GetHealAmount()
    {
        if (IsPerfectHealActive && equippedSkill != null)
        {
            return equippedSkill.healAmount;
        }
        return 0f;
    }
}