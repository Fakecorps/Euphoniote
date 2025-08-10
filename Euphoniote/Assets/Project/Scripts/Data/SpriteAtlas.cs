using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameSpriteAtlas", menuName = "Game/Sprite Atlas")]
public class SpriteAtlas : ScriptableObject
{
    // 使用字典来存储映射关系，更高效
    // 但字典不能被Unity序列化，所以我们用两个列表来模拟
    public List<StrumType> strumKeys;
    public List<Sprite> strumSprites;

    public List<FretKey> fretKeys;
    public List<Sprite> fretSprites;

    private Dictionary<StrumType, Sprite> strumDict;
    private Dictionary<FretKey, Sprite> fretDict;

    // 在游戏启动时构建字典
    public void Initialize()
    {
        strumDict = new Dictionary<StrumType, Sprite>();
        for (int i = 0; i < strumKeys.Count; i++)
            strumDict[strumKeys[i]] = strumSprites[i];

        fretDict = new Dictionary<FretKey, Sprite>();
        for (int i = 0; i < fretKeys.Count; i++)
            fretDict[fretKeys[i]] = fretSprites[i];
    }

    public Sprite GetStrumSprite(StrumType type) => strumDict.ContainsKey(type) ? strumDict[type] : null;
    public Sprite GetFretSprite(FretKey key) => fretDict.ContainsKey(key) ? fretDict[key] : null;
}
