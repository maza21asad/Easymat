using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

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
        ShowPanel(mainMenuPanel);
    }

    private void AnimatePanelFade(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        panel.transform.localScale = Vector3.one * 0.8f;
        canvasGroup.alpha = 0f;
        panel.SetActive(true);

        // Animate both scale & fade
        panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, 0.3f);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        foreach (GameObject panel in allPanels)
            panel.SetActive(false);

        if (panelToShow != null)
        {
            AnimatePanelFade(panelToShow);
            panelHistory.Push(panelToShow);
        }
    }

    public void CloseCurrentPanel()
    {
        if (panelHistory.Count > 0)
        {
            GameObject closedPanel = panelHistory.Pop();

            // Animate closing before deactivating
            closedPanel.transform.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .OnComplete(() => closedPanel.SetActive(false));

            if (panelHistory.Count > 0)
            {
                GameObject previousPanel = panelHistory.Peek();
                AnimatePanelFade(previousPanel);
            }
            else
            {
                AnimatePanelFade(mainMenuPanel);
                panelHistory.Push(mainMenuPanel);
            }
        }
    }

    public void GoBack()
    {
        if (panelHistory.Count > 1)
        {
            GameObject current = panelHistory.Pop();
            GameObject previous = panelHistory.Peek();

            // Animate current panel closing
            CanvasGroup currentGroup = current.GetComponent<CanvasGroup>();
            if (currentGroup == null)
                currentGroup = current.AddComponent<CanvasGroup>();

            current.transform.DOScale(0.8f, 0.25f)
                .SetEase(Ease.InBack);

            currentGroup.DOFade(0f, 0.25f)
                .OnComplete(() =>
                {
                    current.SetActive(false);

                    // Animate the previous panel back in
                    AnimatePanelFade(previous);
                });
        }
    }


    // Load another scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
