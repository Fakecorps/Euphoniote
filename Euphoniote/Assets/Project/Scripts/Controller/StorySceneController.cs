// _Project/Scripts/Story/StorySceneController.cs
using UnityEngine;

public class StorySceneController : MonoBehaviour
{
    // 这个变量用来跟踪我们是否已经开始播放剧情，防止重复触发
    private bool storyStarted = false;

    void Start()
    {
        // 确保 DialogueManager 实例存在
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("场景中找不到 DialogueManager 实例！");
            return;
        }

        // 确保 GameFlowManager 存在并已设置好关卡数据
        if (GameFlowManager.Instance != null && GameFlowManager.CurrentLevelData != null)
        {
            // 从 GameFlowManager 获取当前应该播放的剧情文件名
            // 我们需要一个状态来判断是游戏前还是游戏后
            // 暂时用一个简化的逻辑：检查我们是否刚从结算场景过来
            // (更稳健的方法是 GameFlowManager 中有一个枚举状态)

            string storyToPlay;

            // 一个简单的判断逻辑（未来可以做得更复杂）
            // 假设 GameFlowManager 有一个状态可以查询
            // if (GameFlowManager.Instance.CurrentState == GameState.AfterResults)
            // {
            //    storyToPlay = GameFlowManager.CurrentLevelData.storyEnd;
            // }
            // else
            // {
            //    storyToPlay = GameFlowManager.CurrentLevelData.storyStart;
            // }

            // 为了能立即运行，我们先用一个临时的、不完美的逻辑
            // 这个逻辑假设，如果一个音符被判定过，说明我们刚玩完游戏
            // 你未来需要用一个更好的全局状态来替换它
            if (StatsManager.Instance != null && StatsManager.Instance.MaxCombo > 0)
            {
                storyToPlay = GameFlowManager.CurrentLevelData.storyEnd;
            }
            else
            {
                storyToPlay = GameFlowManager.CurrentLevelData.storyStart;
            }

            if (!string.IsNullOrEmpty(storyToPlay))
            {
                DialogueManager.Instance.StartStory(storyToPlay);
                storyStarted = true;
            }
        }
        else
        {
            Debug.LogWarning("没有找到关卡数据，无法自动开始剧情。");
        }
    }
}