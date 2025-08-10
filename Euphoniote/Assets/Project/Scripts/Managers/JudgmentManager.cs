using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JudgmentManager : MonoBehaviour
{
    public static JudgmentManager Instance { get; private set; }

    // 判定时间窗口 (单位：秒)
    public float perfectWindow = 0.05f; // ±50ms
    public float greatWindow = 0.1f;   // ±100ms
    public float goodWindow = 0.15f;   // ±150ms

    // 存储场景中所有活动的音符
    private static List<NoteController> activeNotes = new List<NoteController>();

    private PlayerInputActions playerInput;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        // 初始化输入系统
        playerInput = new PlayerInputActions();
    }

    void OnEnable()
    {
        // 启用 Gameplay Action Map
        playerInput.Gameplay.Enable();

        // 订阅右手按键的 "performed" 事件
        playerInput.Gameplay.UpStrum.performed += ctx => CheckStrum(StrumType.Up);
        playerInput.Gameplay.DownStrum.performed += ctx => CheckStrum(StrumType.Down);
        // TODO: 为Hold音符添加 started 和 canceled 事件订阅
    }

    void OnDisable()
    {
        playerInput.Gameplay.Disable();
        // 取消订阅，防止内存泄漏
        playerInput.Gameplay.UpStrum.performed -= ctx => CheckStrum(StrumType.Up);
        playerInput.Gameplay.DownStrum.performed -= ctx => CheckStrum(StrumType.Down);
    }

    // 核心判定函数
    private void CheckStrum(StrumType strumType)
    {
        float songPosition = TimingManager.Instance.SongPosition;
        NoteController noteToJudge = null;
        float minTimeDiff = float.MaxValue;

        // 1. 找到时间最接近的、类型匹配的音符
        foreach (var note in activeNotes)
        {
            if (note.noteData.strumType == strumType)
            {
                float timeDiff = Mathf.Abs(note.noteData.time - songPosition);
                if (timeDiff < minTimeDiff)
                {
                    minTimeDiff = timeDiff;
                    noteToJudge = note;
                }
            }
        }

        // 如果没找到合适的音符，或者音符已经超出最大判定范围，则不处理
        if (noteToJudge == null || minTimeDiff > goodWindow)
        {
            Debug.Log("Miss! (空挥)");
            // 可以在这里加一个空挥的惩罚音效或效果
            return;
        }

        // 2. 检查左手按键是否正确
        bool fretsAreCorrect = CheckFrets(noteToJudge.noteData.requiredFrets);
        if (!fretsAreCorrect)
        {
            Debug.Log("Miss! (左手按错了)");
            // 把这个音符当作Miss处理掉
            activeNotes.Remove(noteToJudge);
            Destroy(noteToJudge.gameObject);
            return;
        }

        // 3. 根据时间差进行判定
        if (minTimeDiff <= perfectWindow) { Hit("Perfect", noteToJudge); }
        else if (minTimeDiff <= greatWindow) { Hit("Great", noteToJudge); }
        else if (minTimeDiff <= goodWindow) { Hit("Good", noteToJudge); }
    }

    private bool CheckFrets(List<FretKey> requiredFrets)
    {
        // 这个函数非常关键，需要检查 requiredFrets 里的每一个键是否都被按下了，
        // 并且没有按下任何多余的键。

        // 简单版本：只检查必需的是否按下
        foreach (var fret in requiredFrets)
        {
            switch (fret)
            {
                case FretKey.A: if (!playerInput.Gameplay.FretA.IsPressed()) return false; break;
                case FretKey.S: if (!playerInput.Gameplay.FretS.IsPressed()) return false; break;
                case FretKey.D: if (!playerInput.Gameplay.FretD.IsPressed()) return false; break;
                case FretKey.Q: if (!playerInput.Gameplay.FretQ.IsPressed()) return false; break;
                case FretKey.W: if (!playerInput.Gameplay.FretW.IsPressed()) return false; break;
                case FretKey.E: if (!playerInput.Gameplay.FretE.IsPressed()) return false; break;
            }
        }
        return true;
        // 进阶版本：还需要检查是否按了多余的键，可以增加代码来判断
    }

    private void Hit(string judgment, NoteController note)
    {
        Debug.Log(judgment + "!");
        // TODO: 触发分数增加、连击增加、播放特效和音效

        // 从活动列表中移除并销毁音符
        activeNotes.Remove(note);
        Destroy(note.gameObject);
    }

    // 这两个方法需要被外部调用
    public static void RegisterNote(NoteController note)
    {
        if (!activeNotes.Contains(note))
        {
            activeNotes.Add(note);
        }
    }

    public static void UnregisterNote(NoteController note)
    {
        if (activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
        }
    }
}
