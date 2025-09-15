// _Project/Scripts/Feedback/SkillEffectView.cs

using UnityEngine;

/// <summary>
/// 挂载在技能特效 Prefab 上，提供一个公共接口来设置其内部的技能图标。
/// </summary>
public class SkillEffectView : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("在 Prefab 中拖入用于显示技能图标的那个 SpriteRenderer")]
    public SpriteRenderer skillIconRenderer;

    /// <summary>
    /// 设置要显示的技能图标。
    /// </summary>
    /// <param name="iconSprite">要显示的技能图标 Sprite</param>
    public void SetSkillIcon(Sprite iconSprite)
    {
        if (skillIconRenderer != null)
        {
            skillIconRenderer.sprite = iconSprite;
        }
        else
        {
            Debug.LogWarning("SkillEffectView 上的 skillIconRenderer 没有被赋值！", this.gameObject);
        }
    }
}