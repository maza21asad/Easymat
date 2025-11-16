using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeGameManager : MonoBehaviour
{
    public static SnakeGameManager Instance;

    [Header("UI")]
    public SnakeGameOverUI gameOverUI;
    public SnakeHUDController hud;

    private int score = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddScore(int amount)
    {
        score += amount;
        hud.UpdateScore(score);
    }

    public void GameOver()
    {
        gameOverUI.ShowGameOver();
    }
}
