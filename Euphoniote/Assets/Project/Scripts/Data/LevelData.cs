// _Project/Scripts/Data/LevelData.cs

using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("剧情")]
    public string storyStart; // 游戏前剧情文件名
    public string storyEnd;   // 游戏后剧情文件名

    [Header("音游")]
    public string chartFileName;
    public AudioClip musicClip;
}