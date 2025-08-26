// _Project/Scripts/Gameplay/HoldNoteController.cs (最终修改版 - 增加Miss动画)

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class HoldNoteController : BaseNoteController, INoteController
{
    public static event Action<HoldNoteController> OnShrinkAnimationComplete;

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

    private bool isShrinking = false;
    private bool isCleanupScheduled = false;


    public float HeadTimeDiff { get; private set; }
    public void SetHeadTimeDiff(float diff) { HeadTimeDiff = Mathf.Abs(diff); }

    public void Initialize(NoteData data, float speed, float lineX)
    {
        this.noteData = data;
        PrepareForPooling(); // 每次初始化时都清理一下
        if (headVisuals != null) { headVisuals.Initialize(data, true); }
        if (endCapVisuals != null) { endCapVisuals.Initialize(data, false); }
        float noteEndTime = data.time + data.duration;
        if (headAnchor != null) { headAnchor.Setup(data.time, speed, lineX); }
        if (endCapAnchor != null) { endCapAnchor.Setup(noteEndTime, speed, lineX); }
    }

    public void PrepareForPooling()
    {
        ResetJudgedState();

        IsJudged = false;
        isShrinking = false;
        isBeingHeld = false;
        isCleanupScheduled = false;
        this.enabled = true;
        ResetTrails();
        if (headVisuals != null) { headVisuals.gameObject.SetActive(true); }
        if (endCapVisuals != null) { endCapVisuals.gameObject.SetActive(true); }
    }

    private void ResetTrails()
    {
        ResetSingleTrailSystem(arrowTrail);
        ResetSingleTrailSystem(containerTrail);
    }
    private void ResetSingleTrailSystem(TrailSystem trail)
    {
        if (trail.trailContainer == null) return;
        if (trail.body != null)
        {
            trail.body.size = new Vector2(0, trail.body.size.y);
            trail.body.enabled = true;
        }
        if (trail.head != null) trail.head.localPosition = Vector3.zero;
        if (trail.end != null) trail.end.localPosition = Vector3.zero;
    }

    /// <summary>
    /// [失败时调用] 播放Miss动画后返回对象池。
    /// </summary>
    public void PlayMissAnimationAndRelease()
    {
        if (isCleanupScheduled) return;
        isCleanupScheduled = true;

        SetJudged();
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        List<SpriteRenderer> allRenderers = new List<SpriteRenderer>();
        if (headVisuals != null) allRenderers.AddRange(headVisuals.GetComponentsInChildren<SpriteRenderer>());
        if (endCapVisuals != null) allRenderers.AddRange(endCapVisuals.GetComponentsInChildren<SpriteRenderer>());
        if (arrowTrail.body != null) allRenderers.Add(arrowTrail.body);
        if (arrowTrail.head != null) allRenderers.AddRange(arrowTrail.head.GetComponentsInChildren<SpriteRenderer>());
        if (arrowTrail.end != null) allRenderers.AddRange(arrowTrail.end.GetComponentsInChildren<SpriteRenderer>());
        if (containerTrail.body != null) allRenderers.Add(containerTrail.body);
        if (containerTrail.head != null) allRenderers.AddRange(containerTrail.head.GetComponentsInChildren<SpriteRenderer>());
        if (containerTrail.end != null) allRenderers.AddRange(containerTrail.end.GetComponentsInChildren<SpriteRenderer>());

        float fadeDuration = 0.2f;
        float timer = 0f;
        var initialColors = new Dictionary<SpriteRenderer, Color>();
        foreach (var r in allRenderers) { if (r != null) initialColors[r] = r.color; }

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            foreach (var r in allRenderers)
            {
                if (r != null && initialColors.ContainsKey(r))
                {
                    Color initialColor = initialColors[r];
                    r.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                }
            }
            yield return null;
        }

        PrepareForPooling();
        NotePoolManager.Instance.ReturnToPool("HoldNote", gameObject);
    }

    protected override void OnEnable() { JudgmentManager.RegisterNote(this); }
    protected override void OnDisable() { JudgmentManager.UnregisterNote(this); }

    protected override void Update()
    {
        if (isCleanupScheduled) return;

        if (isShrinking)
        {
            UpdateTrails();
            if (endCapAnchor != null && endCapAnchor.transform.position.x < NoteSpawner.Instance.judgmentLineX - 2f)
            {
                isCleanupScheduled = true;
                OnShrinkAnimationComplete?.Invoke(this);
            }
            return;
        }

        if (IsJudged)
        {
            // 这个分支现在主要用于HoldBreak的立即回收
            isCleanupScheduled = true;
            PrepareForPooling();
            NotePoolManager.Instance.ReturnToPool("HoldNote", gameObject);
            return;
        }

        // 检查超时Miss
        if (!isBeingHeld && noteData != null)
        {
            if (noteData.time < TimingManager.Instance.SongPosition - JudgmentManager.Instance.goodWindow)
            {
                PlayMissAnimationAndRelease();
                JudgmentManager.Instance.BroadcastMissEvent(this);
            }
        }

        UpdateTrails();
    }

    // ... [UpdateTrails, UpdateSingleTrailSystem, SetHeldState, StartSuccessShrink 和 IsCleanForPooling 方法保持不变] ...
    public bool IsCleanForPooling()
    {
        bool arrowTrailClean = arrowTrail.body == null || arrowTrail.body.size.x < 0.01f;
        bool containerTrailClean = containerTrail.body == null || containerTrail.body.size.x < 0.01f;
        return arrowTrailClean && containerTrailClean;
    }
    private void UpdateTrails()
    {
        if (headAnchor == null || endCapAnchor == null) return;
        Vector3 headAnchorWorldPos = headAnchor.transform.position;
        Vector3 endAnchorWorldPos = endCapAnchor.transform.position;
        if (isShrinking) { headAnchorWorldPos.x = NoteSpawner.Instance.judgmentLineX; }
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
        if (localDistance < 0.01f) { trail.body.enabled = false; return; }
        trail.body.enabled = true;
        float localMidX = (localHeadPos.x + localEndPos.x) / 2f;
        trail.body.transform.localPosition = new Vector3(localMidX, trail.body.transform.localPosition.y, trail.body.transform.localPosition.z);
        trail.body.size = new Vector2(localDistance, trail.body.size.y);
    }
    public void SetHeldState(bool held)
    {
        isBeingHeld = held;
        if (isBeingHeld && headAnchor != null) { headAnchor.HoldPosition(); }
    }
    public void StartSuccessShrink()
    {
        isShrinking = true;
        SetJudged();
        if (headVisuals != null) { headVisuals.gameObject.SetActive(false); }
        if (endCapVisuals != null) { endCapVisuals.gameObject.SetActive(false); }
    }
}