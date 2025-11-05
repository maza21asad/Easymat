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
        LevelGrid = new LevelGrid(35, 21);

        snake.Setup(LevelGrid);
        LevelGrid.Setup(snake);
    }

    public static int GetScore()
    {
        return score;
    }

    public static void AddScore()
    {
        score += 1;
    }

    public static void SnakeDied()
    {
        //GameOverWindow.ShowStatic();
    }
}
