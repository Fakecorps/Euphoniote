// _Project/Scripts/Managers/GameFlowManager.cs

using UnityEngine;
using UnityEngine.SceneManagement; // 如果需要切换场景

public class GameFlowManager : MonoBehaviour
{
    // --- 引用所有需要的UI面板和管理器 ---
    [Header("UI 面板")]
    public GameObject levelSelectPanel;
    public GameObject gameReadyPanel;
    public GameObject gameplayUIPanel;
    public GameObject resultsPanel;

    [Header("核心管理器")]
    public DialogueManager dialogueManager;
    public GameManager GameManager; // 这里假设您音游的GameManager叫这个

    private string selectedLevelStory_Start; // 选关后，游戏开始前的剧情文件名
    private string selectedLevelStory_End;   // 结算后，返回主界面前的剧情文件名
    private string selectedLevelChart;       // 选中的谱面文件名
    private AudioClip selectedLevelMusic;    // 选中的音乐

    public static GameFlowManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        DialogueManager.OnStoryComplete += HandleStoryComplete;
    }

    private void OnDisable()
    {
        DialogueManager.OnStoryComplete -= HandleStoryComplete;
    }

    void Start()
    {
        // 游戏启动时，只显示选关界面
        ShowPanel(levelSelectPanel);
    }

    /// <summary>
    /// 当玩家在选关界面点击一个关卡时调用
    /// </summary>
    public void OnLevelSelected(string storyStart, string storyEnd, string chart, AudioClip music)
    {
        selectedLevelStory_Start = storyStart;
        selectedLevelStory_End = storyEnd;
        selectedLevelChart = chart;
        selectedLevelMusic = music;

        // 流程第一步：开始播放游戏前剧情
        ShowPanel(null); // 隐藏所有面板
        dialogueManager.StartStory(selectedLevelStory_Start);
    }

    /// <summary>
    /// 当剧情播放完毕时，由DialogueManager的事件调用
    /// </summary>
    private void HandleStoryComplete(string storyName)
    {
        // 判断当前是哪段剧情结束了
        if (storyName == selectedLevelStory_Start)
        {
            // 游戏前剧情结束 -> 进入游戏准备界面
            ShowPanel(gameReadyPanel);
        }
        else if (storyName == selectedLevelStory_End)
        {
            // 游戏后剧情结束 -> 回到选关界面
            ShowPanel(levelSelectPanel);
        }
    }

    /// <summary>
    /// 当玩家在游戏准备界面（选技能等）点击“开始”按钮时调用
    /// </summary>
    public void OnGameReady()
    {
        // 流程第三步：开始音游部分
        ShowPanel(gameplayUIPanel);
        // 调用您音游的GameManager来启动游戏
        // GameManager音游主管理器.StartGameplay(selectedLevelChart, selectedLevelMusic);
        Debug.Log($"调用音游管理器，开始游戏，谱面: {selectedLevelChart}");
    }

    /// <summary>
    /// 当音游部分结束时（例如，歌曲播放完或玩家失败），由您的音游GameManager调用
    /// </summary>
    public void OnGameplayFinished( /* 可以传入分数、评级等结算数据 */)
    {
        // 流程第四步：显示结算界面
        ShowPanel(resultsPanel);
        // 在这里可以用结算数据更新resultsPanel的内容
    }

    /// <summary>
    /// 当玩家在结算界面点击“继续”按钮时调用
    /// </summary>
    public void OnResultsContinue()
    {
        // 流程第五步：播放游戏后剧情
        ShowPanel(null);
        dialogueManager.StartStory(selectedLevelStory_End);
    }


    /// <summary>
    /// 一个辅助方法，用于方便地切换显示的UI面板
    /// </summary>
    private void ShowPanel(GameObject panelToShow)
    {
        levelSelectPanel.SetActive(panelToShow == levelSelectPanel);
        gameReadyPanel.SetActive(panelToShow == gameReadyPanel);
        gameplayUIPanel.SetActive(panelToShow == gameplayUIPanel);
        resultsPanel.SetActive(panelToShow == resultsPanel);
        // 剧情面板由DialogueManager自己控制，这里不用管
    }
}