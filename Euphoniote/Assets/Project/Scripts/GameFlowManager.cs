// _Project/Scripts/Managers/GameFlowManager.cs (带跳过剧情功能的简化版)

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static LevelData CurrentLevelData { get; private set; }
    private DialogueManager dialogueManager;

    public static GameFlowManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            dialogueManager = GetComponent<DialogueManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        DialogueManager.OnStoryComplete += HandleStoryComplete;
    }

    private void OnDisable()
    {
        DialogueManager.OnStoryComplete -= HandleStoryComplete;
    }

    /// <summary>
    /// 当玩家在【选关场景】点击一个关卡时调用
    /// </summary>
    public void SelectLevel(LevelData levelData)
    {
        CurrentLevelData = levelData;

        // --- 核心修改点：检查是否有开始剧情 ---
        if (levelData != null && !string.IsNullOrEmpty(levelData.storyStart))
        {
            // 如果有剧情，则加载剧情场景
            SceneManager.LoadScene("2_Story");
        }
        else
        {
            // 如果没有剧情，直接跳到游戏准备界面
            Debug.Log("没有开始剧情，直接跳转到游戏准备界面。");
            SceneManager.LoadScene("3_GameReady");
        }
    }

    private void HandleStoryComplete(string storyName)
    {
        if (CurrentLevelData == null) return;

        if (storyName == CurrentLevelData.storyStart)
        {
            // 游戏前剧情结束 -> 加载游戏准备场景
            SceneManager.LoadScene("3_GameReady");
        }
        else if (storyName == CurrentLevelData.storyEnd)
        {
            // 游戏后剧情结束 -> 加载选关场景
            GoToLevelSelect();
        }
    }

    /// <summary>
    /// 当玩家在【游戏准备场景】点击“开始”按钮时调用
    /// </summary>
    public void StartGameplay()
    {
        SceneManager.LoadScene("4_Gameplay");
    }

    /// <summary>
    /// 当【音游场景】结束时调用
    /// </summary>
    public void GameplayFinished()
    {
        SceneManager.LoadScene("5_Results");
    }

    /// <summary>
    /// 当玩家在【结算场景】点击“继续”按钮时调用
    /// </summary>
    public void ResultsContinue()
    {
        // --- 核心修改点：检查是否有结束剧情 ---
        if (CurrentLevelData != null && !string.IsNullOrEmpty(CurrentLevelData.storyEnd))
        {
            // 如果有剧情，则加载剧情场景
            SceneManager.LoadScene("2_Story");
        }
        else
        {
            // 如果没有剧情，直接跳回选关界面
            Debug.Log("没有结束剧情，直接返回选关界面。");
            GoToLevelSelect();
        }
    }

    /// <summary>
    /// 返回选关界面的统一方法
    /// </summary>
    public void GoToLevelSelect()
    {
        CurrentLevelData = null; // 清理数据
        SceneManager.LoadScene("1_LevelSelect");
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("正在返回主菜单...");
        CurrentLevelData = null;
        SceneManager.LoadScene("0_MainMenu");
    }
}