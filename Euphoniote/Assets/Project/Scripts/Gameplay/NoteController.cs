using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NoteController : BaseNoteController
{
    public override void Initialize(NoteData data)
    {
        base.Initialize(data); // 调用父类的公共初始化
        // TapNote没有长条，所以不需要做任何事
    }

    // Update中处理飘过Miss的逻辑
    protected override void Update()
    {
        base.Update(); // 调用父类的移动逻辑

        // Miss判定
        if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
        {
            // 注意：这里我们不直接销毁，而是通知JudgmentManager
            JudgmentManager.Instance.ProcessMiss(this);
        }
    }
}
