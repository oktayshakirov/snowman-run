using UnityEngine;
using TMPro;

public class StartScreenManager : MonoBehaviour
{
    public static StartScreenManager Instance;

    [SerializeField] private GameObject startScreenCanvas;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject settingsScreenCanvas;
    [SerializeField] private GameObject shopScreenCanvas;
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private TMP_Text currentLevelText;

    [Header("Camera Reference")]
    [SerializeField] private PreviewCameraController previewCamera;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowStartScreen();
    }

    public void ShowStartScreen()
    {
        startScreenCanvas.SetActive(true);
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false);
        }

        if (settingsScreenCanvas != null)
        {
            settingsScreenCanvas.SetActive(false);
        }

        if (shopScreenCanvas != null)
        {
            shopScreenCanvas.SetActive(false);
        }

        Time.timeScale = 0;
        UpdateUI();

        if (previewCamera != null)
        {
            previewCamera.ResetCameraPosition();
            previewCamera.MoveCameraToPreviewPosition();
        }
    }

    public void StartGame()
    {
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(true);
        }

        startScreenCanvas.SetActive(false);
        if (settingsScreenCanvas != null)
        {
            settingsScreenCanvas.SetActive(false);
        }

        if (shopScreenCanvas != null)
        {
            shopScreenCanvas.SetActive(false);
        }

        AudioManager.Instance.PlaySound(AudioManager.SoundType.Wee);
        Time.timeScale = 1;
        GameManager.inst.StartNewGame();
    }

    public void OpenSettings()
    {
        if (settingsScreenCanvas != null)
        {
            settingsScreenCanvas.SetActive(true);
        }

        startScreenCanvas.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsScreenCanvas != null)
        {
            settingsScreenCanvas.SetActive(false);
        }

        startScreenCanvas.SetActive(true);
    }

    public void OpenShop()
    {
        if (shopScreenCanvas != null)
        {
            shopScreenCanvas.SetActive(true);
        }
        startScreenCanvas.SetActive(false);
    }

    private void UpdateUI()
    {
        int totalCoins = WalletManager.GetTotalCoins();
        totalCoinsText.text = $"{totalCoins}";
        int currentLevel = PlayerPrefs.GetInt("UnlockedLevels", 1);
        currentLevelText.text = $"Level: {currentLevel}";
    }
}
