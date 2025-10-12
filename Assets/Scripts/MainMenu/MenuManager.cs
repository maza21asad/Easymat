using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public List<GameObject> allPanels;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Show a specific panel and hide others
    public void ShowPanel(GameObject panelToShow)
    {
        foreach (GameObject panel in allPanels)
        {
            panel.SetActive(false);
        }
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            panelHistory.Push(panelToShow);
        }
    }

    // Close current panel (for cross or exit button)
    public void CloseCurrentPanel()
    {
        if (panelHistory.Count > 0)
        {
            // Pop the current panel
            panelHistory.Pop();
            // Show the previous panel
            if (panelHistory.Count > 0)
            {
                GameObject previousPanel = panelHistory.Peek();
                ShowPanel(previousPanel);
            }
            else
            {
                // If there's no previous panel, show the main menu
                ShowPanel(mainMenuPanel);
            }
        }
        else
        {
            // If there's no panel in history, show the main menu
            ShowPanel(mainMenuPanel);
        }
    }

    // Go back to the previous panel
    public void GoBack()
    {
        if (panelHistory.Count > 1)
        {
            // Pop the current panel
            panelHistory.Pop();
            // Show the previous panel
            GameObject previousPanel = panelHistory.Peek();
            ShowPanel(previousPanel);
        }
        else
        {
            // If there's no previous panel, show the main menu
            ShowPanel(mainMenuPanel);
        }
    }

    // Load another scene
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
