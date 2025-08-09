using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StrumType { Up, Down, HoldLeft, HoldRight }//右手枚举
public enum FretKey { A, S, D, F, G }//左手枚举

[System.Serializable]
public class NoteData
{
    public float time;
    public List<FretKey> requiredFrets;//左手所需要的按键
    public StrumType strumType;//右手需要弹奏的类型
    public float duration;
    public bool isSpecial;//是否是特殊音符
}
