// _Project/Scripts/Managers/SkillManager.cs (最终版 - 从GameSettings加载)

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
    private SkillData equippedSkill; // 改为私有，在Initialize时赋值
    private bool isSkillActive = false; // 用于防止技能重复触发

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void Initialize()
    {
        // --- 核心修改：从全局设置中获取玩家选择的技能 ---
        equippedSkill = GameSettings.SelectedSkill;

        if (equippedSkill != null)
        {
            Debug.Log($"<color=cyan>已装备技能: {equippedSkill.skillName}</color>");
        }
        else
        {
            Debug.Log("<color=cyan>没有装备技能。</color>");
        }

        // 重置所有状态
        IsAutoPerfectActive = false;
        IsPerfectHealActive = false;
        IsIgnoreFretsActive = false;
        isSkillActive = false;

        // 确保每次游戏开始都重新订阅事件
        JudgmentManager.OnNoteJudged -= HandleJudgment;
        JudgmentManager.OnNoteJudged += HandleJudgment;

        Debug.Log("SkillManager Initialized and Subscribed.");
    }

    private void OnDisable()
    {
        // 在对象销毁或场景卸载时取消订阅
        if (JudgmentManager.Instance != null)
        {
            JudgmentManager.OnNoteJudged -= HandleJudgment;
        }
    }

    /// <summary>
    /// 监听判定事件，检查是否命中了特殊音符
    /// </summary>
    private void HandleJudgment(JudgmentResult result)
    {
        // 检查是否是特殊音符，并且判定成功，并且技能当前未激活
        if (result.IsSpecialNote && result.Type < JudgmentType.Miss && !isSkillActive)
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
            // 没有装备技能，直接返回，不打印警告，因为这是正常情况
            return;
        }

        if (isSkillActive) return; // 双重保险

        isSkillActive = true; // 技能进入使用中状态

        Debug.Log($"<color=lightblue>技能触发: {equippedSkill.skillName}!</color>");
        OnSkillTriggered?.Invoke();

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

        // 技能结束后，状态重置为非激活，可以再次被触发
        isSkillActive = false;
        Debug.Log("<color=green>技能效果已结束，可再次触发。</color>");
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

    /// <summary>
    /// 供其他系统调用，获取当前技能的回血量
    /// </summary>
    public float GetHealAmount()
    {
        if (IsPerfectHealActive && equippedSkill != null)
        {
            return equippedSkill.healAmount;
        }
        return 0f;
    }
}