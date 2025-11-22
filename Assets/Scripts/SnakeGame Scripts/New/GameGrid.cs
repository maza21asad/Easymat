using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 12;
    public int gridHeight = 24;
    public float cellSize = 40f;

    [Header("References")]
    public RectTransform gamePanel;

    private Vector2 panelOrigin;

    private void Awake()
    {
        AutoFitGridToPanel();    // ← First fit grid to panel size
        CalculateOrigin();       // ← THEN calculate origin (very important!)
    }

    private void OnRectTransformDimensionsChange()
    {
        // If device rotates or resolution changes
        AutoFitGridToPanel();
        CalculateOrigin();
    }
   
    public Vector2 GridToUI(Vector2Int gridPos)
    {
        return new Vector2(
            panelOrigin.x + gridPos.x * cellSize + cellSize / 2f,
            panelOrigin.y + gridPos.y * cellSize + cellSize / 2f
        );
    }

    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth &&
               pos.y >= 0 && pos.y < gridHeight;
    }

    public Vector2Int GetRandomPosition()
    {
        return new Vector2Int(
            Random.Range(0, gridWidth),
            Random.Range(0, gridHeight)
        );
    }

    public void AutoFitGridToPanel()
    {
        if (gamePanel == null) return;

        float panelWidth = gamePanel.rect.width;
        float panelHeight = gamePanel.rect.height;

        float cellWidth = panelWidth / gridWidth;
        float cellHeight = panelHeight / gridHeight;

        // Choose smaller so grid always fits inside the panel
        cellSize = Mathf.Floor(Mathf.Min(cellWidth, cellHeight));
    }

    private void CalculateOrigin()
    {
        panelOrigin = -new Vector2(
            (gridWidth * cellSize) / 2f,
            (gridHeight * cellSize) / 2f
        );
    }
}
