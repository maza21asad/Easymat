using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    private static GameHandler instance;

    private static int score;

    //public static Text metalAppleWarningText; //New
    [SerializeField] private Text metalAppleWarningText; // FIXED (not static)

    [SerializeField] private Snake snake;
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

    // ==================================================== added new ============================================================
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
