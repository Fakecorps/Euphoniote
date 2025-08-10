// _Project/Scripts/Managers/JudgmentManager.cs (修正版)

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class JudgmentManager : MonoBehaviour
{
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
            bool stillHoldingFrets = CheckFrets(activeHoldNote.noteData.requiredFrets);
            bool stillHoldingStrum = (activeHoldNote.noteData.strumType == StrumType.HoldLeft)
                                     ? playerInput.Gameplay.HoldLeft.IsPressed()
                                     : playerInput.Gameplay.HoldRight.IsPressed();

            if (!stillHoldingFrets || !stillHoldingStrum)
            {
                BroadcastJudgment(JudgmentType.HoldBreak, activeHoldNote);
                activeHoldNote = null;
                return;
            }

            float endTime = activeHoldNote.noteData.time + activeHoldNote.noteData.duration;
            if (TimingManager.Instance.SongPosition >= endTime)
            {
                BroadcastJudgment(JudgmentType.Perfect, activeHoldNote, true);
                activeHoldNote = null;
            }
        }
    }
    #endregion

    #region 事件订阅与处理 (这是主要修改部分)
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

    // 将匿名函数改为命名函数
    private void HandleUpStrum(InputAction.CallbackContext context) => CheckTapNote(StrumType.Up);
    private void HandleDownStrum(InputAction.CallbackContext context) => CheckTapNote(StrumType.Down);
    private void HandleHoldLeftStart(InputAction.CallbackContext context) => CheckHoldStart(StrumType.HoldLeft);
    private void HandleHoldRightStart(InputAction.CallbackContext context) => CheckHoldStart(StrumType.HoldRight);
    private void HandleHoldLeftEnd(InputAction.CallbackContext context) => CheckHoldEnd(StrumType.HoldLeft);
    private void HandleHoldRightEnd(InputAction.CallbackContext context) => CheckHoldEnd(StrumType.HoldRight);
    #endregion

    #region 判定逻辑 (这部分与你提供的代码一致，无需修改)
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

        if (timeDiff <= perfectWindow) { BroadcastJudgment(JudgmentType.Perfect, noteToJudge); }
        else if (timeDiff <= greatWindow) { BroadcastJudgment(JudgmentType.Great, noteToJudge); }
        else if (timeDiff <= goodWindow) { BroadcastJudgment(JudgmentType.Good, noteToJudge); }
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
        if (note == null || !note.gameObject.activeSelf) return;
        BroadcastJudgment(JudgmentType.Miss, note);
    }
    #endregion

    #region 辅助方法 (这部分与你提供的代码一致，无需修改)
    private BaseNoteController FindClosestNote(StrumType strumType, bool isHoldNote)
    {
        BaseNoteController closestNote = null;
        float minTimeDiff = float.MaxValue;
        foreach (var note in activeNotes)
        {
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

    private void BroadcastJudgment(JudgmentType type, BaseNoteController note, bool destroyNote = true)
    {
        if (note == null || !note.gameObject.activeSelf) return;
        OnNoteJudged?.Invoke(new JudgmentResult { Type = type, IsSpecialNote = note.noteData.isSpecial });
        if (destroyNote)
        {
            Destroy(note.gameObject);
        }
    }
    #endregion

    #region 静态公共方法 (这部分与你提供的代码一致，无需修改)
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