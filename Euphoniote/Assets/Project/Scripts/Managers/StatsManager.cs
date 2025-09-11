// _Project/Scripts/Managers/StatsManager.cs (最终版 - 增加判定计数)

using UnityEngine;
using System;
using System.Collections.Generic; // 需要这个命名空间来使用 Dictionary

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    // --- 核心数据 ---
    public long CurrentScore { get; private set; }
    public int CurrentCombo { get; private set; }
    public int MaxCombo { get; private set; }
    public float CurrentHealth { get; private set; }

    // --- 新增：判定计数器 ---
    // 用于存储每种判定的发生次数
    public Dictionary<JudgmentType, int> JudgmentCounts { get; private set; }

    [Header("配置")]
    public float maxHealth = 100f;
    public float penaltyAmount = 5f;

    // --- 事件 ---
    public static event Action<long> OnScoreChanged;
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
        CurrentScore = 0;
        CurrentCombo = 0;
        MaxCombo = 0;
        CurrentHealth = maxHealth;

        // 初始化计数器字典，将所有类型的计数都清零
        JudgmentCounts = new Dictionary<JudgmentType, int>
        {
            { JudgmentType.Perfect, 0 },
            { JudgmentType.Great, 0 },
            { JudgmentType.Good, 0 },
            { JudgmentType.Miss, 0 },
            { JudgmentType.HoldBreak, 0 },
            { JudgmentType.HoldHead, 0 } // HoldHead也计数，可用于调试或特殊统计
        };

        // 触发一次事件，确保UI在游戏开始时显示为初始值
        OnScoreChanged?.Invoke(CurrentScore);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        // 订阅判定事件
        JudgmentManager.OnNoteJudged -= HandleJudgment;
        JudgmentManager.OnNoteJudged += HandleJudgment;

        Debug.Log("StatsManager Initialized and Subscribed.");
    }

    private void OnDisable()
    {
        // 在对象销毁或场景卸载时取消订阅
        if (JudgmentManager.Instance != null)
        {
            JudgmentManager.OnNoteJudged -= HandleJudgment;
        }
    }

    private void HandleJudgment(JudgmentResult result)
    {
        // 无论何种判定，都先增加其计数
        if (JudgmentCounts.ContainsKey(result.Type))
        {
            JudgmentCounts[result.Type]++;
        }

        // 根据判定类型执行不同的逻辑（加分、加Combo、扣血等）
        switch (result.Type)
        {
            case JudgmentType.Perfect:
                AddToScore(JudgmentManager.Instance.perfectScore);
                IncrementCombo();
                if (SkillManager.Instance != null && SkillManager.Instance.IsPerfectHealActive)
                {
                    ChangeHealth(SkillManager.Instance.GetHealAmount());
                }
                break;
            case JudgmentType.Great:
                AddToScore(JudgmentManager.Instance.greatScore);
                IncrementCombo();
                break;
            case JudgmentType.Good:
                AddToScore(JudgmentManager.Instance.goodScore);
                IncrementCombo();
                break;
            case JudgmentType.HoldHead:
                IncrementCombo();
                break;
            case JudgmentType.Miss:
            case JudgmentType.HoldBreak:
                BreakCombo();
                ChangeHealth(-penaltyAmount);
                break;
        }
    }

    /// <summary>
    /// 在游戏结束时，由 GameManager 调用，将最终统计数据打包到全局静态类 ResultsData 中
    /// </summary>
    /// <param name="gameWon">游戏是否成功 (true) 或失败 (false)</param>
    public void FinalizeResults(bool gameWon)
    {
        ResultsData.GameWon = gameWon;
        ResultsData.FinalScore = CurrentScore;
        ResultsData.PerfectCount = JudgmentCounts[JudgmentType.Perfect];
        ResultsData.GreatCount = JudgmentCounts[JudgmentType.Great];
        ResultsData.GoodCount = JudgmentCounts[JudgmentType.Good];
        // Miss 和 HoldBreak 都算作失误
        ResultsData.MissCount = JudgmentCounts[JudgmentType.Miss] + JudgmentCounts[JudgmentType.HoldBreak];
        ResultsData.MaxCombo = this.MaxCombo;

        Debug.Log("最终结算数据已打包。");
    }

    public void ChangeHealth(float amount)
    {
        if (CurrentHealth <= 0 && amount < 0) return;
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

    private void AddToScore(long amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}