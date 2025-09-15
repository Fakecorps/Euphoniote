// _Project/Scripts/UI/SkillButtonHandler.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 需要这个来处理鼠标悬停事件

public class SkillButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("技能数据")]
    [Tooltip("在Inspector中拖入此按钮代表的SkillData资产文件")]
    public SkillData skillData;

    [Header("UI 引用")]
    [Tooltip("拖入此按钮下用于显示“已选中”的UI元素")]
    public GameObject selectedIndicator;

    // 静态变量，用于跟踪当前选中的按钮
    private static SkillButtonHandler currentSelectedButton;

    private GameReadyController gameReadyController; // 对主控制器的引用

    public void Initialize(GameReadyController controller)
    {
        this.gameReadyController = controller;
        UpdateSelectionState();
    }

    // 当鼠标移入时调用
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameReadyController != null && skillData != null)
        {
            gameReadyController.ShowSkillDescription(skillData);
        }
    }

    // 当鼠标移出时调用
    public void OnPointerExit(PointerEventData eventData)
    {
        if (gameReadyController != null)
        {
            gameReadyController.HideSkillDescription();
        }
    }

    // 当鼠标点击时调用
    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果再次点击已选中的按钮，则取消选择
        if (currentSelectedButton == this)
        {
            currentSelectedButton = null;
            GameSettings.SetSelectedSkill(null);
            Debug.Log("取消选择技能。");
        }
        else // 否则，选择这个新按钮
        {
            if (currentSelectedButton != null)
            {
                // 先取消上一个按钮的选中状态
                currentSelectedButton.UpdateSelectionState();
            }
            currentSelectedButton = this;
            GameSettings.SetSelectedSkill(skillData);
            Debug.Log($"选择了技能: {skillData.skillName}");
        }

        // 更新当前按钮的视觉状态
        UpdateSelectionState();
    }

    /// <summary>
    /// 根据全局设置更新自己的“已选中”指示器的显隐
    /// </summary>
    public void UpdateSelectionState()
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(GameSettings.SelectedSkill == this.skillData);
        }
    }
}