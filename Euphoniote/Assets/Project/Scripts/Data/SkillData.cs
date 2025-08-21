// _Project/Scripts/Data/SkillData.cs

using UnityEngine;

// 定义技能效果的类型
public enum SkillEffectType
{
    AutoPerfect,
    PerfectHeal,
    IgnoreFrets
}

// [CreateAssetMenu] 属性让我们可以在右键菜单中直接创建这个类型的资产文件
[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("基本信息")]
    public string skillName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon; // 技能图标 (未来UI用)

    [Header("核心参数")]
    public SkillEffectType effectType; // 这个技能属于哪种效果
    public float duration; // 技能持续时间（秒）

    [Header("效果特定参数")]
    [Tooltip("仅用于 PerfectHeal: 每次Perfect回复的血量")]
    public float healAmount;
}