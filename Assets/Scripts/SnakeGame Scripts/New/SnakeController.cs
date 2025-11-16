using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("References")]
    public GameGrid grid;
    public SnakeInputController input;
    public RectTransform snakeHeadUI;
    public RectTransform bodyContainer;
    public GameObject bodyPrefab;
    public RectTransform foodUI;

    [Header("Speed")]
    public float moveInterval = 0.15f;

    private Vector2Int headGridPos;
    private List<Vector2Int> bodyGrid = new List<Vector2Int>();
    private float moveTimer;

    private void Start()
    {
        InitSnake();
        MoveFood();
    }

    private void InitSnake()
    {
        headGridPos = new Vector2Int(grid.gridWidth / 2, grid.gridHeight / 2);
        bodyGrid.Clear();
        UpdateUI();
    }

    private void Update()
    {
        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            MoveSnake();
            moveTimer = 0f;
        }
    }

    private void MoveSnake()
    {
        Vector2Int newHead = headGridPos + input.CurrentDirection;

        // Hit wall?
        if (!grid.IsInsideGrid(newHead))
        {
            SnakeGameManager.Instance.GameOver();
            return;
        }

        // Hit itself?
        if (bodyGrid.Contains(newHead))
        {
            SnakeGameManager.Instance.GameOver();
            return;
        }

        // Move body
        if (bodyGrid.Count > 0)
        {
            bodyGrid.Insert(0, headGridPos);
            bodyGrid.RemoveAt(bodyGrid.Count - 1);
        }

        headGridPos = newHead;

        // Eat food
        if (newHead == GetFoodGridPos())
        {
            Grow();
            MoveFood();
            SnakeGameManager.Instance.AddScore(1);
        }

        UpdateUI();
    }

    private void Grow()
    {
        bodyGrid.Add(headGridPos);
        Instantiate(bodyPrefab, bodyContainer);
    }

    private Vector2Int GetFoodGridPos()
    {
        Vector2 anchored = foodUI.anchoredPosition;
        int x = Mathf.RoundToInt((anchored.x - grid.cellSize / 2 - grid.gamePanel.rect.xMin) / grid.cellSize);
        int y = Mathf.RoundToInt((anchored.y - grid.cellSize / 2 - grid.gamePanel.rect.yMin) / grid.cellSize);
        return new Vector2Int(x, y);
    }

    private void MoveFood()
    {
        Vector2Int pos = grid.GetRandomPosition();
        foodUI.anchoredPosition = grid.GridToUI(pos);
    }

    private void UpdateUI()
    {
        snakeHeadUI.anchoredPosition = grid.GridToUI(headGridPos);

        for (int i = 0; i < bodyGrid.Count; i++)
        {
            RectTransform seg = bodyContainer.GetChild(i).GetComponent<RectTransform>();
            seg.anchoredPosition = grid.GridToUI(bodyGrid[i]);
        }
    }
}
