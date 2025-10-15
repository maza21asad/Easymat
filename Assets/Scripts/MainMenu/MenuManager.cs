using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public List<GameObject> allPanels;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    [Header("UFO Settings")]
    public Transform ufo;
    public float ufoFlyDuration = 1.2f;
    //public Ease ufoFlyEase = Ease.InBounce;
    public Ease ufoFlyEase = Ease.InOutQuad;

    private Vector3 ufoStartPosition;
    private Vector3 ufoStartScale;

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

        if (ufo != null)
        {
            ufoStartPosition = ufo.GetComponent<RectTransform>().anchoredPosition;
            ufoStartScale = ufo.localScale;
        }

        ShowPanel(mainMenuPanel);
    }

    private void ResetUFOPosition()
    {
        if (ufo != null)
        {
            RectTransform ufoRect = ufo.GetComponent<RectTransform>();
            ufoRect.anchoredPosition = ufoStartPosition;
            ufo.localScale = ufoStartScale;
            ufo.gameObject.SetActive(true);
        }
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

    public void UFOFlyIntoButton(GameObject targetPanel)
    {
        if (ufo == null || targetPanel == null) return;

        // Detect which button triggered the click
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        if (clickedButton == null) return;

        Transform targetButton = clickedButton.transform;

        // Make sure UFO is visible
        ufo.gameObject.SetActive(true);

        // Stop any previous animation
        ufo.DOKill();

        // Create the flying sequence
        Sequence seq = DOTween.Sequence();

        // Move UFO toward the button’s position
        RectTransform ufoRect = ufo.GetComponent<RectTransform>();
        RectTransform buttonRect = targetButton.GetComponent<RectTransform>();

        // Animate UFO to the button’s anchored position
        seq.Append(ufoRect.DOAnchorPos(buttonRect.anchoredPosition, ufoFlyDuration).SetEase(ufoFlyEase));

        // Optionally scale UFO to simulate “entering” the button
        seq.Join(ufo.DOScale(0.2f, ufoFlyDuration * 1f));

        // After UFO disappears → show the new panel
        seq.OnComplete(() =>
        {
            ufo.localScale = Vector3.one; // Reset for next time
            ufo.gameObject.SetActive(false);

            // Show the next panel
            ShowPanel(targetPanel);
        });
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

                    // If returning to main menu or a root panel, reset UFO
                    if (previous == mainMenuPanel)
                    {
                        ResetUFOPosition();
                    }
                }); 
        }
    }


    // Load another scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
