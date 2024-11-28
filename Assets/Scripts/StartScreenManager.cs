using UnityEngine;
using TMPro;

public class StartScreenManager : MonoBehaviour
{
    public static StartScreenManager Instance;

    [SerializeField] private GameObject startScreenCanvas;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private TMP_Text totalCoinsText;

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

        Time.timeScale = 0;
        UpdateTotalCoinsUI();
    }

    public void StartGame()
    {
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(true);
        }

        startScreenCanvas.SetActive(false);
        Time.timeScale = 1;


    }

    private void UpdateTotalCoinsUI()
    {
        int totalCoins = WalletManager.GetTotalCoins();
        totalCoinsText.text = $"{totalCoins}";
    }
}
