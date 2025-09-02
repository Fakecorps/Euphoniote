// _Project/Scripts/Data/SimpleBeatmapData.cs

using System.Collections.Generic;

[System.Serializable]
public class SimpleBeatmapData
{
    public string songName;
    public float bpm;
    public float offset;
    public int ticksPerBeat;
    // 注意：这里是一个 int 数组的列表
    public List<int[]> notes;
}