

using System.Collections.Generic;

// 这个类只用于从JSON加载，是临时的“蓝图”
[System.Serializable]
public class BeatmapData
{
    public string songName;
    public float bpm;
    public float offset;
    public int ticksPerBeat;
    public List<BeatNoteData> notes;
}

[System.Serializable]
public class BeatNoteData
{
    public int beat;
    public int subdivision;
    public int endBeat;
    public int endSubdivision;

    public List<FretKey> requiredFrets;
    public StrumType strumType;
    public bool isSpecial;
}