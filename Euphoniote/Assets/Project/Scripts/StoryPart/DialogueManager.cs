// _Project/Scripts/Story/DialogueManager.cs (纯逻辑最终版 - 增加空值检查)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DialogueManager : MonoBehaviour
{
    public static event Action<string> OnStoryComplete;

    // 内部状态
    private string[] dialogueRows;
    private int currentLine;
    private string dialogueIndex;
    private bool isWaitingForChoice = false;
    private string currentStoryName;

    private StoryUIView currentView; // 核心：对当前场景视图的引用

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterView(StoryUIView view)
    {
        currentView = view;
        Debug.Log("StoryUIView 已注册到 DialogueManager。");
    }

    public void UnregisterView()
    {
        currentView = null;
        Debug.Log("StoryUIView 已从 DialogueManager 注销。");
    }

    void Update()
    {
        // 增加对 currentView 的空值检查，确保只有在剧情场景中才响应输入
        if (currentView == null || !currentView.storyPanel.activeSelf || isWaitingForChoice || Input.GetMouseButtonDown(0) == false)
        {
            return;
        }

        if (currentView.IsTyping())
        {
            currentView.CompleteTyping();
        }
        else
        {
            ShowNextDialogueLine();
        }
    }

    public void StartStory(string storyName)
    {
        // 在开始时也做一次检查
        if (currentView == null)
        {
            Debug.LogError("无法开始剧情：没有已注册的 StoryUIView！请确认您在 2_Story 场景中。");
            // 即使没有视图，也应该调用 EndStory 来推进游戏流程，防止卡死
            EndStory(storyName);
            return;
        }

        currentStoryName = storyName;
        Debug.Log($"开始播放剧情: {storyName}");

        string path = Constants.Constants.Story_Path + storyName;
        TextAsset dialogueDataFile = Resources.Load<TextAsset>(path);

        if (dialogueDataFile == null)
        {
            Debug.LogError($"在 Resources/{path} 找不到剧情文件!");
            EndStory(storyName); // 找不到文件也应该结束流程
            return;
        }
        dialogueRows = dialogueDataFile.text.Split('\n');

        currentLine = 1;
        dialogueIndex = "0";
        isWaitingForChoice = false;

        currentView.ResetView();
        currentView.SetPanelActive(true);

        ShowNextDialogueLine();
    }

    private void ShowNextDialogueLine()
    {
        for (; currentLine < dialogueRows.Length; currentLine++)
        {
            if (string.IsNullOrWhiteSpace(dialogueRows[currentLine])) continue;
            string[] cells = dialogueRows[currentLine].Split(',');

            if (cells.Length > 1 && cells[1] == dialogueIndex)
            {
                switch (cells[0])
                {
                    case "#": ProcessDialogueLine(cells); return;
                    case "&": ProcessChoiceLine(); return;
                    case "END": EndStory(); return;
                }
            }
        }
        EndStory();
    }

    /// <summary>
    /// 结束当前剧情的播放（带参数，用于跳过时）
    /// </summary>
    private void EndStory(string storyName)
    {
        Debug.Log($"剧情 '{storyName}' 播放完毕或被跳过。");
        if (currentView != null) currentView.SetPanelActive(false);
        OnStoryComplete?.Invoke(storyName);
    }

    /// <summary>
    /// 结束当前剧情的播放（无参数，内部使用）
    /// </summary>
    private void EndStory()
    {
        EndStory(this.currentStoryName);
    }

    private void ProcessDialogueLine(string[] cells)
    {
        currentView.UpdateText(cells[2], cells[4]);
        currentView.UpdateBackgroundImage(cells[7]);
        currentView.UpdateBackgroundMusic(cells[6]);
        currentView.UpdateCharacterAction(cells[9], cells[8], 1, cells[10]); // 角色1
        currentView.UpdateCharacterAction(cells[12], cells[11], 2, cells[13]); // 角色2

        dialogueIndex = cells[5];
        currentLine++;
    }

    private void ProcessChoiceLine()
    {
        isWaitingForChoice = true;

        var choices = new List<KeyValuePair<string, string>>();
        for (int i = currentLine; i < dialogueRows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(dialogueRows[i])) continue;
            string[] cells = dialogueRows[i].Split(',');
            if (cells.Length > 5 && cells[0] == "&")
            {
                choices.Add(new KeyValuePair<string, string>(cells[4], cells[5]));
            }
            else
            {
                break;
            }
        }
        currentView.ShowChoices(choices);
    }

    public void MakeChoice(string nextDialogueIndex)
    {
        isWaitingForChoice = false;
        currentView.HideChoices();
        dialogueIndex = nextDialogueIndex;

        currentLine++;
        while (currentLine < dialogueRows.Length)
        {
            if (!string.IsNullOrWhiteSpace(dialogueRows[currentLine]))
            {
                string[] cells = dialogueRows[currentLine].Split(',');
                if (cells.Length > 1 && cells[1] == dialogueIndex && cells[0] == "#")
                {
                    break;
                }
            }
            currentLine++;
        }
        ShowNextDialogueLine();
    }
}