// _Project/Scripts/Story/StoryUIView.cs

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Constants;
using System.Collections.Generic;

public class StoryUIView : MonoBehaviour
{
    [Header("UI 核心组件")]
    public GameObject storyPanel;
    public GameObject dialogueUI;
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

    // 在 Awake 中向 DialogueManager 注册自己
    private void Awake()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.RegisterView(this);
        }
        else
        {
            Debug.LogError("无法注册StoryUIView，因为DialogueManager实例不存在！");
        }
    }

    // 在 OnDestroy 中取消注册
    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.UnregisterView();
        }
    }

    // --- 以下是所有供 DialogueManager 调用的公共方法 ---

    public void SetPanelActive(bool isActive)
    {
        storyPanel.SetActive(isActive);
    }

    public void StartTyping(string text)
    {
        textTyping.StartTyping(text);
    }

    public bool IsTyping()
    {
        return textTyping.IsTyping();
    }

    public void CompleteTyping()
    {
        textTyping.CompleteLine();
    }

    public void UpdateText(string name, string content)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim() == " ")
        {
            nameTable.SetActive(false);
        }
        else
        {
            nameTable.SetActive(true);
            speakerName.text = name;
        }
        speakingContent.text = content; // 先设置完整文本，再由TextTyping控制显示
    }

    public void UpdateBackgroundImage(string bgName)
    {
        if (string.IsNullOrWhiteSpace(bgName)) return;
        string path = Constants.Constants.BgImg_Path + bgName.Trim();
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            bgImg.sprite = sprite;
            bgImg.gameObject.SetActive(true);
        }
        else Debug.LogWarning($"在 Resources/{path} 找不到背景图片。");
    }

    public void UpdateBackgroundMusic(string musicName)
    {
        if (string.IsNullOrWhiteSpace(musicName)) return;
        musicName = musicName.Trim();
        if (musicName == "暂停")
        {
            backgroundMusic.Stop();
            return;
        }
        string path = Constants.Constants.BgMusic_Path + musicName;
        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip != null)
        {
            if (backgroundMusic.clip != clip || !backgroundMusic.isPlaying)
            {
                backgroundMusic.clip = clip;
                backgroundMusic.loop = true;
                backgroundMusic.Play();
            }
        }
        else Debug.LogWarning($"在 Resources/{path} 找不到背景音乐。");
    }

    public void UpdateCharacterAction(string action, string path, int characterSlot, string xPos)
    {
        Image character = (characterSlot == 1) ? charactor1 : charactor2;
        if (character == null || string.IsNullOrWhiteSpace(action)) return;

        action = action.Trim();
        path = path.Trim();
        xPos = xPos.Trim();

        string characterPath = Constants.Constants.Charactor_Path + path;
        Sprite sprite = null;
        if (!string.IsNullOrWhiteSpace(path)) sprite = Resources.Load<Sprite>(characterPath);

        switch (action)
        {
            case "load":
            case "appearAt":
                if (sprite != null)
                {
                    character.sprite = sprite;
                    character.color = new Color(1, 1, 1, 1);
                    character.gameObject.SetActive(true);
                    var position = new Vector2(float.Parse(xPos), character.rectTransform.anchoredPosition.y);
                    character.rectTransform.anchoredPosition = position;
                    if (action == "appearAt") character.DOFade(1, Constants.Constants.DURATION_TIME).From(0);
                }
                else if (!string.IsNullOrWhiteSpace(path)) Debug.LogWarning($"在 Resources/{characterPath} 找不到角色图片。");
                break;
            case "disappear":
                character.DOFade(0, Constants.Constants.DURATION_TIME).OnComplete(() => character.gameObject.SetActive(false));
                break;
            case "moveTo":
                character.rectTransform.DOAnchorPosX(float.Parse(xPos), Constants.Constants.DURATION_TIME);
                break;
            case "change":
                if (sprite != null) character.sprite = sprite;
                else if (!string.IsNullOrWhiteSpace(path)) Debug.LogWarning($"在 Resources/{characterPath} 找不到角色图片。");
                break;
        }
    }

    public void ShowChoices(List<KeyValuePair<string, string>> choices)
    {
        choicePanel.SetActive(true);
        foreach (Transform child in buttonGroup)
        {
            Destroy(child.gameObject);
        }

        foreach (var choice in choices)
        {
            GameObject buttonObj = Instantiate(optionButtonPrefab, buttonGroup);
            buttonObj.GetComponentInChildren<TMP_Text>().text = choice.Key; // 选项文本
            string nextIndex = choice.Value; // 目标ID
            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {

                DialogueManager.Instance.MakeChoice(nextIndex);
            });
        }
    }

    public void HideChoices()
    {
        choicePanel.SetActive(false);
    }

    public void ResetView()
    {
        if (charactor1 != null) charactor1.gameObject.SetActive(false);
        if (charactor2 != null) charactor2.gameObject.SetActive(false);
        if (bgImg != null) bgImg.gameObject.SetActive(false);
        if (backgroundMusic != null) backgroundMusic.Stop();
        HideChoices();
    }
}