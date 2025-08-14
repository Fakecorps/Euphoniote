using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugJudgementLogger : MonoBehaviour
{
    private void OnEnable()
    {
        JudgmentManager.OnNoteJudged += HandleJudgment;
    }

    // 在对象禁用时，停止监听，防止内存泄漏
    private void OnDisable()
    {
        JudgmentManager.OnNoteJudged -= HandleJudgment;
    }

    // 当 JudgmentManager 广播一个判定结果时，这个方法会被调用
    private void HandleJudgment(JudgmentResult result)
    {
        // 在控制台打印出详细的判定信息
        string logMessage = $"[判定日志] -> 类型: {result.Type}";

        if (result.IsSpecialNote)
        {
            logMessage += " (特殊音符!)";
        }

        // 根据判定类型选择不同的日志颜色，方便查看
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
            case JudgmentType.HoldBreak:
                Debug.LogWarning(logMessage); // 使用黄色警告来显示Miss和Break
                break;
        }
    }
}
