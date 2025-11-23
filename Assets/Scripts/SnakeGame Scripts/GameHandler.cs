using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    private static GameHandler instance;

    private static int score;

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
}
