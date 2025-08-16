// _Project/Scripts/Managers/StatsManager.cs (完整版)

using UnityEngine;
using System;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    // --- 核心数值 ---
    public int CurrentCombo { get; private set; } = 0;
    public int MaxCombo { get; private set; } = 0;
    // public int score = 0; // Score and health can be added later
    // public float health = 100f;

    // --- 事件 ---
    // 当Combo数值发生变化时广播 (传递新的Combo值)
    public static event Action<int> OnComboChanged;
    // 当Combo中断时广播
    public static event Action OnComboBroken;


    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        // 订阅判定事件
        JudgmentManager.OnNoteJudged += HandleJudgment;
    }

    private void OnDisable()
    {
        // 取消订阅
        JudgmentManager.OnNoteJudged -= HandleJudgment;
    }

    public void Initialize()
    {
        CurrentCombo = 0;
        MaxCombo = 0;
        // 订阅事件
        JudgmentManager.OnNoteJudged += HandleJudgment;
        Debug.Log("StatsManager Initialized and Subscribed.");
    }

    private void HandleJudgment(JudgmentResult result)
    {
        switch (result.Type)
        {
            // 这些判定会增加Combo
            case JudgmentType.Perfect:
            case JudgmentType.Great:
            case JudgmentType.Good:
                IncrementCombo();
                break;

            // 这些判定会中断Combo
            case JudgmentType.Miss:
            case JudgmentType.HoldBreak:
                BreakCombo();
                break;
        }
    }
    private void IncrementCombo()//提供一个Combo接口
    {
        CurrentCombo++;
        if (CurrentCombo > MaxCombo)
        {
            MaxCombo = CurrentCombo;
        }
        OnComboChanged?.Invoke(CurrentCombo);
    }

    private void BreakCombo()
    {
        Debug.Log("Combo Broken");
        // 只有在有combo的时候断连才需要广播事件
        if (CurrentCombo > 0)
        {
            CurrentCombo = 0;
            // 广播Combo中断事件
            OnComboBroken?.Invoke();
        }
    }

    public void AddToCombo(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            IncrementCombo();
        }
    }
}