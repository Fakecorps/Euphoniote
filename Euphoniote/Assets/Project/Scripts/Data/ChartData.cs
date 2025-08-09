using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChartData
{
    public string songName;//歌曲名
    public float bpm;//节拍
    public List<NoteData> notes;//存储音符数据
}
