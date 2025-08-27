// _Project/Scripts/Managers/UIManager.cs (最终修改版 - 增加分数显示)

using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("核心UI元素")]
    public UnityEngine.UI.Image healthBarFill;  // 拖入 HealthBar_Fill
    public TextMeshProUGUI scoreText;         // --- 新增：拖入 ScoreText ---

    [Header("判定反馈UI")]
    public UnityEngine.UI.Image judgmentImage;
    public TextMeshProUGUI judgmentComboText;

    [Header("判定Sprite映射")]
    public List<JudgmentSpriteMapping> judgmentSpriteMappings;

    [Header("动画参数")]
    public float judgmentPunchScale = 1.5f;
    public float judgmentFadeInDuration = 0.05f;
    public float judgmentHoldDuration = 0.3f;
    public float judgmentFadeOutDuration = 0.4f;
    public float comboPunchScale = 1.2f;
    public float comboAnimationDuration = 0.1f;

    // 内部变量
    private Coroutine judgmentImageAnimationCoroutine;
    private Coroutine comboAnimationCoroutine;
    private Dictionary<JudgmentType, Sprite> judgmentSpriteDict;
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
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }
    }

    private void OnEnable()
    {
        // 订阅所有需要的事件
        StatsManager.OnComboChanged += HandleComboChanged;
        StatsManager.OnComboBroken += HandleComboBroken;
        JudgmentManager.OnNoteJudged += HandleJudgmentFeedback;
        StatsManager.OnHealthChanged += HandleHealthChanged;
        StatsManager.OnScoreChanged += HandleScoreChanged; // --- 新增：订阅分数变化事件 ---
    }

    private void OnDisable()
    {
        // 取消所有订阅
        StatsManager.OnComboChanged -= HandleComboChanged;
        StatsManager.OnComboBroken -= HandleComboBroken;
        JudgmentManager.OnNoteJudged -= HandleJudgmentFeedback;
        StatsManager.OnHealthChanged -= HandleHealthChanged;
        StatsManager.OnScoreChanged -= HandleScoreChanged; // --- 新增：取消订阅 ---
    }

    // --- 新增的核心方法 ---
    /// <summary>
    /// 当分数发生变化时由 StatsManager 调用
    /// </summary>
    /// <param name="newScore">新的总分数</param>
    private void HandleScoreChanged(long newScore)
    {
        if (scoreText == null) return;

        // 将分数格式化为7位数的字符串，不足的前面补0
        // 例如：123 -> "0000123"
        scoreText.text = newScore.ToString("D7");
    }


    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;
        float fillAmount = currentHealth / maxHealth;
        healthBarFill.fillAmount = fillAmount;
    }

    // --- 以下是您已有的其他UI逻辑，保持不变 ---
    private void HandleJudgmentFeedback(JudgmentResult result)
    {
        if (result.Type == JudgmentType.HoldHead) return;
        if (judgmentSpriteDict.TryGetValue(result.Type, out Sprite spriteToShow))
        {
            if (judgmentImageAnimationCoroutine != null) StopCoroutine(judgmentImageAnimationCoroutine);
            judgmentImageAnimationCoroutine = StartCoroutine(JudgmentImageAnimation(spriteToShow));
        }
    }

    private void HandleComboChanged(int newCombo)
    {
        if (judgmentComboText == null) return;
        judgmentComboText.text = newCombo.ToString();
        if (newCombo >= 2)
        {
            if (!judgmentComboText.gameObject.activeInHierarchy) judgmentComboText.gameObject.SetActive(true);
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
        judgmentImage.sprite = sprite;
        judgmentImage.gameObject.SetActive(true);
        CanvasGroup imageCG = judgmentImage.GetComponent<CanvasGroup>();
        if (imageCG == null) imageCG = judgmentImage.gameObject.AddComponent<CanvasGroup>();
        Vector3 startScale = initialJudgmentImageScale * judgmentPunchScale;
        Vector3 endScale = initialJudgmentImageScale;
        judgmentImage.transform.localScale = startScale;

        float timer = 0f;
        while (timer < judgmentFadeInDuration)
        {
            timer += Time.deltaTime;
            imageCG.alpha = Mathf.Lerp(0, 1, timer / judgmentFadeInDuration);
            yield return null;
        }
        imageCG.alpha = 1;

        yield return new WaitForSeconds(judgmentHoldDuration);

        timer = 0f;
        while (timer < judgmentFadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / judgmentFadeOutDuration;
            imageCG.alpha = Mathf.Lerp(1, 0, progress);
            judgmentImage.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }

        judgmentImage.gameObject.SetActive(false);
        judgmentImage.transform.localScale = initialJudgmentImageScale;
    }

    private IEnumerator PunchScaleAnimation(Transform targetTransform, Vector3 originalScale, float punchMultiplier)
    {
        Vector3 punchScale = originalScale * punchMultiplier;
        float halfDuration = comboAnimationDuration / 2f;
        float timer = 0f;

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