// _Project/Scripts/Managers/StatsManager.cs (完整版)

using UnityEngine;
using System;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }


    public int CurrentCombo { get; private set; } = 0;
    public int MaxCombo { get; private set; } = 0;

    [Tooltip("玩家的最大生命值")]
    public float maxHealth = 100f;
    [Tooltip("当前生命值")]
    public float CurrentHealth { get; private set; }
    [Tooltip("每次Miss或HoldBreak扣除的生命值")]
    public float penaltyAmount = 20f;

    // 当Combo数值发生变化时广播 (传递新的Combo值)
    public static event Action<int> OnComboChanged;
    // 当Combo中断时广播
    public static event Action OnComboBroken;

    public static event Action<float, float> OnHealthChanged; // 传递 (当前生命值, 最大生命值)
    public static event Action OnGameOver; // 当生命值耗尽时触发

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
        CurrentHealth = maxHealth;
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
                ChangeHealth(-penaltyAmount);
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

    public void BreakCombo()
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

    public void ChangeHealth(float amount)
    {
        Debug.Log("Health Changed: " + amount);
        // 如果游戏已经结束，不再改变生命值
        if (CurrentHealth <= 0) return;

        CurrentHealth += amount;

        // 确保生命值不会超过上限或低于下限
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

        // 广播生命值变化事件，通知UI等系统
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        // 检查游戏是否结束
        if (CurrentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("<color=red>GAME OVER! 生命值耗尽。</color>");
        // 广播游戏结束事件
        OnGameOver?.Invoke();

        // 在这里可以执行一些立即生效的游戏结束逻辑
        // 例如，停止音乐、停止音符生成等
        // Time.timeScale = 0; // 简单粗暴的暂停
    }
}