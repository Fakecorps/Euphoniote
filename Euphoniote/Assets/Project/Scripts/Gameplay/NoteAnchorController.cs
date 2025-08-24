// _Project/Scripts/Gameplay/NoteAnchorController.cs (新脚本)

using UnityEngine;


public class NoteAnchorController : MonoBehaviour
{
    private float noteTime;
    private float scrollSpeed;
    private float judgmentLineX;

    // 是否被“吸附”在判定线上
    private bool isHeldAtLine = false;

    public void Setup(float time, float speed, float lineX)
    {
        this.noteTime = time;
        this.scrollSpeed = speed;
        this.judgmentLineX = lineX;
        this.isHeldAtLine = false; // 确保每次Setup都重置状态
    }

    void Update()
    {
        if (isHeldAtLine)
        {
            // 如果被吸附，则强制位置在判定线
            transform.position = new Vector2(judgmentLineX, transform.position.y);
            return;
        }

        if (TimingManager.Instance != null)
        {
            float currentSongTime = TimingManager.Instance.SongPosition;
            float targetX = judgmentLineX + (noteTime - currentSongTime) * scrollSpeed;
            transform.position = new Vector2(targetX, transform.position.y);
        }
    }

    public void HoldPosition()
    {
        isHeldAtLine = true;
    }
}