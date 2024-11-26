using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject startScreenCanvas;
    [SerializeField] private GameObject gameCanvas;

    private void Start()
    {
        startScreenCanvas.SetActive(true);
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false); 
        }
        Time.timeScale = 0; 
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
}
