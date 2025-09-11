// _Project/Scripts/UI/GameReadyController.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // 需要List

public class GameReadyController : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI hiSpeedValueText;
    public Button hiSpeedDecreaseButton;
    public Button hiSpeedIncreaseButton;
    public TMP_Dropdown skillDropdown;
    public Button startGameplayButton;

    public static GameReadyController Instance;

    [Header("可用技能列表")]
    [Tooltip("在Inspector中拖入所有玩家可选的SkillData资产文件")]
    public List<SkillData> availableSkills;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // --- 初始化UI显示 ---
        UpdateHiSpeedUI();
        PopulateSkillDropdown();

        // --- 绑定按钮事件 ---
        hiSpeedDecreaseButton.onClick.AddListener(OnDecreaseSpeedClicked);
        hiSpeedIncreaseButton.onClick.AddListener(OnIncreaseSpeedClicked);
        skillDropdown.onValueChanged.AddListener(OnSkillSelected);
        startGameplayButton.onClick.AddListener(OnStartGameplayClicked);
    }

    /// <summary>
    /// 更新流速显示的文本
    /// </summary>
    private void UpdateHiSpeedUI()
    {
        // "F1" 格式化数字，保留一位小数
        hiSpeedValueText.text = GameSettings.HiSpeed.ToString("F1");
    }

    /// <summary>
    /// 填充技能选择的下拉菜单
    /// </summary>
    private void PopulateSkillDropdown()
    {
        skillDropdown.ClearOptions();

        List<string> skillNames = new List<string>();
        skillNames.Add("无技能 (No Skill)"); // 第一个选项是“无”

        foreach (var skill in availableSkills)
        {
            skillNames.Add(skill.skillName);
        }

        skillDropdown.AddOptions(skillNames);

        // 根据GameSettings中的值，设置下拉菜单的初始选中项
        if (GameSettings.SelectedSkill == null)
        {
            skillDropdown.value = 0;
        }
        else
        {
            int index = availableSkills.IndexOf(GameSettings.SelectedSkill) + 1;
            skillDropdown.value = index;
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

    /// <summary>
    /// 当下拉菜单选项变化时调用
    /// </summary>
    /// <param name="index">新选中项的索引</param>
    private void OnSkillSelected(int index)
    {
        if (index == 0) // "无技能" 选项
        {
            GameSettings.SetSelectedSkill(null);
            Debug.Log("选择了：无技能");
        }
        else
        {
            // 索引需要减1，因为我们的列表中第一个是“无”
            SkillData selectedSkill = availableSkills[index - 1];
            GameSettings.SetSelectedSkill(selectedSkill);
            Debug.Log($"选择了技能: {selectedSkill.skillName}");
        }
    }

    private void OnStartGameplayClicked()
    {
        // 调用全局流程管理器，进入游戏场景
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.StartGameplay();
        }
    }
}