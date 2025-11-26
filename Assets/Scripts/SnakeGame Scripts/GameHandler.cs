using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;

    private static int score;

    [SerializeField] private Snake snake;
    [SerializeField] private Text metalAppleWarningText;
    public Canvas mainCanvas;

    private LevelGrid LevelGrid;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LevelGrid = new LevelGrid(16, 30);

        snake.Setup(LevelGrid);
        LevelGrid.Setup(snake);
    }

    private void Update()
    {
        if (LevelGrid != null)
            LevelGrid.Update();
    }


    public static int GetScore()
    {
        return score;
    }

    public static void AddScore(int amount)
    {
        score += amount;
    }

    public static void SnakeDied()
    {
        //GameOverWindow.ShowStatic();
    }

    //New
    public static void ShowMetalWarning(string msg)
    {
        instance.metalAppleWarningText.text = msg;
        instance.metalAppleWarningText.gameObject.SetActive(true);
    }

    //New
    public static void HideMetalWarning()
    {
        instance.metalAppleWarningText.gameObject.SetActive(false);
    }
}
