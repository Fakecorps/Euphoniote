// _Project/Scripts/Core/GameSettings.cs

using UnityEngine;

/// <summary>
/// 一个静态类，用于在整个游戏生命周期内存储玩家的设置。
/// </summary>
public static class GameSettings
{
    // --- 音符流速设置 ---
    public static float HiSpeed { get; private set; } = 10.0f; // 默认流速
    private const float MinSpeed = 5.0f;
    private const float MaxSpeed = 30.0f;
    private const float SpeedStep = 0.1f;

    // --- 技能选择设置 ---
    public static SkillData SelectedSkill { get; private set; } = null;

    // --- 公共方法 ---

    public static void SetHiSpeed(float newSpeed)
    {
        HiSpeed = Mathf.Clamp(newSpeed, MinSpeed, MaxSpeed);
    }

    public static void IncreaseHiSpeed()
    {
        SetHiSpeed(HiSpeed + SpeedStep);
    }

    public static void DecreaseHiSpeed()
    {
        SetHiSpeed(HiSpeed - SpeedStep);
    }

    public static void SetSelectedSkill(SkillData skill)
    {
        SelectedSkill = skill;
    }
}