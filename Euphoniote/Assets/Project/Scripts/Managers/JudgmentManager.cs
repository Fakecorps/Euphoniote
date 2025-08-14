// _Project/Scripts/Managers/JudgmentManager.cs (完整最终版)

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class JudgmentManager : MonoBehaviour
{
    // ... (Singleton, 变量, Awake, OnEnable/Disable, Update, 事件订阅等部分代码与你之前版本一致，无需修改) ...
    #region Singleton
    public static JudgmentManager Instance { get; private set; }
    #endregion

    [Header("判定时间窗口 (秒)")]
    public float perfectWindow = 0.05f;
    public float greatWindow = 0.1f;
    public float goodWindow = 0.15f;

    public static event Action<JudgmentResult> OnNoteJudged;

    private static List<BaseNoteController> activeNotes = new List<BaseNoteController>();
    private PlayerInputActions playerInput;
    private HoldNoteController activeHoldNote = null;

    #region Unity生命周期方法
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
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

    void Update()
    {
        if (activeHoldNote != null)
        {
            // 首先，检查音符对象是否还存在。如果因为其他原因（如ProcessMiss）被销毁，应立即清空引用。
            if (activeHoldNote.gameObject == null || !activeHoldNote.gameObject.activeInHierarchy)
            {
                activeHoldNote = null;
                return;
            }

            // 获取音符的标准结束时间
            float noteEndTime = activeHoldNote.noteData.time + activeHoldNote.noteData.duration;
            float songPosition = TimingManager.Instance.SongPosition;

            // 【核心改动】检查是否成功
            // 如果当前歌曲时间已经超过了音符的结束时间，那么就判定为成功。
            if (songPosition >= noteEndTime)
            {
                // 判定为Perfect（或你希望的任何成功判定），并销毁音符
                BroadcastJudgment(JudgmentType.Perfect, activeHoldNote, true);

                // 清空引用，判定流程结束
                activeHoldNote = null;
                return; // 成功后立刻返回，不再执行后续的HoldBreak检查
            }

            // 如果还没到结束时间，再检查玩家是否中途松手 (HoldBreak)
            bool stillHoldingFrets = CheckFrets(activeHoldNote.noteData.requiredFrets);
            bool stillHoldingStrum = (activeHoldNote.noteData.strumType == StrumType.HoldLeft)
                                        ? playerInput.Gameplay.HoldLeft.IsPressed()
                                        : playerInput.Gameplay.HoldRight.IsPressed();

            if (!stillHoldingFrets || !stillHoldingStrum)
            {
                // 玩家在音符结束前松手了，判定为 HoldBreak
                BroadcastJudgment(JudgmentType.HoldBreak, activeHoldNote, false); // HoldBreak不直接销毁
                activeHoldNote.SetHeldState(false); // 通知音符它不再被按住

                // 清空引用，判定流程结束
                activeHoldNote = null;
                return;
            }
        }
    }
    #endregion

    #region 事件订阅与处理
    private void SubscribeToInputEvents()
    {
        playerInput.Gameplay.UpStrum.performed += HandleUpStrum;
        playerInput.Gameplay.DownStrum.performed += HandleDownStrum;
        playerInput.Gameplay.HoldLeft.started += HandleHoldLeftStart;
        playerInput.Gameplay.HoldRight.started += HandleHoldRightStart;
        playerInput.Gameplay.HoldLeft.canceled += HandleHoldLeftEnd;
        playerInput.Gameplay.HoldRight.canceled += HandleHoldRightEnd;
    }

    private void UnsubscribeFromInputEvents()
    {
        playerInput.Gameplay.UpStrum.performed -= HandleUpStrum;
        playerInput.Gameplay.DownStrum.performed -= HandleDownStrum;
        playerInput.Gameplay.HoldLeft.started -= HandleHoldLeftStart;
        playerInput.Gameplay.HoldRight.started -= HandleHoldRightStart;
        playerInput.Gameplay.HoldLeft.canceled -= HandleHoldLeftEnd;
        playerInput.Gameplay.HoldRight.canceled -= HandleHoldRightEnd;
    }

    private void HandleUpStrum(InputAction.CallbackContext context) => CheckTapNote(StrumType.Up);
    private void HandleDownStrum(InputAction.CallbackContext context) => CheckTapNote(StrumType.Down);
    private void HandleHoldLeftStart(InputAction.CallbackContext context) => CheckHoldStart(StrumType.HoldLeft);
    private void HandleHoldRightStart(InputAction.CallbackContext context) => CheckHoldStart(StrumType.HoldRight);
    private void HandleHoldLeftEnd(InputAction.CallbackContext context) => CheckHoldEnd(StrumType.HoldLeft);
    private void HandleHoldRightEnd(InputAction.CallbackContext context) => CheckHoldEnd(StrumType.HoldRight);
    #endregion

    #region 判定逻辑
    private void CheckTapNote(StrumType strumType)
    {
        BaseNoteController noteToJudge = FindClosestNote(strumType, false);
        if (noteToJudge == null) return;

        float timeDiff = Mathf.Abs(noteToJudge.noteData.time - TimingManager.Instance.SongPosition);
        if (timeDiff > goodWindow) return;

        if (!CheckFrets(noteToJudge.noteData.requiredFrets))
        {
            BroadcastJudgment(JudgmentType.Miss, noteToJudge);
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

        float timeDiff = Mathf.Abs(noteToJudge.noteData.time - TimingManager.Instance.SongPosition);
        if (timeDiff > goodWindow) return;

        if (!CheckFrets(noteToJudge.noteData.requiredFrets))
        {
            BroadcastJudgment(JudgmentType.Miss, noteToJudge);
            return;
        }

        if (noteToJudge is HoldNoteController holdNote)
        {
            activeHoldNote = holdNote;
            activeHoldNote.SetHeldState(true);

            if (timeDiff <= perfectWindow) { BroadcastJudgment(JudgmentType.Perfect, holdNote, false); }
            else if (timeDiff <= greatWindow) { BroadcastJudgment(JudgmentType.Great, holdNote, false); }
            else if (timeDiff <= goodWindow) { BroadcastJudgment(JudgmentType.Good, holdNote, false); }
        }
    }

    private void CheckHoldEnd(StrumType strumType) { }

    public void ProcessMiss(BaseNoteController note)
    {
        if (note == null || !note.gameObject.activeSelf || note.IsJudged) return;
        BroadcastJudgment(JudgmentType.Miss, note, false);
    }
    #endregion

    #region 辅助方法
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
                if (timeDiff < minTimeDiff)
                {
                    minTimeDiff = timeDiff;
                    closestNote = note;
                }
            }
        }
        return closestNote;
    }

    private bool CheckFrets(List<FretKey> requiredFrets)
    {
        // ... (这部分代码无需修改)
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
        note.SetJudged();

        OnNoteJudged?.Invoke(new JudgmentResult { Type = type, IsSpecialNote = note.noteData.isSpecial });

        if (destroyNote)
        {
            Destroy(note.gameObject);
        }
    }
    #endregion

    #region 静态公共方法
    public static void RegisterNote(BaseNoteController note)
    {
        if (note != null && !activeNotes.Contains(note))
        {
            activeNotes.Add(note);
        }
    }

    public static void UnregisterNote(BaseNoteController note)
    {
        if (note != null && activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
        }
    }
    #endregion
}