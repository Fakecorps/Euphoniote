// _Project/Scripts/Managers/JudgmentManager.cs (修正编译错误后的完整最终版)

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using System.Linq;

public class JudgmentManager : MonoBehaviour
{
    public static JudgmentManager Instance { get; private set; }

    [Header("判定时间窗口 (秒)")]
    public float perfectWindow = 0.05f;
    public float greatWindow = 0.1f;
    public float goodWindow = 0.15f;

    [Header("得分配置")]
    public int perfectScore = 1000;
    public int greatScore = 500;
    public int goodScore = 200;

    public static event Action<JudgmentResult> OnNoteJudged;

    private static List<INoteController> activeNotes = new List<INoteController>();
    private PlayerInputActions playerInput;
    private HoldNoteController activeHoldNote = null;

    public class HoldDebugInfo
    {
        public string Stage;
        public float TimeDiff;
        public JudgmentType? HeadJudgment;
        public JudgmentType? FinalJudgment;
    }
    public static event Action<HoldDebugInfo> OnHoldNoteDebug;

    /// <summary>
    /// 广播一个Miss判定事件。主要由音符超时自检时调用。
    /// </summary>
    public void BroadcastMissEvent(INoteController note)
    {
        if (note == null || !note.GetGameObject().activeSelf) return;
        OnNoteJudged?.Invoke(new JudgmentResult { Type = JudgmentType.Miss, IsSpecialNote = note.GetNoteData().isSpecial });
    }

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
        playerInput = new PlayerInputActions();
    }

    public void Initialize()
    {
        playerInput.Gameplay.Enable();
        UnsubscribeFromInputEvents();
        SubscribeToInputEvents();
        Debug.Log("JudgmentManager Initialized and Subscribed.");
    }

    public void DisableInput()
    {
        if (playerInput != null)
        {
            playerInput.Gameplay.Disable();
            UnsubscribeFromInputEvents();
        }
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

    void Update()
    {
        if (activeHoldNote != null)
        {
            if (activeHoldNote.GetGameObject() == null || !activeHoldNote.GetGameObject().activeInHierarchy)
            {
                activeHoldNote = null;
                return;
            }

            bool stillHoldingFrets = CheckFrets(activeHoldNote.GetNoteData().requiredFrets);
            bool stillHoldingStrum = (activeHoldNote.GetNoteData().strumType == StrumType.HoldLeft)
                                     ? playerInput.Gameplay.HoldLeft.IsPressed()
                                     : playerInput.Gameplay.HoldRight.IsPressed();

            if (!stillHoldingFrets || !stillHoldingStrum)
            {
                OnHoldNoteDebug?.Invoke(new HoldDebugInfo { Stage = "Break" });
                BroadcastJudgment(JudgmentType.HoldBreak, activeHoldNote);
                activeHoldNote.SetHeldState(false);
                activeHoldNote = null;
                return;
            }

            float judgmentLineX = NoteSpawner.Instance.judgmentLineX;
            if (activeHoldNote.endCapAnchor != null && activeHoldNote.endCapAnchor.transform.position.x <= judgmentLineX)
            {
                float headTimeDiff = activeHoldNote.HeadTimeDiff;
                JudgmentType finalJudgment = JudgmentType.Good;
                if (headTimeDiff <= perfectWindow) { finalJudgment = JudgmentType.Perfect; }
                else if (headTimeDiff <= greatWindow) { finalJudgment = JudgmentType.Great; }

                OnHoldNoteDebug?.Invoke(new HoldDebugInfo { Stage = "Success", TimeDiff = headTimeDiff, FinalJudgment = finalJudgment });

                BroadcastJudgment(finalJudgment, activeHoldNote);
                activeHoldNote.StartSuccessShrink();
                activeHoldNote = null;
                return;
            }

            int passedBeats = TimingManager.Instance.GetPassedBeats();
            if (passedBeats > 0)
            {
                StatsManager.Instance.AddToCombo(passedBeats);
            }
        }
    }

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
        var noteToJudge = FindClosestTapNote(strumType);
        if (noteToJudge == null) return;

        // --- 修正点：确保变量名正确 ---
        // 我们从接口获取具体的 NoteController 实例
        var noteControllerInstance = noteToJudge.GetGameObject().GetComponent<NoteController>();
        if (noteControllerInstance == null) return;

        float timeDiff = Mathf.Abs(noteControllerInstance.GetNoteData().time - TimingManager.Instance.SongPosition);
        if (timeDiff > goodWindow) return;

        if (!CheckFrets(noteControllerInstance.GetNoteData().requiredFrets))
        {
            BroadcastMissEvent(noteControllerInstance);
            noteControllerInstance.PlayMissAnimationAndRelease();
            return;
        }

        JudgmentType judgmentType;
        if (SkillManager.Instance != null && SkillManager.Instance.IsAutoPerfectActive)
        {
            judgmentType = JudgmentType.Perfect;
        }
        else
        {
            if (timeDiff <= perfectWindow) { judgmentType = JudgmentType.Perfect; }
            else if (timeDiff <= greatWindow) { judgmentType = JudgmentType.Great; }
            else { judgmentType = JudgmentType.Good; }
        }

        OnNoteJudged?.Invoke(new JudgmentResult { Type = judgmentType, IsSpecialNote = noteControllerInstance.GetNoteData().isSpecial });
        noteControllerInstance.ReleaseImmediately();
    }

    private void CheckHoldStart(StrumType strumType)
    {
        if (activeHoldNote != null) return;
        HoldNoteController noteToJudge = FindClosestHoldNote(strumType);
        if (noteToJudge == null) return;

        NoteData data = noteToJudge.GetNoteData();
        float timeDiffRaw = data.time - TimingManager.Instance.SongPosition;
        float timeDiffAbs = Mathf.Abs(timeDiffRaw);

        if (timeDiffAbs > goodWindow) return;

        if (!CheckFrets(data.requiredFrets))
        {
            BroadcastMissEvent(noteToJudge);
            noteToJudge.PlayMissAnimationAndRelease();
            return;
        }

        JudgmentType headJudgment;
        if (SkillManager.Instance != null && SkillManager.Instance.IsAutoPerfectActive) { headJudgment = JudgmentType.Perfect; }
        else
        {
            if (timeDiffAbs <= perfectWindow) headJudgment = JudgmentType.Perfect;
            else if (timeDiffAbs <= greatWindow) headJudgment = JudgmentType.Great;
            else headJudgment = JudgmentType.Good;
        }

        OnHoldNoteDebug?.Invoke(new HoldDebugInfo { Stage = "Start", TimeDiff = timeDiffRaw, HeadJudgment = headJudgment });

        activeHoldNote = noteToJudge;
        activeHoldNote.SetHeadTimeDiff(timeDiffAbs);
        activeHoldNote.SetHeldState(true);
        TimingManager.Instance.ResetBeatTracking();
        BroadcastJudgment(JudgmentType.HoldHead, noteToJudge);
    }

    private INoteController FindClosestTapNote(StrumType strumType)
    {
        return activeNotes
            .OfType<NoteController>()
            .Where(n => !n.IsJudged && n.GetNoteData().strumType == strumType)
            .OrderBy(n => Mathf.Abs(n.GetNoteData().time - TimingManager.Instance.SongPosition))
            .FirstOrDefault();
    }

    private HoldNoteController FindClosestHoldNote(StrumType strumType)
    {
        return activeNotes
            .OfType<HoldNoteController>()
            .Where(n => !n.IsJudged && n.GetNoteData().strumType == strumType)
            .OrderBy(n => Mathf.Abs(n.GetNoteData().time - TimingManager.Instance.SongPosition))
            .FirstOrDefault();
    }

    private bool CheckFrets(List<FretKey> requiredFrets)
    {
        if (SkillManager.Instance != null && SkillManager.Instance.IsIgnoreFretsActive) return true;
        if (requiredFrets == null || requiredFrets.Count == 0) return true;

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

    private void BroadcastJudgment(JudgmentType type, INoteController note)
    {
        if (note == null || !note.GetGameObject().activeSelf || (note.IsJudged && type != JudgmentType.HoldHead)) return;

        if (type != JudgmentType.HoldHead)
        {
            note.SetJudged();
        }

        OnNoteJudged?.Invoke(new JudgmentResult { Type = type, IsSpecialNote = note.GetNoteData().isSpecial });
    }

    public static void RegisterNote(INoteController note)
    {
        if (note != null && !activeNotes.Contains(note)) activeNotes.Add(note);
    }

    public static void UnregisterNote(INoteController note)
    {
        if (note != null && activeNotes.Contains(note)) activeNotes.Remove(note);
    }
}