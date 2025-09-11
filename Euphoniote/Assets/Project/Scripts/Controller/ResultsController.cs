// _Project/Scripts/UI/ResultsController.cs (图标版最终版)

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class ResultsController : MonoBehaviour
{
    // --- 为了方便在Inspector中设置，我们创建一个小类 ---
    [System.Serializable]
    public class StatDisplay
    {
        public GameObject container; // 整个容器 (e.g., Perfect_Container)
        public TextMeshProUGUI valueText; // 显示数值的文本
        // 图标是静态的，我们不需要在代码里引用它
    }

    [Header("UI 引用")]
    public TextMeshProUGUI titleText;
    public StatDisplay perfectStatDisplay;
    public StatDisplay greatStatDisplay;
    public StatDisplay goodStatDisplay;
    public StatDisplay missStatDisplay;
    public StatDisplay finalScoreDisplay; // 总分也用这个结构
    public Button continueButton;

    [Header("动画参数")]
    public float delayBetweenStats = 0.3f;
    public float numberCrawlDuration = 0.8f;

    void Start()
    {
        // 初始时隐藏所有统计UI
        titleText.gameObject.SetActive(false);
        perfectStatDisplay.container.SetActive(false);
        greatStatDisplay.container.SetActive(false);
        goodStatDisplay.container.SetActive(false);
        missStatDisplay.container.SetActive(false);
        finalScoreDisplay.container.SetActive(false);

        continueButton.interactable = false;
        continueButton.onClick.AddListener(OnContinueClicked);

        StartCoroutine(ShowResultsAnimation());
    }

    private IEnumerator ShowResultsAnimation()
    {
        // 1. 显示标题
        titleText.text = ResultsData.GameWon ? "演奏成功" : "游戏失败";
        titleText.gameObject.SetActive(true);
        // 可以给标题也加一个淡入或放大动画
        yield return new WaitForSeconds(delayBetweenStats * 2);

        // 2. 逐个显示判定统计
        yield return StartCoroutine(AnimateStatDisplay(perfectStatDisplay, ResultsData.PerfectCount));
        yield return new WaitForSeconds(delayBetweenStats);

        yield return StartCoroutine(AnimateStatDisplay(greatStatDisplay, ResultsData.GreatCount));
        yield return new WaitForSeconds(delayBetweenStats);

        yield return StartCoroutine(AnimateStatDisplay(goodStatDisplay, ResultsData.GoodCount));
        yield return new WaitForSeconds(delayBetweenStats);



        yield return StartCoroutine(AnimateStatDisplay(missStatDisplay, ResultsData.MissCount));
        yield return new WaitForSeconds(delayBetweenStats * 2);

        // 3. 显示最终分数
        yield return StartCoroutine(AnimateStatDisplay(finalScoreDisplay, (int)ResultsData.FinalScore));

        // 4. 动画结束，启用按钮
        continueButton.interactable = true;
    }

    /// <summary>
    /// 播放单个统计项的出现和数字滚动动画
    /// </summary>
    private IEnumerator AnimateStatDisplay(StatDisplay display, int targetValue)
    {
        // 激活容器，让图标和文本框一起出现
        display.container.SetActive(true);

        // 可以给容器加一个简单的出现动画
        display.container.transform.localScale = Vector3.zero;
        display.container.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // 等待出现动画
        yield return new WaitForSeconds(0.1f);

        // 开始数字滚动
        float timer = 0f;
        int startValue = 0;

        while (timer < numberCrawlDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / numberCrawlDuration;
            int currentValue = (int)Mathf.Lerp(startValue, targetValue, progress);
            display.valueText.text = currentValue.ToString();
            yield return null;
        }

        // 确保最终显示准确的数值
        display.valueText.text = targetValue.ToString();
    }

    private void OnContinueClicked()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ResultsContinue();
        }
    }
}