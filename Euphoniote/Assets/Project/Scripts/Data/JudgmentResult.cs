using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JudgmentType { Perfect, Great, Good, Miss, HoldBreak }

public struct JudgmentResult
{
    public JudgmentType Type;
    public bool IsSpecialNote;
}
