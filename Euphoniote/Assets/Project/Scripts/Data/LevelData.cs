// _Project/Scripts/Data/LevelData.cs

using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("剧情")]
    public string storyStart;
    public string storyEnd;

    [Header("音游")]
    public string chartFileName;
    public AudioClip musicClip;
}