// _Project/Scripts/Managers/PauseManager.cs (最终修改版 - 图片倒计时 & 返回选关)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System;
using UnityEngine.Audio;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public static bool IsPaused { get; private set; } = false;

    [Header("UI 引用")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;
    public Slider volumeSlider;
    public Image blackenImage; // 我保留了这个引用，以防你需要它

    [Header("倒计时UI (图片版)")] // --- 修改部分 ---
    public Image countdownImage;        // 拖入新的 CountdownImage
    public Sprite countdownSprite3;     // 拖入 "3" 的图片
    public Sprite countdownSprite2;     // 拖入 "2" 的图片
    public Sprite countdownSprite1;     // 拖入 "1" 的图片

    [Header("音频设置")]
    public AudioMixer mainAudioMixer;
    public string masterVolumeParameter = "MasterVolume";
    public AudioClip countdownTickSound;
    public AudioClip countdownGoSound;
    private AudioSource uiAudioSource;

    public static event Action<bool> OnPauseStateChanged;
    private PlayerInputActions playerInput;

    public static bool IsCountingDown { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
        playerInput = new PlayerInputActions();
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        if (mainAudioMixer != null)
        {
            AudioMixerGroup[] uiGroups = mainAudioMixer.FindMatchingGroups("UI");
            if (uiGroups.Length > 0) uiAudioSource.outputAudioMixerGroup = uiGroups[0];
            else Debug.LogError("在 AudioMixer 中找不到名为 'UI' 的混音组！", this.gameObject);
        }
    }

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        if (countdownImage != null) countdownImage.gameObject.SetActive(false); // --- 修改部分 ---
        if (blackenImage != null) blackenImage.gameObject.SetActive(false);
        IsPaused = false;
        Time.timeScale = 1f;

        resumeButton.onClick.AddListener(TogglePause);
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitToLevelSelect); // --- 修改部分 ---
        volumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (PlayerPrefs.HasKey(masterVolumeParameter))
        {
            float savedVolume = PlayerPrefs.GetFloat(masterVolumeParameter);
            volumeSlider.value = savedVolume;
            SetMasterVolume(savedVolume);
        }
        else
        {
            volumeSlider.value = 1f;
        }
    }

    private void OnEnable()
    {
        playerInput.Gameplay.Enable();
        playerInput.Gameplay.Pause.performed += _ => TogglePause();
    }

    private void OnDisable()
    {
        playerInput.Gameplay.Disable();
    }

    public void TogglePause()
    {

        if (IsCountingDown) return;

        if (countdownImage != null && countdownImage.gameObject.activeInHierarchy) return; // --- 修改部分 ---

        IsPaused = !IsPaused;
        OnPauseStateChanged?.Invoke(IsPaused);

        if (IsPaused) { PauseGame(); }
        else { ResumeGame(); }
    }

    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        if (blackenImage != null) blackenImage.gameObject.SetActive(true);
        Time.timeScale = 0f;
        if (TimingManager.Instance != null) TimingManager.Instance.musicSource.Pause();
    }

    private void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        if (blackenImage != null) blackenImage.gameObject.SetActive(false);
        StartCoroutine(ResumeCountdownCoroutine());
    }

    // --- 核心修改：倒计时协程 ---
    private IEnumerator ResumeCountdownCoroutine()
    {

        IsCountingDown = true;

        if (countdownImage == null)
        {
            // 如果没有设置倒计时图片，直接恢复游戏以防卡住
            Debug.LogWarning("倒计时图片未设置，直接恢复游戏。");
            Time.timeScale = 1f;
            if (TimingManager.Instance != null) TimingManager.Instance.musicSource.UnPause();
            yield break;
        }

        countdownImage.gameObject.SetActive(true);

        // 3
        countdownImage.sprite = countdownSprite3;
        countdownImage.SetNativeSize(); // 自动调整Image大小以匹配图片原始比例
        if (countdownTickSound != null) uiAudioSource.PlayOneShot(countdownTickSound);
        yield return new WaitForSecondsRealtime(1f);

        // 2
        countdownImage.sprite = countdownSprite2;
        countdownImage.SetNativeSize();
        if (countdownTickSound != null) uiAudioSource.PlayOneShot(countdownTickSound);
        yield return new WaitForSecondsRealtime(1f);

        // 1
        countdownImage.sprite = countdownSprite1;
        countdownImage.SetNativeSize();
        if (countdownTickSound != null) uiAudioSource.PlayOneShot(countdownTickSound);
        yield return new WaitForSecondsRealtime(1f);

        countdownImage.gameObject.SetActive(false);
        if (countdownGoSound != null) uiAudioSource.PlayOneShot(countdownGoSound);

        Time.timeScale = 1f;
        if (TimingManager.Instance != null) TimingManager.Instance.musicSource.UnPause();

        IsCountingDown = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToLevelSelect()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        OnPauseStateChanged?.Invoke(IsPaused);
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToLevelSelect();
        }
        else
        {
            SceneManager.LoadScene("1_LevelSelect");
        }
    }

    public void SetMasterVolume(float value)
    {
        mainAudioMixer.SetFloat(masterVolumeParameter, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(masterVolumeParameter, value);
    }
}