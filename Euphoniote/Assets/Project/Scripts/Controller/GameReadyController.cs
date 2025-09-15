// _Project/Scripts/UI/GameReadyController.cs (最终版 - 跟随鼠标的Tooltip)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameReadyController : MonoBehaviour
{
    [Header("流速UI引用")]
    public TextMeshProUGUI hiSpeedValueText;
    public Button hiSpeedDecreaseButton;
    public Button hiSpeedIncreaseButton;

    [Header("技能UI引用")]
    public SkillButtonHandler[] skillButtons; // 在Inspector中拖入所有技能按钮
    public GameObject skillDescriptionPanel;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;
    public CanvasGroup skillDescriptionCanvasGroup; // 拖入描述面板上的 Canvas Group
    public RectTransform mainCanvasRectTransform; // 拖入最外层的 Canvas

    [Header("核心控制")]
    public Button startGameplayButton;

    [Header("动画参数")]
    public float tooltipFadeDuration = 0.1f;

    [Header("画布与相机")]
    public Canvas mainCanvas;

    // 内部状态
    private bool isTooltipVisible = false;

    void Start()
    {
        // --- 初始化UI显示 ---
        UpdateHiSpeedUI();
        InitializeSkillButtons();

        // --- 绑定按钮事件 ---
        if (hiSpeedDecreaseButton != null)
            hiSpeedDecreaseButton.onClick.AddListener(OnDecreaseSpeedClicked);
        if (hiSpeedIncreaseButton != null)
            hiSpeedIncreaseButton.onClick.AddListener(OnIncreaseSpeedClicked);
        if (startGameplayButton != null)
            startGameplayButton.onClick.AddListener(OnStartGameplayClicked);

        // 初始隐藏描述面板
        if (skillDescriptionPanel != null)
        {
            skillDescriptionPanel.SetActive(false);
        }
        if (skillDescriptionCanvasGroup != null)
        {
            skillDescriptionCanvasGroup.alpha = 0;
        }
    }

    void Update()
    {
        // 如果 tooltip 可见，则每一帧都更新它的位置以跟随鼠标
        if (isTooltipVisible)
        {
            UpdateTooltipPosition();
        }
    }

    /// <summary>
    /// 初始化所有技能按钮，将自身的引用传递给它们
    /// </summary>
    private void InitializeSkillButtons()
    {
        foreach (var button in skillButtons)
        {
            if (button != null)
            {
                button.Initialize(this);
            }
        }
    }

    /// <summary>
    /// 更新流速显示的文本
    /// </summary>
    private void UpdateHiSpeedUI()
    {
        if (hiSpeedValueText != null)
        {
            // "F1" 格式化数字，保留一位小数
            hiSpeedValueText.text = GameSettings.HiSpeed.ToString("F1");
        }
    }

    // --- 公共方法，供 SkillButtonHandler 调用 ---

    /// <summary>
    /// 显示指定技能的描述面板 (Tooltip)
    /// </summary>
    public void ShowSkillDescription(SkillData skill)
    {
        if (skill == null) return;
        if (skillDescriptionPanel == null) return;

        skillNameText.text = skill.skillName;
        skillDescriptionText.text = skill.description;

        skillDescriptionPanel.SetActive(true);
        isTooltipVisible = true;

        // 播放淡入动画
        StopAllCoroutines(); // 停掉可能正在播放的淡出动画
        StartCoroutine(FadeTooltip(1f));

        // 立即更新一次位置，避免第一帧闪烁在屏幕角落
        UpdateTooltipPosition();
    }

    /// <summary>
    /// 隐藏技能描述面板
    /// </summary>
    public void HideSkillDescription()
    {
        isTooltipVisible = false;

        // 播放淡出动画
        StopAllCoroutines();
        StartCoroutine(FadeTooltip(0f));
    }

    /// <summary>
    /// 核心逻辑：更新 Tooltip 的位置以跟随鼠标
    /// </summary>
    private void UpdateTooltipPosition()
    {
        // 1. 获取鼠标在屏幕上的像素坐标
        Vector2 mousePosition = Input.mousePosition;

        // --- 核心修改 ---
        // 获取 Canvas 关联的相机
        Camera uiCamera = (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : mainCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform,
            mousePosition,
            uiCamera, // <<-- 使用正确的相机
            out Vector2 localPoint
            );

        skillDescriptionPanel.GetComponent<RectTransform>().anchoredPosition = localPoint;
    
    }

    /// <summary>
    /// 一个简单的淡入淡出协程
    /// </summary>
    private IEnumerator FadeTooltip(float targetAlpha)
    {
        if (skillDescriptionCanvasGroup == null) yield break;

        float startAlpha = skillDescriptionCanvasGroup.alpha;
        float timer = 0f;

        while (timer < tooltipFadeDuration)
        {
            timer += Time.deltaTime;
            skillDescriptionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / tooltipFadeDuration);
            yield return null;
        }

        skillDescriptionCanvasGroup.alpha = targetAlpha;

        // 如果是淡出动画，在动画结束后再禁用对象，以节省性能
        if (targetAlpha == 0)
        {
            skillDescriptionPanel.SetActive(false);
        }
    }

    // --- 按钮点击事件处理 ---
    private void OnDecreaseSpeedClicked()
    {
        GameSettings.DecreaseHiSpeed();
        UpdateHiSpeedUI();
    }

    private void OnIncreaseSpeedClicked()
    {
        GameSettings.IncreaseHiSpeed();
        UpdateHiSpeedUI();
    }

    private void OnStartGameplayClicked()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.StartGameplay();
        }
    }
}