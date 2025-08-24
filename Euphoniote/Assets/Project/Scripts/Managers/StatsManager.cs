// _Project/Scripts/Managers/StatsManager.cs (集成技能系统版)

using UnityEngine;
using System;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

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
        CurrentHealth = maxHealth;

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
            case JudgmentType.Great:
            case JudgmentType.Good:
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
}