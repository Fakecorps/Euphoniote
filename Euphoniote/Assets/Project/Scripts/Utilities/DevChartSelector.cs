// _Project/Scripts/Dev/DevChartSelector.cs

using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class DevChartSelector : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject chartSelectPanel; // 拖入 DevChartSelectPanel
    public Transform contentParent;     // 拖入 Scroll View 的 Content 对象
    public GameObject chartButtonPrefab; // 拖入 ChartButton_Prefab

    [Header("管理器引用")]
    public GameManager gameManager; // 拖入场景中的 GameManager

    void Start()
    {
        // 游戏开始时隐藏所有游戏对象，只显示谱面选择UI
        // 这是一个简化的处理，你可能需要禁用 NoteSpawner 等
        Time.timeScale = 0f; // 暂停游戏
        gameManager.enabled = false; // 暂时禁用 GameManager 的 Update

        PopulateChartList();
    }

    /// <summary>
    /// 扫描 Charts 文件夹并生成按钮列表
    /// </summary>
    private void PopulateChartList()
    {
        string chartsPath = Path.Combine(Application.streamingAssetsPath, "Charts");

        if (!Directory.Exists(chartsPath))
        {
            Debug.LogError($"找不到谱面文件夹: {chartsPath}");
            return;
        }

        // 获取所有 .json 文件
        string[] chartFiles = Directory.GetFiles(chartsPath, "*.json");

        // 为每个谱面文件创建一个按钮
        foreach (string filePath in chartFiles)
        {
            string fileName = Path.GetFileName(filePath);

            GameObject buttonObj = Instantiate(chartButtonPrefab, contentParent);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = fileName;

            // 为按钮添加点击事件
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnChartSelected(fileName));
        }
    }

    /// <summary>
    /// 当一个谱面按钮被点击时调用
    /// </summary>
    private void OnChartSelected(string chartFileName)
    {
        Debug.Log($"选择了谱面: {chartFileName}");

        // 1. 隐藏谱面选择UI
        //chartSelectPanel.SetActive(false);

        //// 2. 将选择的谱面文件名传递给 GameManager
        //gameManager.SetChartToPlay(chartFileName, null); // 音乐暂时留空

        //// 3. 重新启用 GameManager 并恢复时间
        //gameManager.enabled = true;
        //Time.timeScale = 1f;

        //// 4. 让 GameManager 自己开始游戏
        //gameManager.StartGameplayForDev();
    }
}