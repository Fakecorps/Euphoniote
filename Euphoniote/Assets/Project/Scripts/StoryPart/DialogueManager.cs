// _Project/Scripts/Story/DialogueManager.cs (重构版)

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Constants; // 假设你的Constants命名空间是这个
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System; // 需要这个来使用 Action

public class DialogueManager : MonoBehaviour
{
    public static event Action<string> OnStoryComplete; // <<-- 核心事件：剧情结束时触发

    [Header("UI 核心组件")]
    public GameObject storyPanel;
    public GameObject dialogueUI; // 包含名字、对话框、按钮等的UI容器
    public TMP_Text speakerName;
    public TMP_Text speakingContent;
    public GameObject nameTable;
    public TextTyping textTyping;

    [Header("场景与角色")]
    public Image charactor1;
    public Image charactor2;
    public Image bgImg;

    [Header("选项系统")]
    public GameObject choicePanel;
    public GameObject optionButtonPrefab;
    public Transform buttonGroup;

    [Header("音频")]
    public AudioSource backgroundMusic;

    // 内部状态
    private string[] dialogueRows;
    private int currentLine;
    private string dialogueIndex;
    private bool isWaitingForChoice = false;
    private string currentStoryName;

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        storyPanel.SetActive(false); // 初始时隐藏整个剧情面板
    }

    void Update()
    {
        // 只有在剧情播放中且没有等待玩家做选择时，才响应点击
        if (storyPanel.activeSelf && !isWaitingForChoice && Input.GetMouseButtonDown(0))
        {
            if (textTyping.IsTyping())
            {
                textTyping.CompleteLine();
            }
            else
            {
                ShowNextDialogueLine();
            }
        }
    }

    /// <summary>
    /// 外部调用的入口：开始播放一段剧情
    /// </summary>
    public void StartStory(string storyName)
    {
        currentStoryName = storyName;
        Debug.Log($"开始播放剧情: {storyName}");

        // 加载剧情文件
        string path = Constants.Constants.Story_Path + storyName;
        TextAsset dialogueDataFile = Resources.Load<TextAsset>(path);
        if (dialogueDataFile == null)
        {
            Debug.LogError($"找不到剧情文件: {path}");
            EndStory(); // 如果找不到文件，直接结束
            return;
        }
        dialogueRows = dialogueDataFile.text.Split('\n');

        // 初始化状态
        currentLine = 1; // 从第一行有效数据开始
        dialogueIndex = "0";
        isWaitingForChoice = false;

        // 清理旧的角色和背景
        charactor1.gameObject.SetActive(false);
        charactor2.gameObject.SetActive(false);
        bgImg.gameObject.SetActive(false);
        backgroundMusic.Stop();

        storyPanel.SetActive(true);
        ShowNextDialogueLine();
    }

    private void ShowNextDialogueLine()
    {
        // 循环直到找到匹配当前 dialogueIndex 的行，或者剧情结束
        for (; currentLine < dialogueRows.Length; currentLine++)
        {
            if (string.IsNullOrWhiteSpace(dialogueRows[currentLine])) continue; // 跳过空行

            string[] cells = dialogueRows[currentLine].Split(',');

            if (cells.Length > 1 && cells[1] == dialogueIndex)
            {
                // 根据行类型处理
                switch (cells[0])
                {
                    case "#": // 对话行
                        ProcessDialogueLine(cells);
                        return; // 找到并处理完一行后，等待下一次输入
                    case "&": // 选项行
                        ProcessChoiceLine();
                        return;
                    case "END": // 结束标记
                        EndStory();
                        return;
                }
            }
        }

        // 如果循环走完都没找到匹配的行，也意味着当前分支结束
        Debug.LogWarning($"剧情分支 {dialogueIndex} 结束，或找不到匹配行。");
        EndStory();
    }

    private void EndStory()
    {
        Debug.Log($"剧情 '{currentStoryName}' 播放完毕。");
        storyPanel.SetActive(false);
        OnStoryComplete?.Invoke(currentStoryName); // 广播剧情结束事件
    }

    private void ProcessDialogueLine(string[] cells)
    {
        // 更新文本
        UpdateText(cells[2], cells[4]);

        // 更新背景音乐和图片
        UpdateBackgroundImage(cells[7]);
        UpdateBackgroundMusic(cells[6]);

        // 更新角色
        UpdateCharacterAction(cells[9], cells[8], charactor1, cells[10]);
        UpdateCharacterAction(cells[12], cells[11], charactor2, cells[13]);

        // 更新下一个对话索引
        dialogueIndex = cells[5];
        currentLine++;
    }

    private void ProcessChoiceLine()
    {
        isWaitingForChoice = true;
        choicePanel.SetActive(true);

        // 清理旧的选项按钮
        foreach (Transform child in buttonGroup)
        {
            Destroy(child.gameObject);
        }

        // 生成新的选项
        for (int i = currentLine; i < dialogueRows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(dialogueRows[i])) continue;
            string[] cells = dialogueRows[i].Split(',');
            if (cells[0] != "&") break; // 遇到非选项行就停止

            GameObject buttonObj = Instantiate(optionButtonPrefab, buttonGroup);
            buttonObj.GetComponentInChildren<TMP_Text>().text = cells[4];

            string nextIndex = cells[5]; // 捕获变量以避免闭包问题
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnOptionClicked(nextIndex));
        }
    }

    private void OnOptionClicked(string nextDialogueIndex)
    {
        isWaitingForChoice = false;
        choicePanel.SetActive(false);
        dialogueIndex = nextDialogueIndex;

        // 找到选项对应的下一行开始处理
        currentLine++; // 跳过当前选项行
        while (!dialogueRows[currentLine].Split(',')[0].Equals("#"))
        {
            currentLine++;
        }
        ShowNextDialogueLine();
    }

    // --- 以下是各种更新UI的辅助方法 (从旧代码中提取和简化) ---
    private void UpdateText(string name, string content)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            nameTable.SetActive(false);
        }
        else
        {
            nameTable.SetActive(true);
            speakerName.text = name;
        }
        textTyping.StartTyping(content);
    }

    private void UpdateBackgroundImage(string bgName)
    {
        if (string.IsNullOrWhiteSpace(bgName)) return;
        string path = Constants.Constants.BgImg_Path + bgName;
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            bgImg.sprite = sprite;
            bgImg.gameObject.SetActive(true);
        }
    }

    private void UpdateBackgroundMusic(string musicName)
    {
        if (string.IsNullOrWhiteSpace(musicName)) return;
        if (musicName == "暂停")
        {
            backgroundMusic.Stop();
            return;
        }
        string path = Constants.Constants.BgMusic_Path + musicName;
        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip != null)
        {
            if (backgroundMusic.clip != clip)
            {
                backgroundMusic.clip = clip;
                backgroundMusic.loop = true;
                backgroundMusic.Play();
            }
        }
    }

    private void UpdateCharacterAction(string action, string path, Image character, string xPos)
    {
        if (string.IsNullOrWhiteSpace(action)) return;
        string characterPath = Constants.Constants.Charactor_Path + path;

        switch (action)
        {
            case "load":
            case "appearAt":
                Sprite sprite = Resources.Load<Sprite>(characterPath);
                if (sprite != null)
                {
                    character.sprite = sprite;
                    character.gameObject.SetActive(true);
                    var position = new Vector2(float.Parse(xPos), character.rectTransform.anchoredPosition.y);
                    character.rectTransform.anchoredPosition = position;
                    if (action == "appearAt")
                    {
                        character.DOFade(1, Constants.Constants.DURATION_TIME).From(0);
                    }
                }
                break;
            case "disappear":
                character.DOFade(0, Constants.Constants.DURATION_TIME).OnComplete(() => character.gameObject.SetActive(false));
                break;
            case "moveTo":
                character.rectTransform.DOAnchorPosX(float.Parse(xPos), Constants.Constants.DURATION_TIME);
                break;
            case "change":
                Sprite newSprite = Resources.Load<Sprite>(characterPath);
                if (newSprite != null) character.sprite = newSprite;
                break;
        }
    }
}