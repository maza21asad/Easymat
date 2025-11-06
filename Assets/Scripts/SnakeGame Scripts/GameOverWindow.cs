using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button playAgainButton;
    //[SerializeField] private Button exitButton;
    [SerializeField] private Button returnHomeButton;

    [SerializeField] private string returnSceneName = "MainMenu";

    private void Start()
    {
        // Make sure the panel is hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Hook up button events
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(RestartGame);

        //if (exitButton != null)
        //    exitButton.onClick.AddListener(ExitGame);

        if (returnHomeButton != null)
            returnHomeButton.onClick.AddListener(() => LoadScene(returnSceneName));
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Pause the game
        Time.timeScale = 0f;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game!");
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}

