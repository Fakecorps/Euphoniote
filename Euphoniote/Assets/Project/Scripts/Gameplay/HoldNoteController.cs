using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNoteController : BaseNoteController
{
    public Transform holdTrail;    // 长条的Transform
    public Transform holdEndCap;   // 尾巴的Transform

    private bool isBeingHeld = false;

    public override void Initialize(NoteData data)
    {
        base.Initialize(data); // 调用父类的公共初始化

        // HoldNote特有的初始化：设置长条长度和尾巴位置
        float trailLength = data.duration * scrollSpeed;
        holdTrail.localScale = new Vector3(trailLength, holdTrail.localScale.y, holdTrail.localScale.z);
        holdEndCap.localPosition = new Vector3(trailLength, holdEndCap.localPosition.y, holdEndCap.localPosition.z);
    }

    protected override void Update()
    {
        base.Update(); // 调用父类的移动逻辑

        if (isBeingHeld)
        {
            // 如果正在被按住，可以更新长条的视觉效果，比如让它“消耗”掉
            // 简单起见，我们先让它跟着头部移动
        }
        else
        {
            // 如果飘过判定区，算作Miss
            if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
            {
                JudgmentManager.Instance.ProcessMiss(this);
            }
        }
    }

    // 提供一个方法给JudgmentManager，用于更新被按住的状态
    public void SetHeldState(bool held)
    {
        isBeingHeld = held;
        // 在这里可以改变音符的外观，比如让它高亮
    }
}
