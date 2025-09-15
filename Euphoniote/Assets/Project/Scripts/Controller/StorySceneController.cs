// _Project/Scripts/Story/StorySceneController.cs (最终版)
using UnityEngine;

public class StorySceneController : MonoBehaviour
{
    void Start()
    {
        if (DialogueManager.Instance == null || GameFlowManager.Instance == null || GameFlowManager.CurrentLevelData == null)
        {
            Debug.LogWarning("无法自动开始剧情：缺少必要的管理器或关卡数据。");
            return;
        }

        string storyToPlay = null;

        // 通过检查 ResultsData 来判断我们是从哪里来的
        // 如果 FinalScore > 0，说明我们刚玩完一局，应该播放结束剧情
        if (ResultsData.FinalScore > 0 && ResultsData.GameWon)
        {
            storyToPlay = GameFlowManager.CurrentLevelData.storyEnd;
        }
        else
        {
            // 否则，我们就是刚从选关/准备界面来的，应该播放开始剧情
            storyToPlay = GameFlowManager.CurrentLevelData.storyStart;
        }

        if (!string.IsNullOrEmpty(storyToPlay))
        {
            DialogueManager.Instance.StartStory(storyToPlay);
        }
        else
        {
            // 如果该播放的剧情为空（没有配置），直接通知 GameFlowManager 跳过
            Debug.Log("当前流程节点没有配置剧情，直接跳过。");
            GameFlowManager.Instance.SkipStory();
        }
    }
}