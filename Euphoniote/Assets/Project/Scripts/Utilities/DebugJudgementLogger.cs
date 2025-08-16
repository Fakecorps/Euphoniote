// _Project/Scripts/Utilities/DebugJudgmentLogger.cs (修正版)

using UnityEngine;

public class DebugJudgmentLogger : MonoBehaviour
{
    private void OnEnable()
    {
        // 订阅核心判定事件
        JudgmentManager.OnNoteJudged += HandleCoreJudgment;

        // 订阅 Hold Note 调试事件 (可选，如果想看更详细的HoldNote过程)
        JudgmentManager.OnHoldNoteDebug += HandleHoldDebug;
    }

    private void OnDisable()
    {
        // 取消订阅，非常重要
        JudgmentManager.OnNoteJudged -= HandleCoreJudgment;
        JudgmentManager.OnHoldNoteDebug -= HandleHoldDebug;
    }

    /// <summary>
    /// 处理核心的、所有音符都会触发的判定事件
    /// </summary>
    private void HandleCoreJudgment(JudgmentResult result)
    {
        // --- 核心判定日志，适用于所有音符（TapNote 和 HoldNote） ---
        string logMessage = $"[判定日志] -> 类型: {result.Type}";
        if (result.IsSpecialNote)
        {
            logMessage += " (特殊音符!)";
        }

        switch (result.Type)
        {
            case JudgmentType.Perfect:
                Debug.Log($"<color=cyan>{logMessage}</color>");
                break;
            case JudgmentType.Great:
                Debug.Log($"<color=green>{logMessage}</color>");
                break;
            case JudgmentType.Good:
                Debug.Log($"<color=yellow>{logMessage}</color>");
                break;
            case JudgmentType.Miss:
                Debug.Log($"<color=red>{logMessage}</color>");
                break;
            case JudgmentType.HoldBreak:
                Debug.Log($"<color=red>{logMessage}</color>");
                break;
        }
        // --- 核心判定日志结束 ---
    }

    /// <summary>
    /// 处理专门为 Hold Note 提供的、带有详细信息的调试事件
    /// </summary>
    private void HandleHoldDebug(JudgmentManager.HoldDebugInfo info)
    {
        // 这些日志提供 Hold Note 内部状态的详细信息，例如头部判定的精准度或中断的原因。
        switch (info.Stage)
        {
            case "Start":
                // 打印 Hold Note 的头部判定信息
                Debug.Log($"<color=#6495ED>[Hold 调试] <b>开始:</b> 头部判定: {info.HeadJudgment}, 时间误差: {info.TimeDiff:F3}s</color>");
                break;
            case "Success":
                // 打印 Hold Note 成功时的最终判定（通常与头部判定一致）
                Debug.Log($"<color=#32CD32>[Hold 调试] <b>成功:</b> 最终判定: {info.FinalJudgment}, 基于头部误差: {info.TimeDiff:F3}s</color>");
                break;
            case "Break":
                // 仅打印 Hold Break 的调试信息，不重复打印核心判定（核心判定由 HandleCoreJudgment 打印）
                Debug.Log("[Hold 调试] <b>中断!</b>");
                break;
        }
    }
}