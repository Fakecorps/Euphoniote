// _Project/Scripts/UI/LevelSelectButton.cs

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("关卡数据")]
    [Tooltip("在Inspector中拖入这个按钮代表的LevelData资产文件")]
    public LevelData levelDataToLoad;

    [Header("UI组件")]
    [Tooltip("悬停时出现的背景图片（比如btn_xuanzhong.png）")]
    public Image hoverBackground;  // 在Inspector里拖进去

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        hoverBackground = GetComponent<Image>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }

        // 默认隐藏背景
        if (hoverBackground != null)
        {
            hoverBackground.enabled = false;
        }
    }

    private void OnButtonClick()
    {
        if (levelDataToLoad != null)
        {
            Debug.Log($"玩家选择了关卡: {levelDataToLoad.name}");
          
            GameFlowManager.Instance.SelectLevel(levelDataToLoad);
        }
        else
        {
            Debug.LogError("这个关卡按钮没有配置LevelData！", this.gameObject);
        }
    }

    // 鼠标移入时显示背景
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverBackground != null)
        {
            hoverBackground.enabled = true;
        }
    }

    // 鼠标移出时隐藏背景
    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverBackground != null)
        {
            hoverBackground.enabled = false;
        }
    }
}
