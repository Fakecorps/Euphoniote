// _Project/Scripts/Managers/StatsManager.cs (集成技能系统版)

using UnityEngine;
using System;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    // --- 分数系统 ---
    public long CurrentScore { get; private set; } = 0; // 使用 long 类型防止分数溢出
    public static event Action<long> OnScoreChanged; // 分数变化时触发的事件

    // --- Combo & Health 系统 (保持不变) ---
    public int CurrentCombo { get; private set; } = 0;
    public int MaxCombo { get; private set; } = 0;
    public float maxHealth = 100f;
    public float CurrentHealth { get; private set; }
    public float penaltyAmount = 5f;

    public static event Action<int> OnComboChanged;
    public static event Action OnComboBroken;
    public static event Action<float, float> OnHealthChanged;
    public static event Action OnGameOver;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void Initialize()
    {
        CurrentCombo = 0;
        MaxCombo = 0;
        CurrentScore = 0;
        CurrentHealth = maxHealth;

        // 触发一次事件，确保UI在游戏开始时显示为初始值 (例如 "0000000")
        OnScoreChanged?.Invoke(CurrentScore);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        JudgmentManager.OnNoteJudged -= HandleJudgment;
        JudgmentManager.OnNoteJudged += HandleJudgment;

        Debug.Log("StatsManager Initialized and Subscribed.");
    }

    private void OnDisable()
    {
        JudgmentManager.OnNoteJudged -= HandleJudgment;
    }

    private void HandleJudgment(JudgmentResult result)
    {
        switch (result.Type)
        {
            case JudgmentType.Perfect:
                AddToScore(JudgmentManager.Instance.perfectScore);
                IncrementCombo();
                break;
            case JudgmentType.Great:
                AddToScore(JudgmentManager.Instance.greatScore);
                IncrementCombo();
                break;
            case JudgmentType.Good:
                AddToScore(JudgmentManager.Instance.goodScore);
                IncrementCombo();
                break;
            case JudgmentType.HoldHead: // <<-- 确保将 HoldHead 添加到增加 Combo 的行列
                IncrementCombo();
                // ... (PerfectHeal的逻辑)
                break;

            case JudgmentType.Miss:
            case JudgmentType.HoldBreak:
                BreakCombo();
                ChangeHealth(-penaltyAmount);
                break;
        }
    }

    public void ChangeHealth(float amount)
    {
        if (CurrentHealth <= 0 && amount < 0) return; // 游戏结束后不再扣血
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (CurrentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("<color=red>GAME OVER!</color>");
        OnGameOver?.Invoke();
    }

    private void IncrementCombo()
    {
        CurrentCombo++;
        if (CurrentCombo > MaxCombo) { MaxCombo = CurrentCombo; }
        OnComboChanged?.Invoke(CurrentCombo);
    }



    public void BreakCombo()
    {
        if (CurrentCombo > 0)
        {
            CurrentCombo = 0;
            OnComboBroken?.Invoke();
        }
    }
    public void AddToCombo(int amount)
    {
        for (int i = 0; i < amount; i++) { IncrementCombo(); }
    }

    private void AddToScore(int amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}