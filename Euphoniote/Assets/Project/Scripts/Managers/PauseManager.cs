// _Project/Scripts/Managers/PauseManager.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.Audio; // 需要这个来控制AudioMixer

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
    public TextMeshProUGUI countdownText;

    [Header("音频设置")]
    public AudioMixer mainAudioMixer; // 拖入你的主AudioMixer
    public string masterVolumeParameter = "MasterVolume"; // AudioMixer中暴露的参数名
    public AudioClip countdownTickSound;
    public AudioClip countdownGoSound;
    private AudioSource uiAudioSource; // 用于播放UI音效

    // 内部状态
    private PlayerInputActions playerInput;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        playerInput = new PlayerInputActions();

        // 为自己添加一个AudioSource组件
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.outputAudioMixerGroup = mainAudioMixer.FindMatchingGroups("UI")[0]; // 假设你有UI混音组
    }

    void Start()
    {
        // 确保UI初始状态正确
        pauseMenuPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);
        IsPaused = false;
        Time.timeScale = 1f; // 确保游戏开始时时间是正常流逝的

        // 绑定按钮事件
        resumeButton.onClick.AddListener(TogglePause);
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        // 绑定滑条事件
        volumeSlider.onValueChanged.AddListener(SetMasterVolume);

        // 初始化滑条的值
        if (PlayerPrefs.HasKey(masterVolumeParameter))
        {
            float savedVolume = PlayerPrefs.GetFloat(masterVolumeParameter);
            volumeSlider.value = savedVolume;
            SetMasterVolume(savedVolume);
        }
        else
        {
            volumeSlider.value = 1f; // 默认满音量
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
        // 如果倒计时正在进行，则不允许切换暂停状态
        if (countdownText.gameObject.activeInHierarchy) return;

        IsPaused = !IsPaused;

        if (IsPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // 暂停游戏时间！
        // 暂停音乐
        if (TimingManager.Instance != null)
        {
            TimingManager.Instance.musicSource.Pause();
        }
    }

    private void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        StartCoroutine(ResumeCountdownCoroutine());
    }

    private IEnumerator ResumeCountdownCoroutine()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        if (countdownTickSound != null) uiAudioSource.PlayOneShot(countdownTickSound);
        yield return new WaitForSecondsRealtime(1f); // 使用真实时间，不受Time.timeScale影响

        countdownText.text = "2";
        if (countdownTickSound != null) uiAudioSource.PlayOneShot(countdownTickSound);
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "1";
        if (countdownTickSound != null) uiAudioSource.PlayOneShot(countdownTickSound);
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);
        if (countdownGoSound != null) uiAudioSource.PlayOneShot(countdownGoSound);

        Time.timeScale = 1f; // 恢复游戏时间！
        // 恢复音乐
        if (TimingManager.Instance != null)
        {
            TimingManager.Instance.musicSource.UnPause();
        }
    }

    public void RestartGame()
    {
        // 确保在加载场景前恢复时间流逝，否则场景加载可能会出问题
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
        // 在编辑器中，上面这行可能无效，可以加上下面这行
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void SetMasterVolume(float value)
    {
        // AudioMixer使用对数单位(dB)，范围通常是-80到20
        // 我们需要将滑条的线性值(0-1)转换为对数dB值
        // Mathf.Log10(value) * 20 是一个标准的转换公式
        mainAudioMixer.SetFloat(masterVolumeParameter, Mathf.Log10(value) * 20);
        // 保存设置，以便下次启动游戏时加载
        PlayerPrefs.SetFloat(masterVolumeParameter, value);
    }
}