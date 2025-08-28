// _Project/Scripts/Managers/SoundFeedbackManager.cs (最终修复版)

using UnityEngine;
using System.Collections.Generic;

public class SoundFeedbackManager : MonoBehaviour
{
    public static SoundFeedbackManager Instance { get; private set; }

    [Header("音效源")]
    [Tooltip("用于播放大部分一次性音效的AudioSource")]
    public AudioSource sfxSource;

    [Header("判定音效")]
    public AudioClip perfectSound;
    public AudioClip greatSound;
    public AudioClip goodSound;

    [Header("输入与UI音效")]
    public AudioClip missStrumSound; // 空扫音效
    public AudioClip pauseSound;     // 暂停音效

    [Header("技能音效")]
    public AudioClip skillActivateSound; // 技能发动音效

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void Initialize()
    {
        // 订阅所有需要的事件
        JudgmentManager.OnNoteJudged += HandleNoteJudged;
        JudgmentManager.OnMissStrum += HandleMissStrum;
        SkillManager.Instance.OnSkillTriggered += HandleSkillTriggered;
        PauseManager.OnPauseStateChanged += HandlePauseStateChanged;

        Debug.Log("SoundFeedbackManager Initialized and subscribed to events.");
    }

    private void OnDisable()
    {
        // 在对象销毁或禁用时取消订阅
        if (JudgmentManager.Instance != null)
        {
            JudgmentManager.OnNoteJudged -= HandleNoteJudged;
            JudgmentManager.OnMissStrum -= HandleMissStrum;
        }
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnSkillTriggered -= HandleSkillTriggered;
        }
        if (PauseManager.Instance != null)
        {
            PauseManager.OnPauseStateChanged -= HandlePauseStateChanged;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 处理判定事件，播放对应的音效
    /// </summary>
    private void HandleNoteJudged(JudgmentResult result)
    {
        switch (result.Type)
        {
            case JudgmentType.Perfect:
                PlaySound(perfectSound);
                break;
            case JudgmentType.Great:
                PlaySound(greatSound);
                break;

            case JudgmentType.Good:
                PlaySound(goodSound);
                break;
        }
    }

    /// <summary>
    /// 处理空扫事件
    /// </summary>
    private void HandleMissStrum()
    {
        Debug.Log("Miss strum!");
        PlaySound(missStrumSound);
    }

    /// <summary>
    /// 处理暂停状态变化
    /// </summary>
    private void HandlePauseStateChanged(bool isPaused)
    {
        if (isPaused) // 只在打开暂停菜单时播放
        {
            PlaySound(pauseSound);
        }
    }

    /// <summary>
    /// 处理技能触发
    /// </summary>
    private void HandleSkillTriggered()
    {
        PlaySound(skillActivateSound);
    }
}