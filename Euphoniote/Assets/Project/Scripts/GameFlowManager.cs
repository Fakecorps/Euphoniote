// _Project/Scripts/Managers/GameFlowManager.cs (多场景最终版 - 增加失败流程)

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

    public void SelectLevel(LevelData levelData)
    {
        CurrentLevelData = levelData;

        if (levelData != null && !string.IsNullOrEmpty(levelData.storyStart))
        {
            SceneManager.LoadScene("2_Story");
        }
        else
        {
            Debug.Log("没有开始剧情，直接跳转到游戏准备界面。");
            SceneManager.LoadScene("3_GameReady");
        }
    }

    private void HandleStoryComplete(string storyName)
    {
        if (CurrentLevelData == null) return;

        if (storyName == CurrentLevelData.storyStart)
        {
            SceneManager.LoadScene("3_GameReady");
        }
        else if (storyName == CurrentLevelData.storyEnd)
        {
            GoToLevelSelect();
        }
    }

    public void StartGameplay()
    {
        SceneManager.LoadScene("4_Gameplay");
    }

    public void GameplayFinished()
    {
        SceneManager.LoadScene("5_Results");
    }

    public void ResultsContinue()
    {
        // 根据游戏结果决定流程
        if (ResultsData.GameWon)
        {
            // 如果游戏成功，检查是否有结束剧情
            if (CurrentLevelData != null && !string.IsNullOrEmpty(CurrentLevelData.storyEnd))
            {
                Debug.Log("游戏成功，正在加载游戏后剧情...");
                SceneManager.LoadScene("2_Story");
            }
            else
            {
                Debug.Log("游戏成功，但没有配置游戏后剧情，直接返回选关界面。");
                GoToLevelSelect();
            }
        }
        else // 如果游戏失败
        {
            Debug.Log("游戏失败，直接返回选关界面。");
            GoToLevelSelect();
        }
    }

    public void GoToLevelSelect()
    {
        CurrentLevelData = null;
        // 清理一下结算数据，为下一局做准备
        ResultsData.FinalScore = 0;
        SceneManager.LoadScene("1_LevelSelect");
    }

    /// <summary>
    /// 由 StorySceneController 调用，用于跳过空的剧情节点
    /// </summary>
    public void SkipStory()
    {
        // 根据当前游戏状态判断应该跳过的是哪个剧情
        if (ResultsData.FinalScore > 0) // 粗略判断刚玩完一局
        {
            HandleStoryComplete(CurrentLevelData?.storyEnd);
        }
        else
        {
            HandleStoryComplete(CurrentLevelData?.storyStart);
        }
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("正在返回主菜单...");
        CurrentLevelData = null;
        SceneManager.LoadScene("0_MainMenu");
    }
}