// _Project/Scripts/Managers/JudgmentManager.cs

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

    public static event Action<JudgmentResult> OnNoteJudged;

    // 使用通用接口 INoteController 来管理所有类型的音符
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
                BroadcastJudgment(JudgmentType.HoldBreak, activeHoldNote, false);
                activeHoldNote.SetHeldState(false);
                activeHoldNote = null;
                return;
            }

            // 从 NoteSpawner 获取判定线的X坐标
            float judgmentLineX = NoteSpawner.Instance.judgmentLineX;
            if (activeHoldNote.endCapAnchor != null && activeHoldNote.endCapAnchor.transform.position.x <= judgmentLineX)
            {
                float headTimeDiff = activeHoldNote.HeadTimeDiff;
                JudgmentType finalJudgment = JudgmentType.Good;
                if (headTimeDiff <= perfectWindow) { finalJudgment = JudgmentType.Perfect; }
                else if (headTimeDiff <= greatWindow) { finalJudgment = JudgmentType.Great; }

                OnHoldNoteDebug?.Invoke(new HoldDebugInfo { Stage = "Success", TimeDiff = headTimeDiff, FinalJudgment = finalJudgment });

                // 2. 广播判定结果，但不销毁Note对象 (destroyNote = false)
                //    这会更新分数、Combo等，但让Note的GameObject继续存在
                BroadcastJudgment(finalJudgment, activeHoldNote, false);

                // 3. 命令NoteController自己开始播放收缩动画
                //    NoteController现在接管了自己的生命周期
                activeHoldNote.StartSuccessShrink();

                // 4. JudgmentManager完成任务，清除对这个Note的引用，不再管理它
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
        BaseNoteController noteToJudge = FindClosestTapNote(strumType);
        if (noteToJudge == null) return;

        float timeDiff = Mathf.Abs(noteToJudge.GetNoteData().time - TimingManager.Instance.SongPosition);
        if (timeDiff > goodWindow) return;

        if (!CheckFrets(noteToJudge.GetNoteData().requiredFrets))
        {
            BroadcastJudgment(JudgmentType.Miss, noteToJudge, false);
            return;
        }

        if (SkillManager.Instance != null && SkillManager.Instance.IsAutoPerfectActive)
        {
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
        HoldNoteController noteToJudge = FindClosestHoldNote(strumType);
        if (noteToJudge == null) return;

        NoteData data = noteToJudge.GetNoteData();
        float timeDiffRaw = data.time - TimingManager.Instance.SongPosition;
        float timeDiffAbs = Mathf.Abs(timeDiffRaw);

        if (timeDiffAbs > goodWindow) return;

        if (!CheckFrets(data.requiredFrets))
        {
            BroadcastJudgment(JudgmentType.Miss, noteToJudge, false);
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

        BroadcastJudgment(JudgmentType.HoldHead, noteToJudge, false);
    }

    public void ProcessMiss(INoteController note)
    {
        if (note == null || !note.GetGameObject().activeSelf || note.IsJudged) return;
        BroadcastJudgment(JudgmentType.Miss, note, false);
    }

    private BaseNoteController FindClosestTapNote(StrumType strumType)
    {
        return activeNotes
            .OfType<BaseNoteController>()
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

    private void BroadcastJudgment(JudgmentType type, INoteController note, bool destroyNote = false)
    {
        if (note == null || !note.GetGameObject().activeSelf || (note.IsJudged && type != JudgmentType.HoldHead)) return;

        if (type != JudgmentType.HoldHead)
        {
            note.SetJudged();
        }

        OnNoteJudged?.Invoke(new JudgmentResult { Type = type, IsSpecialNote = note.GetNoteData().isSpecial });

        if (destroyNote)
        {
            Destroy(note.GetGameObject());
        }
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