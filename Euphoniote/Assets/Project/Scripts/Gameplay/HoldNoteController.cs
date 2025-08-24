// _Project/Scripts/Gameplay/HoldNoteController.cs (事件驱动销毁版)

using UnityEngine;
using System; // 需要这个来使用 Action

public class HoldNoteController : MonoBehaviour, INoteController
{
    // --- 新增部分 ---
    /// <summary>
    /// 当 Hold Note 的收缩动画完成时触发的静态事件。
    /// 任何外部系统都可以监听此事件来进行清理工作。
    /// </summary>
    public static event Action<HoldNoteController> OnShrinkAnimationComplete;

    // ... [其他所有字段和属性保持不变] ...
    [Header("核心模块")]
    public NoteHeadController headVisuals;
    public NoteAnchorController headAnchor;
    public NoteHeadController endCapVisuals;
    public NoteAnchorController endCapAnchor;

    [System.Serializable]
    public class TrailSystem
    {
        public Transform trailContainer;
        public Transform head;
        public SpriteRenderer body;
        public Transform end;
    }

    [Header("长条组件 (三段式)")]
    public TrailSystem arrowTrail;
    public TrailSystem containerTrail;

    private bool isBeingHeld = false;
    private NoteData noteData;
    private bool isShrinking = false;

    public bool IsJudged { get; private set; } = false;
    public void SetJudged() { IsJudged = true; }
    public float HeadTimeDiff { get; private set; }
    public void SetHeadTimeDiff(float diff) { HeadTimeDiff = Mathf.Abs(diff); }
    public NoteData GetNoteData() => noteData;
    public GameObject GetGameObject() => gameObject;

    // ... [Initialize, OnEnable, OnDisable 方法保持不变] ...
    public void Initialize(NoteData data, float speed, float lineX)
    {
        this.noteData = data;
        if (headVisuals != null) { headVisuals.Initialize(data, true); }
        if (endCapVisuals != null) { endCapVisuals.Initialize(data, false); }

        float noteEndTime = data.time + data.duration;
        if (headAnchor != null) { headAnchor.Setup(data.time, speed, lineX); }
        if (endCapAnchor != null) { endCapAnchor.Setup(noteEndTime, speed, lineX); }
    }

    void OnEnable()
    {
        JudgmentManager.RegisterNote(this);
    }
    void OnDisable()
    {
        JudgmentManager.UnregisterNote(this);
    }

    void Update()
    {
        if (isShrinking)
        {
            UpdateTrails();

            // --- 核心修改点 ---
            // 检查动画是否完成
            if (endCapAnchor != null && endCapAnchor.transform.position.x < NoteSpawner.Instance.judgmentLineX - 2f)
            {
                // 广播事件，将自身作为参数传递出去
                OnShrinkAnimationComplete?.Invoke(this);

                // 禁用此脚本，防止事件被重复触发
                this.enabled = false;
            }
            return;
        }

        if (IsJudged)
        {
            // 失败时的销毁逻辑不变，由外部系统决定
            Destroy(gameObject);
            return;
        }

        if (!isBeingHeld && noteData != null)
        {
            if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
            {
                JudgmentManager.Instance.ProcessMiss(this);
            }
        }

        UpdateTrails();
    }

    // ... [UpdateTrails 和 UpdateSingleTrailSystem 方法保持不变] ...
    private void UpdateTrails()
    {
        if (headAnchor == null || endCapAnchor == null) return;

        Vector3 headAnchorWorldPos = headAnchor.transform.position;
        Vector3 endAnchorWorldPos = endCapAnchor.transform.position;

        if (isShrinking)
        {
            headAnchorWorldPos.x = NoteSpawner.Instance.judgmentLineX;
        }

        UpdateSingleTrailSystem(arrowTrail, headAnchorWorldPos, endAnchorWorldPos);
        UpdateSingleTrailSystem(containerTrail, headAnchorWorldPos, endAnchorWorldPos);
    }

    private void UpdateSingleTrailSystem(TrailSystem trail, Vector3 headWorldPos, Vector3 endWorldPos)
    {
        if (trail.trailContainer == null || trail.head == null || trail.body == null || trail.end == null) return;

        Vector3 localHeadPos = trail.trailContainer.InverseTransformPoint(headWorldPos);
        Vector3 localEndPos = trail.trailContainer.InverseTransformPoint(endWorldPos);

        trail.head.localPosition = new Vector3(localHeadPos.x, trail.head.localPosition.y, trail.head.localPosition.z);
        trail.end.localPosition = new Vector3(localEndPos.x, trail.end.localPosition.y, trail.end.localPosition.z);

        float localDistance = Mathf.Abs(localEndPos.x - localHeadPos.x);

        if (localDistance < 0.01f)
        {
            trail.body.enabled = false;
            return;
        }
        trail.body.enabled = true;

        float localMidX = (localHeadPos.x + localEndPos.x) / 2f;

        trail.body.transform.localPosition = new Vector3(
            localMidX,
            trail.body.transform.localPosition.y,
            trail.body.transform.localPosition.z
        );

        trail.body.size = new Vector2(localDistance, trail.body.size.y);
    }

    // ... [SetHeldState 和 StartSuccessShrink 方法保持不变] ...
    public void SetHeldState(bool held)
    {
        isBeingHeld = held;
        if (isBeingHeld && headAnchor != null)
        {
            headAnchor.HoldPosition();
        }
    }

    public void StartSuccessShrink()
    {
        isShrinking = true;
        SetJudged();
        if (headVisuals != null) { headVisuals.gameObject.SetActive(false); }
        if (endCapVisuals != null) { endCapVisuals.gameObject.SetActive(false); }
    }
}