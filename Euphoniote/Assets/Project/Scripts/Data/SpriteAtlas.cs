// _Project/Scripts/Data/SpriteAtlas.cs (完整最终版)

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameSpriteAtlas", menuName = "Game/Sprite Atlas")]
public class SpriteAtlas : ScriptableObject
{
    [Header("箭头 Sprites (Tap & Hold 头部)")]
    public List<StrumType> strumKeys;
    public List<Sprite> strumSprites;

    [Header("按键字母 Sprites")]
    public List<FretKey> fretKeys;
    public List<Sprite> fretSprites;

    [Header("Hold Note 长条模板")]
    [Tooltip("这里只配置 HoldLeft 和 HoldRight 对应的九宫格模板图")]
    public List<StrumType> holdNoteKeys;
    public List<Sprite> holdNoteSprites;

    [Header("容器模板 ")]

    [Tooltip("普通音容器模板")]
    public Sprite normalContainerTemplate;
    [Tooltip("特殊音符容器模板")]
    public Sprite specialContainerTemplate;
    // 内部字典，用于快速查找，不对外暴露
    private Dictionary<StrumType, Sprite> strumDict;
    private Dictionary<FretKey, Sprite> fretDict;
    private Dictionary<StrumType, Sprite> holdNoteDict;

    public void Initialize()
    {

        strumDict = new Dictionary<StrumType, Sprite>();
        // 使用 Mathf.Min 来防止列表长度不匹配导致的越界错误
        for (int i = 0; i < Mathf.Min(strumKeys.Count, strumSprites.Count); i++)
        {
            strumDict[strumKeys[i]] = strumSprites[i];
        }

        fretDict = new Dictionary<FretKey, Sprite>();
        for (int i = 0; i < Mathf.Min(fretKeys.Count, fretSprites.Count); i++)
        {
            fretDict[fretKeys[i]] = fretSprites[i];
        }

        // --- 这是关键的新增部分 ---
        holdNoteDict = new Dictionary<StrumType, Sprite>();
        for (int i = 0; i < Mathf.Min(holdNoteKeys.Count, holdNoteSprites.Count); i++)
        {
            holdNoteDict[holdNoteKeys[i]] = holdNoteSprites[i];
        }
        // --- 结束新增 ---
    }

    public Sprite GetStrumSprite(StrumType type)
    {
        // 使用 TryGetValue 更安全，如果键不存在不会报错
        if (strumDict != null && strumDict.TryGetValue(type, out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }

    public Sprite GetFretSprite(FretKey key)
    {
        if (fretDict != null && fretDict.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }

    public Sprite GetHoldNoteSprite(StrumType type)
    {
        if (holdNoteDict != null && holdNoteDict.TryGetValue(type, out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }

    public Sprite GetContainerTemplate(bool isSpecial)
    {
        // 根据 isSpecial 返回对应的模板图
        if (isSpecial)
        {
            return specialContainerTemplate;
        }
        else
        {
            return normalContainerTemplate;
        }
    }
}