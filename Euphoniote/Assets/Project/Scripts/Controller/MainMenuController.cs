// _Project/Scripts/UI/MainMenuController.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI 引用")]
    public Button startGameButton;
    public Button quitGameButton;

    [Header("场景设置")]
    [Tooltip("点击“开始游戏”后要加载的引导场景的名称")]
    public string bootstrapSceneName = "0_Bootstrap";

    void Start()
    {
        // 为按钮绑定点击事件
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(QuitGame);
        }
    }

    public void StartGame()
    {
        // 加载 Bootstrap 场景。
        // Bootstrap 场景会负责加载持久化管理器和真正的第一个游戏场景（选关界面）。
        Debug.Log($"开始游戏，正在加载引导场景: {bootstrapSceneName}...");
        SceneManager.LoadScene(bootstrapSceneName);
    }
    public void QuitGame()
    {
        Debug.Log("正在退出游戏...");
        Application.Quit();

        // 在编辑器模式下，Application.Quit() 可能无效，添加以下代码以便在编辑器中测试
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}