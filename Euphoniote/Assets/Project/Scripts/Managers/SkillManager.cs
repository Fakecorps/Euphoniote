// _Project/Scripts/Managers/SkillManager.cs (最终版 - 增加公共查询方法)

using UnityEngine;
using System.Collections;
using System;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public event Action OnSkillTriggered;

    // --- 技能状态查询接口 ---
    public bool IsAutoPerfectActive { get; private set; }
    public bool IsPerfectHealActive { get; private set; }
    public bool IsIgnoreFretsActive { get; private set; }

    // 内部状态
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

        if (activeSkillCoroutine != null)
        {
            StopCoroutine(activeSkillCoroutine);
            activeSkillCoroutine = null;
        }

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
        if (result.IsSpecialNote && result.Type < JudgmentType.Miss)
        {
            TriggerSkill();
        }
    }

    private void TriggerSkill()
    {
        if (equippedSkill == null) return;

        OnSkillTriggered?.Invoke();

        if (activeSkillCoroutine != null)
        {
            StopCoroutine(activeSkillCoroutine);
        }

        activeSkillCoroutine = StartCoroutine(SkillCoroutine(equippedSkill));
    }

    private IEnumerator SkillCoroutine(SkillData skill)
    {
        Debug.Log($"<color=lightblue>技能效果开始: {skill.skillName}!</color>");

        ActivateSkillEffect(skill.effectType, true);
        yield return new WaitForSeconds(skill.duration);

        Debug.Log($"<color=gray>技能效果结束: {skill.skillName}</color>");
        ActivateSkillEffect(skill.effectType, false);

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

    /// <summary>
    /// 返回当前装备的技能数据。
    /// </summary>
    /// <returns>当前装备的 SkillData，如果没有则返回 null。</returns>
    public SkillData GetEquippedSkill()
    {
        return equippedSkill;
    }
}