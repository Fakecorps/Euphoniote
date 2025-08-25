// _Project/Scripts/Managers/JudgmentSpriteMapping.cs

using UnityEngine;

// 这个 [System.Serializable] 属性是关键，它让这个类的实例可以在Inspector面板中显示和编辑
[System.Serializable]
public class JudgmentSpriteMapping
{
    public JudgmentType judgment;
    public Sprite sprite;
}