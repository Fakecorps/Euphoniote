// _Project/Scripts/Managers/JudgmentManager.cs (完整最终版)

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class JudgmentManager : MonoBehaviour
{
    #region Singleton & Core Variables
    public static JudgmentManager Instance { get; private set; }

    [Header("判定时间窗口 (秒)")]
    public float perfectWindow = 0.05f;
    public float greatWindow = 0.1f;
    public float goodWindow = 0.15f;

    public static event Action<JudgmentResult> OnNoteJudged;

    private static List<BaseNoteController> activeNotes = new List<BaseNoteController>();
    private PlayerInputActions playerInput;
    private HoldNoteController activeHoldNote = null;
    #endregion

    #region Debug Events
    // 定义一个包含详细信息的事件参数类，专门用于调试
    public class HoldDebugInfo
    {
        public string Stage; // "Start", "Break", "Success"
        public float TimeDiff;
        public JudgmentType? HeadJudgment;
        public JudgmentType? FinalJudgment;
    }
    // 定义调试事件
    public static event Action<HoldDebugInfo> OnHoldNoteDebug;
    #endregion

    #region Unity Lifecycle & Input Subscription
    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        playerInput = new PlayerInputActions();
    }

    void OnEnable()
    {
        playerInput.Gameplay.Enable();
        SubscribeToInputEvents();
    }

    void OnDisable()
    {
        playerInput.Gameplay.Disable();
        UnsubscribeFromInputEvents();
    }

    private void SubscribeToInputEvents()
    {
        playerInput.Gameplay.UpStrum.performed += HandleTapNoteInput;
        playerInput.Gameplay.DownStrum.performed += HandleTapNoteInput;
        playerInput.Gameplay.HoldLeft.started += HandleHoldNoteInput;
        playerInput.Gameplay.HoldRight.started += HandleHoldNoteInput;
    }

    private void UnsubscribeFromInputEvents()
    {
        playerInput.Gameplay.UpStrum.performed -= HandleTapNoteInput;
        playerInput.Gameplay.DownStrum.performed -= HandleTapNoteInput;
        playerInput.Gameplay.HoldLeft.started -= HandleHoldNoteInput;
        playerInput.Gameplay.HoldRight.started -= HandleHoldNoteInput;
    }
    #endregion
    public void Initialize()
    {
        // 订阅事件
        SubscribeToInputEvents();
        Debug.Log("JudgmentManager Initialized and Subscribed.");
    }

    void Update()
    {
        if (activeHoldNote != null)
        {
            if (activeHoldNote.gameObject == null || !activeHoldNote.gameObject.activeInHierarchy)
            {
                activeHoldNote = null;
                return;
            }

            // 【核心逻辑重构】判定顺序调整

            // 1. 优先检查玩家是否中途松手 (HoldBreak)
            bool stillHoldingFrets = CheckFrets(activeHoldNote.noteData.requiredFrets);
            bool stillHoldingStrum = (activeHoldNote.noteData.strumType == StrumType.HoldLeft)
                                        ? playerInput.Gameplay.HoldLeft.IsPressed()
                                        : playerInput.Gameplay.HoldRight.IsPressed();

            if (!stillHoldingFrets || !stillHoldingStrum)
            {
                StatsManager.Instance.BreakCombo();
                OnHoldNoteDebug?.Invoke(new HoldDebugInfo { Stage = "Break" });
                BroadcastJudgment(JudgmentType.HoldBreak, activeHoldNote, false);
                activeHoldNote.SetHeldState(false);
                activeHoldNote = null;
                return; 
            }

            // 2. 如果没有中断，再检查是否成功
            float songPosition = TimingManager.Instance.SongPosition;
            float noteEndTime = activeHoldNote.noteData.time + activeHoldNote.noteData.duration;

            if (songPosition >= noteEndTime)
            {
                float headTimeDiff = activeHoldNote.HeadTimeDiff;
                JudgmentType finalJudgment = JudgmentType.Good;
                if (headTimeDiff <= perfectWindow) { finalJudgment = JudgmentType.Perfect; }
                else if (headTimeDiff <= greatWindow) { finalJudgment = JudgmentType.Great; }

                OnHoldNoteDebug?.Invoke(new HoldDebugInfo { Stage = "Success", TimeDiff = headTimeDiff, FinalJudgment = finalJudgment });
                BroadcastJudgment(finalJudgment, activeHoldNote, true);
                activeHoldNote.SetHeldState(false);
                activeHoldNote = null;
                return;
            }

            // 3. 如果既没有中断也没有成功，则处理节拍 Combo
            int passedBeats = TimingManager.Instance.GetPassedBeats();
            if (passedBeats > 0)
            {
                StatsManager.Instance.AddToCombo(passedBeats);
            }
        }
    }

    #region Input Handlers & Judgment Logic
    private void HandleTapNoteInput(InputAction.CallbackContext context)
    {
        StrumType type = context.action == playerInput.Gameplay.UpStrum ? StrumType.Up : StrumType.Down;
        CheckTapNote(type);
    }

    private void HandleHoldNoteInput(InputAction.CallbackContext context)
    {
        StrumType type = context.action == playerInput.Gameplay.HoldLeft ? StrumType.HoldLeft : StrumType.HoldRight;
        CheckHoldStart(type);
    }

    private void CheckTapNote(StrumType strumType)
    {
        BaseNoteController noteToJudge = FindClosestNote(strumType, false);
        if (noteToJudge == null) return;

        float timeDiff = Mathf.Abs(noteToJudge.noteData.time - TimingManager.Instance.SongPosition);
        if (timeDiff > goodWindow) return;

        if (!CheckFrets(noteToJudge.noteData.requiredFrets))
        {
            BroadcastJudgment(JudgmentType.Miss, noteToJudge, false);
            return;
        }

        if (SkillManager.Instance != null && SkillManager.Instance.IsAutoPerfectActive)
        {
            // 如果技能激活，所有非Miss的判定都强制为Perfect
            BroadcastJudgment(JudgmentType.Perfect, noteToJudge, false);
            return;
        }

        if (timeDiff <= perfectWindow) { BroadcastJudgment(JudgmentType.Perfect, noteToJudge, false); }
        else if (timeDiff <= greatWindow) { BroadcastJudgment(JudgmentType.Great, noteToJudge, false); }
        else if (timeDiff <= goodWindow) { BroadcastJudgment(JudgmentType.Good, noteToJudge, false); }
    }

    private void CheckHoldStart(StrumType strumType)
    {
        if (activeHoldNote != null) return;
        BaseNoteController noteToJudge = FindClosestNote(strumType, true);
        if (noteToJudge == null) return;

        float timeDiffRaw = noteToJudge.noteData.time - TimingManager.Instance.SongPosition;
        float timeDiffAbs = Mathf.Abs(timeDiffRaw);

        if (timeDiffAbs > goodWindow) return;

        if (!CheckFrets(noteToJudge.noteData.requiredFrets))
        {
            BroadcastJudgment(JudgmentType.Miss, noteToJudge, false);
            return;
        }

        if (noteToJudge is HoldNoteController holdNote)
        {
            OnHoldNoteDebug?.Invoke(new HoldDebugInfo { Stage = "Start", TimeDiff = timeDiffRaw, HeadJudgment = JudgmentType.Good });

            activeHoldNote = holdNote;
            activeHoldNote.SetHeadTimeDiff(timeDiffAbs);
            activeHoldNote.SetHeldState(true);
            TimingManager.Instance.ResetBeatTracking();

            JudgmentType headJudgment;

            // --- 【技能系统接入】 ---
            if (SkillManager.Instance != null && SkillManager.Instance.IsAutoPerfectActive)
            {
                headJudgment = JudgmentType.Perfect;
            }
            else // 正常判定
            {
                if (timeDiffAbs <= perfectWindow) headJudgment = JudgmentType.Perfect;
                else if (timeDiffAbs <= greatWindow) headJudgment = JudgmentType.Great;
                else headJudgment = JudgmentType.Good;
            }
            // --- 技能系统结束 ---

            BroadcastJudgment(headJudgment, holdNote, false);
        }
    }

    public void ProcessMiss(BaseNoteController note)
    {
        if (note == null || !note.gameObject.activeSelf || note.IsJudged) return;
        BroadcastJudgment(JudgmentType.Miss, note, false);
    }
    #endregion

    #region Helper & Static Methods
    private BaseNoteController FindClosestNote(StrumType strumType, bool isHoldNote)
    {
        BaseNoteController closestNote = null;
        float minTimeDiff = float.MaxValue;
        foreach (var note in activeNotes)
        {
            if (note.IsJudged) continue;
            bool typeMatch = (isHoldNote) ? (note.noteData.duration > 0) : (note.noteData.duration == 0);
            if (note.noteData.strumType == strumType && typeMatch)
            {
                float timeDiff = Mathf.Abs(note.noteData.time - TimingManager.Instance.SongPosition);
                if (timeDiff < minTimeDiff) { minTimeDiff = timeDiff; closestNote = note; }
            }
        }
        return closestNote;
    }

    private bool CheckFrets(List<FretKey> requiredFrets)
    {
        if (SkillManager.Instance != null && SkillManager.Instance.IsIgnoreFretsActive)
        {
            return true;
        }
        if (requiredFrets == null || requiredFrets.Count == 0)
        {
            return true; // 直接判定左手条件成功
        }

        foreach (var fret in requiredFrets)
        {

            switch (fret)
            {
                case FretKey.Q: if (!playerInput.Gameplay.FretQ.IsPressed()) return false; break;
                case FretKey.W: if (!playerInput.Gameplay.FretW.IsPressed()) return false; break;
                case FretKey.E: if (!playerInput.Gameplay.FretE.IsPressed()) return false; break;
                case FretKey.A: if (!playerInput.Gameplay.FretA.IsPressed()) return false; break;
                case FretKey.S: if (!playerInput.Gameplay.FretS.IsPressed()) return false; break;
                case FretKey.D: if (!playerInput.Gameplay.FretD.IsPressed()) return false; break;
            }
        }
        return true;
    }

    private void BroadcastJudgment(JudgmentType type, BaseNoteController note, bool destroyNote = false)
    {
        if (note == null || !note.gameObject.activeSelf || note.IsJudged) return;
        Debug.Log($"<color=magenta>[EVENT SENT] JudgmentManager 正在广播: {type}</color>", note.gameObject);
        note.SetJudged();
        OnNoteJudged?.Invoke(new JudgmentResult { Type = type, IsSpecialNote = note.noteData.isSpecial });
        if (destroyNote)
        {
            Destroy(note.gameObject);
        }
    }

    public static void RegisterNote(BaseNoteController note)
    {
        if (note != null && !activeNotes.Contains(note)) activeNotes.Add(note);
    }

    public static void UnregisterNote(BaseNoteController note)
    {
        if (note != null && activeNotes.Contains(note)) activeNotes.Remove(note);
    }
    #endregion
}