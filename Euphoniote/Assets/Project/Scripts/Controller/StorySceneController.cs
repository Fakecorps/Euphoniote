using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorySceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameFlowManager.CurrentLevelData != null)
        {
            // 判断当前是游戏前还是游戏后
            // 这是一个简化的逻辑，你可能需要一个更复杂的全局状态来判断
            // 比如 GameFlowManager.CurrentGameState
            // 这里我们假设如果游戏刚打完，会有一个状态标记
            //if (/* 游戏刚结束 */)
            //{
            //    DialogueManager.Instance.StartStory(GameFlowManager.CurrentLevelData.storyEnd);
            //}
            //else
            //{
            //    DialogueManager.Instance.StartStory(GameFlowManager.CurrentLevelData.storyStart);
            //}
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
