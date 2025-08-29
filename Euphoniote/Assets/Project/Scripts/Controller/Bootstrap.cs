// _Project/Scripts/Core/Bootstrap.cs

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 引导脚本，游戏启动时运行。
/// 它的唯一职责是确保全局管理器对象持久存在，并加载第一个游戏场景。
/// </summary>
public class Bootstrap : MonoBehaviour
{
    // 在Inspector中设置要加载的第一个场景的名称，这样更灵活
    [Header("配置")]
    [Tooltip("游戏启动后要加载的第一个场景的名称")]
    public string firstSceneToLoad = "1_LevelSelect";

    // Awake在场景中所有对象的Start方法之前被调用，是执行初始设置的理想位置
    private void Awake()
    {
        // 这一行是整个脚本的核心！
        // DontDestroyOnLoad告诉Unity：
        // “当加载新场景时，不要销毁挂载了这个脚本的GameObject。”
        // 因为我们所有的全局管理器都挂载在同一个GameObject上，所以它们都会被保留下来。
        DontDestroyOnLoad(this.gameObject);

        // 第一次启动游戏时，直接加载第一个场景
        // 如果我们是从其他场景返回到引导场景（正常情况下不应该发生），
        // 为了避免重复加载，可以加一个简单的检查。
        // 但在标准流程中，这个脚本只会执行一次。
        Debug.Log("引导程序启动，正在加载第一个场景...");
        SceneManager.LoadScene(firstSceneToLoad);
    }
}