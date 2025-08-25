// _Project/Scripts/Managers/UIManager.cs (最终修改版 - 保持UI比例)

using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("判定反馈UI")]
    public UnityEngine.UI.Image judgmentImage;     // 拖入 JudgmentImage
    public TextMeshProUGUI judgmentComboText;   // 拖入 JudgmentComboText

    [Header("判定Sprite映射")]
    public List<JudgmentSpriteMapping> judgmentSpriteMappings;

    [Header("动画参数")]
    public float judgmentPunchScale = 1.5f;     // 图片初始放大的倍数
    public float judgmentFadeInDuration = 0.05f;
    public float judgmentHoldDuration = 0.3f;
    public float judgmentFadeOutDuration = 0.4f;
    public float comboPunchScale = 1.2f;        // Combo文字放大的倍数
    public float comboAnimationDuration = 0.1f;

    // 内部变量
    private Coroutine judgmentImageAnimationCoroutine;
    private Coroutine comboAnimationCoroutine;
    private Dictionary<JudgmentType, Sprite> judgmentSpriteDict;

    // --- 新增：用于存储UI原始缩放的变量 ---
    private Vector3 initialJudgmentImageScale;
    private Vector3 initialComboTextScale;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        judgmentSpriteDict = new Dictionary<JudgmentType, Sprite>();
        foreach (var mapping in judgmentSpriteMappings)
        {
            judgmentSpriteDict[mapping.judgment] = mapping.sprite;
        }
    }

    void Start()
    {
        // --- 核心修改点：在游戏开始时，记录下你在Editor中设置的原始Scale ---
        if (judgmentImage != null)
        {
            initialJudgmentImageScale = judgmentImage.transform.localScale;
            judgmentImage.gameObject.SetActive(false);
        }
        if (judgmentComboText != null)
        {
            initialComboTextScale = judgmentComboText.transform.localScale;
            judgmentComboText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        StatsManager.OnComboChanged += HandleComboChanged;
        StatsManager.OnComboBroken += HandleComboBroken;
        JudgmentManager.OnNoteJudged += HandleJudgmentFeedback;
    }

    private void OnDisable()
    {
        StatsManager.OnComboChanged -= HandleComboChanged;
        StatsManager.OnComboBroken -= HandleComboBroken;
        JudgmentManager.OnNoteJudged -= HandleJudgmentFeedback;
    }

    private void HandleJudgmentFeedback(JudgmentResult result)
    {
        if (result.Type == JudgmentType.HoldHead) return;

        if (judgmentSpriteDict.TryGetValue(result.Type, out Sprite spriteToShow))
        {
            if (judgmentImageAnimationCoroutine != null)
            {
                StopCoroutine(judgmentImageAnimationCoroutine);
            }
            judgmentImageAnimationCoroutine = StartCoroutine(JudgmentImageAnimation(spriteToShow));
        }
    }

    private void HandleComboChanged(int newCombo)
    {
        if (judgmentComboText == null) return;

        judgmentComboText.text = newCombo.ToString();

        if (newCombo >= 2)
        {
            if (!judgmentComboText.gameObject.activeInHierarchy)
            {
                judgmentComboText.gameObject.SetActive(true);
            }

            if (comboAnimationCoroutine != null) StopCoroutine(comboAnimationCoroutine);
            comboAnimationCoroutine = StartCoroutine(PunchScaleAnimation(judgmentComboText.transform, initialComboTextScale, comboPunchScale));
        }
    }

    private void HandleComboBroken()
    {
        if (judgmentComboText == null) return;
        judgmentComboText.gameObject.SetActive(false);
    }

    private IEnumerator JudgmentImageAnimation(Sprite sprite)
    {
        // 准备
        judgmentImage.sprite = sprite;
        judgmentImage.gameObject.SetActive(true);

        CanvasGroup imageCG = judgmentImage.GetComponent<CanvasGroup>();
        if (imageCG == null) imageCG = judgmentImage.gameObject.AddComponent<CanvasGroup>();

        // --- 核心修改点：使用原始Scale作为动画基准 ---
        Vector3 startScale = initialJudgmentImageScale * judgmentPunchScale*0.8f;
        Vector3 endScale = initialJudgmentImageScale; // 动画结束时恢复到原始比例

        judgmentImage.transform.localScale = startScale; // 设置初始放大状态

        // 1. 淡入 (同时稍微缩小到最终停留大小)
        float timer = 0f;
        while (timer < judgmentFadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / judgmentFadeInDuration;
            imageCG.alpha = Mathf.Lerp(0, 1, progress);
            // 可以选择在这里也加一个缩放动画，或者保持不变
            yield return null;
        }
        imageCG.alpha = 1;

        // 2. 停留
        yield return new WaitForSeconds(judgmentHoldDuration);

        // 3. 缩小并淡出
        timer = 0f;
        while (timer < judgmentFadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / judgmentFadeOutDuration;
            imageCG.alpha = Mathf.Lerp(1, 0, progress);
            judgmentImage.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }

        // 清理
        judgmentImage.gameObject.SetActive(false);
        judgmentImage.transform.localScale = initialJudgmentImageScale; // 确保恢复
    }

    private IEnumerator PunchScaleAnimation(Transform targetTransform, Vector3 originalScale, float punchMultiplier)
    {
        Vector3 punchScale = originalScale * punchMultiplier;
        float halfDuration = comboAnimationDuration / 2f;
        float timer = 0f;

        // --- 核心修改点：动画在原始比例和放大比例之间进行 ---
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(originalScale, punchScale, timer / halfDuration);
            yield return null;
        }

        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(punchScale, originalScale, timer / halfDuration);
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }
}