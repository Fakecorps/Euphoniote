// _Project/Scripts/Managers/UIManager.cs (最终优化版)

using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 元素引用")]
    public TextMeshProUGUI comboText;

    [Header("Combo 动画设置")]
    public float comboPunchScale = 1.2f;
    public float comboAnimationDuration = 0.1f;

    private Coroutine comboAnimationCoroutine;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        if (comboText != null)
        {
            // 游戏开始时，确保Combo文本是隐藏的
            comboText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        StatsManager.OnComboChanged += HandleComboChanged;
        StatsManager.OnComboBroken += HandleComboBroken;
    }

    private void OnDisable()
    {
        StatsManager.OnComboChanged -= HandleComboChanged;
        StatsManager.OnComboBroken -= HandleComboBroken;
    }

    /// <summary>
    /// 当Combo数值变化时调用。负责显示和更新。
    /// </summary>
    private void HandleComboChanged(int newCombo)
    {
        if (comboText == null) return;

        // 【核心修改】
        // 只要Combo大于0，我们就更新文本。
        // 显示与否的逻辑由具体的数值决定。
        comboText.text = $"COMBO\n{newCombo}";

        // Combo大于等于2时才显示，并播放动画
        if (newCombo >= 2)
        {
            if (!comboText.gameObject.activeInHierarchy)
            {
                comboText.gameObject.SetActive(true);
            }

            if (comboAnimationCoroutine != null) StopCoroutine(comboAnimationCoroutine);
            comboAnimationCoroutine = StartCoroutine(PunchScaleAnimation(comboText.transform));
        }
        else if (newCombo == 1)
        {
            // Combo为1时不显示，但文本内容已经更新
            comboText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 当Combo中断时调用。负责隐藏。
    /// </summary>
    private void HandleComboBroken()
    {
        if (comboText == null) return;

        // 【核心修改】
        // 中断时，立即隐藏文本，并确保动画停止且缩放复位。
        if (comboAnimationCoroutine != null)
        {
            StopCoroutine(comboAnimationCoroutine);
            comboText.transform.localScale = Vector3.one;
        }
        comboText.gameObject.SetActive(false);
    }

    private IEnumerator PunchScaleAnimation(Transform targetTransform)
    {
        Vector3 originalScale = Vector3.one;
        float halfDuration = comboAnimationDuration / 2f;
        float timer = 0f;

        // 放大
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(originalScale.x, comboPunchScale, timer / halfDuration);
            targetTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        // 复原
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(comboPunchScale, originalScale.x, timer / halfDuration);
            targetTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }
}