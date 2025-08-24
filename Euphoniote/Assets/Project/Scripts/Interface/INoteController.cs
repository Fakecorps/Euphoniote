
public interface INoteController
{
    bool IsJudged { get; }
    void SetJudged();
    NoteData GetNoteData();
    UnityEngine.GameObject GetGameObject(); // 提供一个获取自身GameObject的方法
}