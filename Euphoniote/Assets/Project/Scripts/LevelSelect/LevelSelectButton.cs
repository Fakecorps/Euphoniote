// _Project/Scripts/UI/LevelSelectButton.cs

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [Header("关卡数据")]
    [Tooltip("在Inspector中拖入这个按钮代表的LevelData资产文件")]
    public LevelData levelDataToLoad;


    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
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
}
