using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager_Game : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject exitConfirmPanel;

    [Header("Buttons")]
    public Button settingsButton;
    public Button continueButton;
    public Button exitButton;
    public Button exitYesButton;
    public Button exitNoButton;

    private Stack<GameObject> panelStack = new Stack<GameObject>();
    private bool isPaused = false;

    private void Start()
    {
        settingsButton.onClick.AddListener(OpenSettings);
        continueButton.onClick.AddListener(ResumeGame);
        exitButton.onClick.AddListener(OpenExitConfirm);
        exitYesButton.onClick.AddListener(ExitGame);
        exitNoButton.onClick.AddListener(GoBack);
    }

    // ------------------------------------------------------------------
    // BASIC PANEL SYSTEM (no MenuManager dependency)
    // ------------------------------------------------------------------
    private void ShowPanel(GameObject panel)
    {
        foreach (var p in panelStack)
            p.SetActive(false);

        panel.SetActive(true);
        AnimatePopup(panel);
        panelStack.Push(panel);
    }

    private void ClosePanel(GameObject panel)
    {
        if (panelStack.Count == 0) return;

        GameObject top = panelStack.Pop();

        AnimateClose(top);
        top.SetActive(false);

        if (panelStack.Count > 0)
        {
            GameObject previous = panelStack.Peek();
            previous.SetActive(true);
            AnimatePopup(previous);
        }
    }

    private void GoBack()
    {
        if (panelStack.Count > 1)
        {
            GameObject closingPanel = panelStack.Pop();
            AnimateClose(closingPanel);
            closingPanel.SetActive(false);

            GameObject previous = panelStack.Peek();
            previous.SetActive(true);
            AnimatePopup(previous);
        }
    }

    // ------------------------------------------------------------------
    // ANIMATIONS
    // ------------------------------------------------------------------
    private void AnimatePopup(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        panel.transform.localScale = Vector3.one * 0.4f;
        cg.alpha = 0f;

        panel.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        cg.DOFade(1f, 0.3f);
    }

    private void AnimateClose(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        panel.transform.DOScale(0.6f, 0.25f).SetEase(Ease.InBack);
        cg.DOFade(0f, 0.25f);
    }

    // ------------------------------------------------------------------
    // SETTINGS BUTTON
    // ------------------------------------------------------------------
    private void OpenSettings()
    {
        if (isPaused) return;

        Time.timeScale = 0f;
        isPaused = true;

        ShowPanel(settingsPanel);
    }

    private void ResumeGame()
    {
        StartCoroutine(ResumeAfterDelay());
    }

    private IEnumerator ResumeAfterDelay()
    {
        GameObject panel = settingsPanel;

        AnimateClose(panel);
        yield return new WaitForSecondsRealtime(0.25f);

        ClosePanel(panel);
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // ------------------------------------------------------------------
    // EXIT CONFIRM
    // ------------------------------------------------------------------
    private void OpenExitConfirm()
    {
        ShowPanel(exitConfirmPanel);
    }

    private void ExitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
