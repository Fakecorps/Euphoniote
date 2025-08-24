// _Project/Scripts/Data/JudgmentResult.cs

public enum JudgmentType 
{ 
    Perfect, 
    Great, 
    Good, 
    Miss, 
    HoldBreak, 
    HoldHead // <<-- 确保添加了这个新的枚举值
}

public struct JudgmentResult
{
    public JudgmentType Type;
    public bool IsSpecialNote;
}